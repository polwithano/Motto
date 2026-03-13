Shader "Custom/Background/FluidWaveURP"
{
    // ─────────────────────────────────────────────────────────────────────────
    //  FluidWaveURP
    //
    //  Back to front:
    //    BackFill   — antiphase wave (amplitude = -_WaveAmp), darker colour
    //    FrontFill  — primary wave  (amplitude = +_WaveAmp), lighter colour
    //    Fresnel    — surface glow + lateral edge glow
    //
    //  The inverted amplitude creates a fake depth: when the front crest rises,
    //  the back trough dips, making the front feel closer.
    //
    //  Driven by WaveController:  _WaveHeight (0..1), _WaveTurbulence (0..1)
    // ─────────────────────────────────────────────────────────────────────────

    Properties
    {
        [Header(Wave)]
        _WaveHeight         ("Wave Height (-0.2..1.2)",      Range(-0.2,1.2))    = 0.5

        // Shared amplitude — back wave uses the negative of this value
        _WaveAmp            ("Wave Amplitude",          Range(0,0.08)) = 0.025

        // Shared frequency and speed for both waves
        _WaveFreq           ("Wave Frequency",          Float)         = 2.0
        _WaveSpeed          ("Wave Speed",              Float)         = 1.8

        // Phase offset between back and front wave (degrees).
        // 180 = perfect antiphase — back max when front min.
        _PhaseOffset        ("Phase Offset (degrees)",  Range(0,360))  = 180.0

        // Lifts the front wave above the back in UV space.
        // Creates the visible gap between the two colour bands.
        _FrontBaseOffset    ("Front Base Offset (UV)",  Range(0,0.25)) = 0.08

        // Bob — vertical oscillation pinched at sides, synced on both waves
        _BobFrequency       ("Bob Frequency",           Float)         = 1.0
        _BobAmplitude       ("Bob Amplitude",           Range(0,0.05)) = 0.012
        _BobVerticalStrength("Bob Vertical Strength",   Range(0,0.3))  = 0.125

        // Turbulence
        _WaveTurbulence     ("Turbulence (0..1)",       Range(0,1))    = 0.0
        _TurbAmplitude      ("Turb Amplitude",          Range(0,0.06)) = 0.022
        _TurbFrequency      ("Turb Frequency",          Float)         = 18.0
        _TurbSpeed          ("Turb Speed",              Float)         = 5.0

        [Header(Colours)]
        _FrontFillInner     ("Front Fill Inner",        Color)         = (0.35, 1.00, 1.00, 0.92)
        _FrontFillOuter     ("Front Fill Outer",        Color)         = (0.00, 0.35, 1.00, 0.92)
        _BackFillColour     ("Back Fill Colour",        Color)         = (0.00, 0.22, 0.75, 0.78)

        [Header(Fresnel)]
        _FresnelColour      ("Fresnel Colour",          Color)         = (0.55, 0.90, 1.00, 1.00)
        _FresnelPower       ("Fresnel Power",           Range(1,12))   = 5.0
        _FresnelStrength    ("Fresnel Strength",        Range(0,2))    = 1.0
        // Lateral (left/right edge) fresnel width in UV space
        _FresnelEdgeWidth   ("Fresnel Edge Width",      Range(0,0.35)) = 0.12
        // Surface (top of liquid) fresnel thickness in UV space
        _FresnelSurfaceWidth("Fresnel Surface Width",   Range(0,0.15)) = 0.06

        _RectAspect         ("Rect Aspect (W/H)",       Float)         = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent+10"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull   Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            float  _WaveHeight, _WaveAmp;
            float  _WaveFreq, _WaveSpeed;
            float  _PhaseOffset, _FrontBaseOffset;
            float  _BobFrequency,  _BobAmplitude, _BobVerticalStrength;
            float  _WaveTurbulence, _TurbAmplitude, _TurbFrequency, _TurbSpeed;

            float4 _FrontFillInner, _FrontFillOuter, _BackFillColour;

            float4 _FresnelColour;
            float  _FresnelPower, _FresnelStrength;
            float  _FresnelEdgeWidth, _FresnelSurfaceWidth;

            float  _RectAspect;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }
            
            // ── Wave functions ────────────────────────────────────────────────
            //
            //  Back  baseline : _WaveHeight               (true water level)
            //  Front baseline : _WaveHeight + _FrontBaseOffset  (sits above back)
            //
            //  Both oscillate with _WaveAmp at the same frequency/speed.
            //  _PhaseOffset shifts the front sinusoid relative to the back.
            //  At 180° the front crests exactly where the back troughs →
            //  maximum visible separation at all times.
            //
            //  The colour band between them always exists because their
            //  baselines are independent — _FrontBaseOffset guarantees a floor.

            static const float DEG2RAD = 0.01745329251;
            static const float PI8 = 3.14159265f; 

            float BackWaveY(float x, float t)
            {
                float bob  = sin(t * _BobFrequency * PI8) * _BobAmplitude;
                float wave = sin(x * _WaveFreq + t * _WaveSpeed) * _WaveAmp;
                wave += bob * _BobVerticalStrength;
                wave += sin(x * _TurbFrequency - t * _TurbSpeed * 1.3)
                      * _TurbAmplitude * _WaveTurbulence * 0.6;
                return _WaveHeight + wave;
            }

            float FrontWaveY(float x, float t)
            {
                float bob      = sin(t * _BobFrequency * PI8) * _BobAmplitude;
                float phaseRad = _PhaseOffset * DEG2RAD;
                float wave     = sin(x * _WaveFreq + t * _WaveSpeed + phaseRad) * _WaveAmp;
                wave -= bob * _BobVerticalStrength;
                wave += sin(x * _TurbFrequency + t * _TurbSpeed)
                      * _TurbAmplitude * _WaveTurbulence;
                return _WaveHeight + _FrontBaseOffset + wave;
            }

            // ── Fragment ──────────────────────────────────────────────────────
            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float  t  = _Time.y;

                float frontY   = FrontWaveY(uv.x, t);
                float backY    = BackWaveY (uv.x, t);
                float frontSDF = uv.y - frontY;  // > 0 above, < 0 inside
                float backSDF  = uv.y - backY;

                // Discard pixels well above the front wave
                if (frontSDF > 1) discard;

                // ── Fill masks ────────────────────────────────────────────────
                float frontFill = 1.0 - smoothstep(-0.002, 0.002, frontSDF);
                float backFill  = saturate(
                    (1.0 - smoothstep(-0.002, 0.002, backSDF)) - frontFill);

                // ── Fresnel — lateral edges ───────────────────────────────────
                float edgeDist = min(uv.x, 1.0 - uv.x);
                float edgeT    = saturate(edgeDist / _FresnelEdgeWidth);
                float fresnel  = pow(1.0 - edgeT, _FresnelPower) * _FresnelStrength;
                fresnel       *= frontFill;

                // ── Fresnel — surface (top of liquid) ─────────────────────────
                // _FresnelSurfaceWidth controls how far down the glow extends
                float surfW       = max(_FresnelSurfaceWidth, 0.001);
                float surfT       = saturate(abs(frontSDF) / surfW);
                float surfFresnel = pow(1.0 - surfT, _FresnelPower) * frontFill;

                // ── Front colour — lateral fresnel shifts inner→outer ─────────
                float3 frontColour = lerp(_FrontFillInner.rgb, _FrontFillOuter.rgb,
                                          saturate(fresnel + surfFresnel * 0.4));

                // ── Composite ─────────────────────────────────────────────────
                float3 rgb = 0;
                float  a   = 0;

                // Back fill
                float bA = _BackFillColour.a * backFill;
                rgb = lerp(rgb, _BackFillColour.rgb, bA);
                a   = saturate(a + bA);

                // Front fill
                float fA = _FrontFillInner.a * frontFill;
                rgb = lerp(rgb, frontColour, fA);
                a   = saturate(a + fA * (1.0 - a));

                // Fresnel additive glow (lateral + surface)
                rgb += _FresnelColour.rgb * _FresnelColour.a
                     * (fresnel + surfFresnel * 0.5);

                if (a < .004) discard;
                return float4(saturate(rgb), saturate(a));
            }
            ENDHLSL
        }
    }
}
