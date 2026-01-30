Shader "Custom/BalatroDisplacementUpdateURP"
{
    Properties
    {
        _DispTex ("Previous Displacement (RG)", 2D) = "black" {}
        _Decay ("Decay (0..1)", Range(0,1)) = 0.985
        _Inject ("Injection Strength", Range(0,2)) = 1.0
        _Advect ("Advection Strength", Range(0,2)) = 0.6

        _MaxSources ("Max Sources", Int) = 8
        _RectAspect ("Rect Aspect (width/height)", Float) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Name "UpdateDisplacement"
            ZWrite Off
            ZTest Always
            Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

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

            TEXTURE2D(_DispTex);
            SAMPLER(sampler_DispTex);

            float _Decay;
            float _Inject;
            float _Advect;

            int _MaxSources;
            float _RectAspect;

            float4 _RippleSources[32];
            float4 _RippleParams[32];

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float2 DecodeDisp(float4 rg)
            {
                return rg.xy * 2.0 - 1.0;
            }

            float2 EncodeDisp(float2 d)
            {
                d = clamp(d, -1.0, 1.0);
                return d * 0.5 + 0.5;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float t = _Time.y;

                // === 1. ADVECT THE FIELD ===
                float4 prev = SAMPLE_TEXTURE2D(_DispTex, sampler_DispTex, uv);
                float2 velocity = DecodeDisp(prev);

                // --- Symmetric advection (RK2 / midpoint) ---
                float2 v0 = velocity;

                // first backtrace
                float2 uv1 = uv - v0 * _Advect * 0.5;
                uv1 = clamp(uv1, 0.001, 0.999);

                float2 v1 = DecodeDisp(
                    SAMPLE_TEXTURE2D(_DispTex, sampler_DispTex, uv1)
                );

                // second backtrace using midpoint velocity
                float2 backUV = uv - v1 * _Advect;
                backUV = clamp(backUV, 0.001, 0.999);

                float2 newVelocity = DecodeDisp(
                    SAMPLE_TEXTURE2D(_DispTex, sampler_DispTex, backUV)
                );

                // === 2. RIPPLE INJECTION (adds velocity) ===
                float2 duv = float2(_RectAspect, 1.0);
                float2 injection = 0;

                [loop]
                for (int i = 0; i < _MaxSources; i++)
                {
                    float4 src = _RippleSources[i];
                    float4 prm = _RippleParams[i];

                    float dt = t - src.z;
                    if (dt < 0) continue;

                    float2 d = (uv - src.xy) * duv;
                    float dist = length(d);

                    float travel = dist - prm.y * dt;
                    float wave   = sin(travel * prm.x);
                    float ring   = saturate(1.0 - abs(travel) / prm.w);
                    float atten  = 1.0 / (1.0 + dist * dist * 6.0 + dt * prm.z);

                    float strength = src.w * wave * ring * atten;
                    float2 dir = (dist > 1e-4) ? d / dist : float2(0,0);

                    injection += dir * strength;
                }

                // === 3. UPDATE RULE (IRREVERSIBLE) ===
                newVelocity *= _Decay;
                newVelocity += injection * _Inject;

                float2 packed = EncodeDisp(newVelocity);
                return float4(packed, 0, 1);
            }

            ENDHLSL
        }
    }
}
