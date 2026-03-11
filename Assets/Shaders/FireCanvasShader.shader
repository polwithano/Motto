Shader "Custom/FireCanvasShader"
{
    Properties
    {
        _NoiseTex ("Noise Texture", 2D) = "white" {}

        _BottomColor ("Bottom Color", Color) = (0, 0.7, 1, 1)
        _MiddleColor ("Middle Color", Color) = (1, 0.5, 0, 1)
        _TopColor ("Top Color", Color) = (1, 0.03, 0.001, 1)

        _FireAlpha ("Fire Alpha", Range(0,1)) = 1
        _FireSpeed ("Fire Speed", Vector) = (0,2,0,0)
        _FireAperture ("Fire Aperture", Range(0,3)) = 0.22
        _CelSteps ("Cel Steps", Range(2,8)) = 4
        _OutlineStrength ("Outline Strength", Range(0,10)) = 4
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;

            float4 _BottomColor;
            float4 _MiddleColor;
            float4 _TopColor;

            float _FireAlpha;
            float2 _FireSpeed;
            float _FireAperture;
            float _CelSteps;
            float _OutlineStrength;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _NoiseTex);
                return OUT;
            }

            float posterize(float v, float steps)
            {
                return floor(v * steps) / steps;
            }

            float4 tri_color_mix_cel(float4 c1, float4 c2, float4 c3, float pos, float steps)
            {
                // Posterize la position -> sauts de couleur nets
                pos = saturate(pos);
                pos = posterize(pos, steps);

                if (pos < 0.5)
                    return lerp(c1, c2, saturate(pos * 2.0));
                else
                    return lerp(c2, c3, saturate((pos - 0.5) * 2.0));
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float time = _Time.y;

                float2 uv1 = uv + time * _FireSpeed;
                float2 uv2 = uv + time * _FireSpeed * 1.5;

                float n1 = tex2D(_NoiseTex, frac(uv1)).r;
                float n2 = tex2D(_NoiseTex, frac(uv2)).r;

                float combined = (n1 + n2) * 0.5;

                // ✅ Posterize le noise -> formes en blocs
                combined = posterize(combined, _CelSteps);

                float noise = uv.y * (((uv.y + _FireAperture) * combined - _FireAperture) * 75.0);
                noise += sin(uv.y * 10.0 + time * 2.0) * 0.1;

                float gradient_pos = clamp(noise * 0.08, 0.3, 1.0);

                // ✅ Couleurs en paliers nets
                float4 color = tri_color_mix_cel(_BottomColor, _MiddleColor, _TopColor, gradient_pos, _CelSteps);

                // ✅ Outline sur les bords de flamme
                float edge = fwidth(noise) * _OutlineStrength;
                color.rgb = lerp(color.rgb, float3(0.05, 0.02, 0.0), saturate(edge));

                color.a = saturate(noise) * _FireAlpha;

                return color;
            }

            ENDHLSL
        }
    }
}