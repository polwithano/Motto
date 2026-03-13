Shader "Custom/Background/CheckerboardSingleRippleURP"
{
    Properties
    {
        // ── Checkerboard ─────────────────────────────────────────────────────
        _Scale          ("Checker Scale",       Float)        = 8
        _Rotation       ("Rotation",            Float)        = 0
        _Speed          ("Animation Speed",     Float)        = 0.4
        _LineWobble     ("Line Wobble",         Range(0,1))   = 0.25
        _LineFrequency  ("Line Frequency",      Float)        = 3
        _ColorA         ("Color A",             Color)        = (0.95, 0.92, 0.85, 1)
        _ColorB         ("Color B",             Color)        = (0.65, 0.80, 1.00, 1)
        _OutlineWidth   ("Outline Width",       Range(0,0.5)) = 0.08
        _OutlineSoft    ("Outline Softness",    Range(0,0.5)) = 0.05
        _OutlineColor   ("Outline Color",       Color)        = (0.05, 0.05, 0.08, 1)
        _OutlineStrength("Outline Strength",    Range(0,2))   = 1
        _MoireStrength  ("Moire Strength",      Range(0,1))   = 0.25
        _MoireFrequency ("Moire Frequency",     Float)        = 18
        _MoireSpeed     ("Moire Speed",         Float)        = 0.2
        _MorphDiamond   ("Morph to Diamond",    Range(0,1))   = 0.0

        // ── Ripple system ────────────────────────────────────────────────────
        _MaxSources      ("Max Sources",        Int)          = 8
        _GlobalStrength  ("Global Strength",    Range(0,0.1)) = 0.02
        _RippleVisibility("Ripple Visibility",  Range(0,2))   = 1
        _RectAspect      ("Rect Aspect (W/H)",  Float)        = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"     = "Opaque"
            "Queue"          = "Geometry"
        }

        Pass
        {
            ZWrite On
            ZTest  LEqual
            Cull   Off

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            // ── Uniforms ─────────────────────────────────────────────────────
            float  _Scale, _Rotation, _Speed, _LineWobble, _LineFrequency;
            float4 _ColorA, _ColorB, _OutlineColor;
            float  _OutlineWidth, _OutlineSoft, _OutlineStrength;
            float  _MoireStrength, _MoireFrequency, _MoireSpeed;
            float  _MorphDiamond;

            int    _MaxSources;
            float  _GlobalStrength, _RippleVisibility, _RectAspect;
            float4 _RippleSources[32];
            float4 _RippleParams[32];

            // ── Vertex ───────────────────────────────────────────────────────
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            // ── Pattern helpers ──────────────────────────────────────────────
            float2 MorphSquareToDiamond(float2 uv, float morph)
            {
                float2 d = float2(uv.x + uv.y, uv.x - uv.y);
                return lerp(uv, d, morph);
            }

            float2 TrippyOffset(float2 uv, float t, float freq, float wobble)
            {
                return uv + float2(sin(uv.y * freq + t)       * wobble,
                                   sin(uv.x * freq - t * 1.3) * wobble);
            }

            float CheckerWithOutline(float2 uv, float outW, float outS, out float outline)
            {
                float2 cell = abs(frac(uv) - 0.5);
                float  edge = max(cell.x, cell.y);
                outline = smoothstep(0.5 - outW - outS, 0.5 - outW, edge);
                return step(0.0, sin(uv.x * 3.14159265) * sin(uv.y * 3.14159265));
            }

            float Moire(float2 uv, float t, float freq, float spd)
            {
                return sin(uv.x * freq + t * spd)
                     * sin(uv.y * (freq * 1.03) - t * spd * 1.1);
            }

            // ── Fragment ─────────────────────────────────────────────────────
            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv01 = IN.uv;
                float  t    = _Time.y;

                // ── Ripple accumulation ──────────────────────────────────────
                float2 duv    = float2(_RectAspect, 1.0);
                float2 offset = 0;
                float  emboss = 0;

                [loop]
                for (int i = 0; i < _MaxSources; i++)
                {
                    float4 src  = _RippleSources[i];
                    float4 prm  = _RippleParams[i];
                    float  dt   = t - src.z;
                    if (dt < 0) continue;

                    float2 d    = (uv01 - src.xy) * duv;
                    float  dist = length(d);
                    float  trvl = dist - prm.y * dt;
                    float  wave = sin(trvl * prm.x);
                    float  ring = saturate(1.0 - abs(trvl) / prm.w);
                    float  att  = 1.0 / (1.0 + dist * dist * 6.0 + dt * prm.z);
                    float  str  = src.w * wave * ring * att;

                    emboss += wave * ring * att * src.w;
                    offset += (dist > 1e-4 ? d / dist : 0) * str;
                }

                offset *= _GlobalStrength * _RippleVisibility;
                float2 pd = offset * 6.0 * _RippleVisibility;

                // ── Checkerboard ─────────────────────────────────────────────
                float2 uv = uv01 * 2.0 - 1.0;
                uv.x *= _ScreenParams.x / _ScreenParams.y;
                uv   += pd;

                float a = _Rotation + t * _Speed;
                float s = sin(a), c = cos(a);
                uv = float2(uv.x * c - uv.y * s, uv.x * s + uv.y * c);
                uv *= _Scale;
                uv  = MorphSquareToDiamond(uv, _MorphDiamond);
                uv  = TrippyOffset(uv, t * _Speed, _LineFrequency, _LineWobble);

                float m = Moire(uv, t, _MoireFrequency, _MoireSpeed);
                uv += normalize(uv + 1e-5) * m * _MoireStrength * 0.05;

                float  outline;
                float  chk     = CheckerWithOutline(uv, _OutlineWidth, _OutlineSoft, outline);
                float4 baseCol = lerp(_ColorA, _ColorB, chk);
                float4 oc      = _OutlineColor * outline * _OutlineStrength;
                float4 col     = lerp(baseCol, oc, outline);

                // ── Emboss from ripple ───────────────────────────────────────
                float3 n = normalize(float3(offset * 40.0, 1.0));
                col.rgb += emboss * dot(n, normalize(float3(-0.4, 0.6, 0.7)))
                         * 0.08 * _RippleVisibility;

                return col;
            }
            ENDHLSL
        }
    }
}
