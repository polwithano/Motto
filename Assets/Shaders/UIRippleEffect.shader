Shader "UI/RippleMultiSource"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // Ripple global tuning
        _MaxSources ("Max Sources", Int) = 8
        _GlobalStrength ("Global Strength", Range(0, 0.1)) = 0.02
        _AspectCorrect ("Aspect Correct (1=yes)", Float) = 1
        _RectAspect ("Rect Aspect (width/height)", Float) = 1
        _RippleVisibility ("Ripple Visibility", Range(0, 2)) = 1

        // UI required
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [HideInInspector] _ClipRect ("Clip Rect", Vector) = (-32767,-32767,32767,32767)
        [HideInInspector] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;

            float4 _ClipRect;
            float _UseUIAlphaClip;

            // Ripple globals
            int _MaxSources;
            float _RippleVisibility;
            float _GlobalStrength;
            float _AspectCorrect;
            float _RectAspect;

            // Arrays (set from script)
            // xy = pos (uv), z = startTime, w = amplitude
            float4 _RippleSources[32];
            // x = frequency, y = speed, z = damping, w = width
            float4 _RippleParams[32];

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPos = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // UI clipping
                float2 pixelPos = i.worldPos.xy;
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                float embossSum = 0;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(pixelPos, _ClipRect);
                #endif

                // Early out if fully transparent (optional)
                // if (col.a <= 0.001) return col;

                float t = _Time.y;

                // Aspect correction so circles stay circles in non-square rects.
                // We approximate using screen aspect; for perfect, pass rect ratio from script.
                float2 uv = i.uv;
                float2 duv = float2(_RectAspect, 1.0);

                float2 offset = 0;

                // Sum all ripples
                [loop]
                for (int idx = 0; idx < _MaxSources; idx++)
                {
                    float4 src = _RippleSources[idx];
                    float4 prm = _RippleParams[idx];

                    float2 p = src.xy;
                    float startTime = src.z;
                    float amp = src.w;

                    float freq = prm.x;
                    float speed = prm.y;
                    float damp = prm.z;
                    float width = prm.w;

                    float dt = t - startTime;
                    if (dt < 0) continue;

                    float2 d = (uv - p) * duv;
                    float dist = length(d);

                    // Distance relative to propagation
                    float travel = dist - speed * dt;

                    // Train d’ondes périodiques
                    float wave = sin(travel * freq);

                    // Masque circulaire : garde seulement les crêtes visibles
                    float ring = saturate(1.0 - abs(travel) / width);

                    // Atténuation douce (mobile friendly)
                    float attenuation = 1.0 / (1.0 + dist * dist * 6.0 + dt * damp);

                    // Force finale
                    float strength = amp * wave * ring * attenuation;

                    // Relief signal (used later for shading)
                    float emboss = wave * ring * attenuation;
                    embossSum += emboss * amp;
                                        
                    // Displace along radial direction
                    float2 dir = (dist > 1e-4) ? (d / dist) : float2(0,0);
                    offset += dir * strength;
                }

                offset *= _GlobalStrength * _RippleVisibility;

                float2 uv2 = uv + offset;

                fixed4 outCol = tex2D(_MainTex, uv2) * i.color;

                // --- Subtle depth shading for white UI ---
                float3 lightDir = normalize(float3(-0.4, 0.6, 0.7)); // top-left light

                // Fake normal from ripple direction
                float3 fakeNormal = normalize(float3(offset.xy * 40.0, 1.0));

                // Lighting term
                float lighting = dot(fakeNormal, lightDir);

                // Final emboss contribution
                float depth = embossSum * lighting;

                // Apply as subtle monochrome shading
                outCol.rgb += depth * 0.08 * _RippleVisibility;

                #ifdef UNITY_UI_CLIP_RECT
                outCol.a *= UnityGet2DClipping(pixelPos, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                if (_UseUIAlphaClip > 0.5) clip(outCol.a - 0.001);
                #endif

                return outCol;
            }
            ENDCG
        }
    }
}
