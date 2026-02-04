Shader "Custom/CheckerboardRippleURP"
{
    Properties
    {
        _SplitValue ("Split Value (-1..1)", Range(-1,1)) = 0
        _SplitLeft ("Split Left (Y)", Range(-0.2,1.2)) = 0.5
        _SplitRight ("Split Right (Y)", Range(-0.2,1.2)) = 0.5
        _SplitFeather ("Split Feather", Range(0,0.1)) = 0.01
        _SplitLineWidth ("Split Line Width", Range(0,0.1)) = 0.01
        _SplitLineSoft ("Split Line Soft", Range(0,0.1)) = 0.01
        _SplitLineColor ("Split Line Color", Color) = (1,1,1,1)
        _SplitLineStrength ("Split Line Strength", Range(0,2)) = 1

        _AScale ("A Checker Scale", Float) = 8
        _ARotation ("A Rotation", Float) = 0
        _ASpeed ("A Animation Speed", Float) = 0.4
        _ALineWobble ("A Line Wobble", Range(0,1)) = 0.25
        _ALineFrequency ("A Line Frequency", Float) = 3
        _AColorA ("A Color A", Color) = (0.95, 0.92, 0.85, 1)
        _AColorB ("A Color B", Color) = (0.65, 0.8, 1.0, 1)
        _AOutlineWidth ("A Outline Width", Range(0,0.5)) = 0.08
        _AOutlineSoft ("A Outline Softness", Range(0,0.5)) = 0.05
        _AOutlineColor ("A Outline Color", Color) = (0.05,0.05,0.08,1)
        _AOutlineStrength ("A Outline Strength", Range(0,2)) = 1
        _AMoireStrength ("A Moire Strength", Range(0,1)) = 0.25
        _AMoireFrequency ("A Moire Frequency", Float) = 18
        _AMoireSpeed ("A Moire Speed", Float) = 0.2

        _BScale ("B Checker Scale", Float) = 8
        _BRotation ("B Rotation", Float) = 0
        _BSpeed ("B Animation Speed", Float) = 0.4
        _BLineWobble ("B Line Wobble", Range(0,1)) = 0.25
        _BLineFrequency ("B Line Frequency", Float) = 3
        _BColorA ("B Color A", Color) = (0.95, 0.92, 0.85, 1)
        _BColorB ("B Color B", Color) = (0.65, 0.8, 1.0, 1)
        _BOutlineWidth ("B Outline Width", Range(0,0.5)) = 0.08
        _BOutlineSoft ("B Outline Softness", Range(0,0.5)) = 0.05
        _BOutlineColor ("B Outline Color", Color) = (0.05,0.05,0.08,1)
        _BOutlineStrength ("B Outline Strength", Range(0,2)) = 1
        _BMoireStrength ("B Moire Strength", Range(0,1)) = 0.25
        _BMoireFrequency ("B Moire Frequency", Float) = 18
        _BMoireSpeed ("B Moire Speed", Float) = 0.2

        _MaxSources ("Max Sources", Int) = 8
        _GlobalStrength ("Global Strength", Range(0,0.1)) = 0.02
        _RippleVisibility ("Ripple Visibility", Range(0,2)) = 1
        _RectAspect ("Rect Aspect", Float) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }

        Pass
        {
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

            float _SplitValue;
            float _SplitLeft;
            float _SplitRight;
            float _SplitFeather;
            float _SplitLineWidth;
            float _SplitLineSoft;
            float4 _SplitLineColor;
            float _SplitLineStrength;

            float _AScale;
            float _ARotation;
            float _ASpeed;
            float _ALineWobble;
            float _ALineFrequency;
            float4 _AColorA;
            float4 _AColorB;
            float _AOutlineWidth;
            float _AOutlineSoft;
            float4 _AOutlineColor;
            float _AOutlineStrength;
            float _AMoireStrength;
            float _AMoireFrequency;
            float _AMoireSpeed;

            float _BScale;
            float _BRotation;
            float _BSpeed;
            float _BLineWobble;
            float _BLineFrequency;
            float4 _BColorA;
            float4 _BColorB;
            float _BOutlineWidth;
            float _BOutlineSoft;
            float4 _BOutlineColor;
            float _BOutlineStrength;
            float _BMoireStrength;
            float _BMoireFrequency;
            float _BMoireSpeed;

            int _MaxSources;
            float _GlobalStrength;
            float _RippleVisibility;
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

            float2 MorphSquareToDiamond(float2 uv, float morph)
            {
                float2 d = float2(uv.x + uv.y, uv.x - uv.y);
                return lerp(uv, d, morph);
            }

            float2 TrippyOffset(float2 uv, float t, float freq, float wobble)
            {
                float x = sin(uv.y * freq + t) * wobble;
                float y = sin(uv.x * freq - t * 1.3) * wobble;
                return uv + float2(x, y);
            }

            float CheckerWithOutline(float2 uv, float outlineWidth, float outlineSoft, out float outline)
            {
                float2 cell = abs(frac(uv) - 0.5);
                float edge = max(cell.x, cell.y);
                outline = smoothstep(0.5 - outlineWidth - outlineSoft, 0.5 - outlineWidth, edge);
                return step(0.0, sin(uv.x * 3.14159265) * sin(uv.y * 3.14159265));
            }

            float Moire(float2 uv, float t, float freq, float speed)
            {
                float m1 = sin(uv.x * freq + t * speed);
                float m2 = sin(uv.y * (freq * 1.03) - t * speed * 1.1);
                return m1 * m2;
            }

            float4 CheckerBoard(
                float2 resolution,
                float2 uv01,
                float2 rippleOffset,
                float morph,
                float scale,
                float rotation,
                float animSpeed,
                float lineFreq,
                float lineWobble,
                float4 colA,
                float4 colB,
                float outlineWidth,
                float outlineSoft,
                float4 outlineColor,
                float outlineStrength,
                float moireStrength,
                float moireFreq,
                float moireSpeed
            )
            {
                float2 uv = uv01 * 2.0 - 1.0;
                uv.x *= resolution.x / resolution.y;

                uv += rippleOffset;

                float a = rotation + _Time.y * animSpeed;
                float s = sin(a);
                float c = cos(a);
                uv = float2(uv.x * c - uv.y * s, uv.x * s + uv.y * c);

                uv *= scale;
                uv = MorphSquareToDiamond(uv, morph);
                uv = TrippyOffset(uv, _Time.y * animSpeed, lineFreq, lineWobble);

                float m = Moire(uv, _Time.y, moireFreq, moireSpeed);
                uv += normalize(uv + 1e-5) * m * moireStrength * 0.05;

                float outline;
                float chk = CheckerWithOutline(uv, outlineWidth, outlineSoft, outline);

                float4 baseCol = lerp(colA, colB, chk);
                float4 ocol = outlineColor * outline * outlineStrength;

                return lerp(baseCol, ocol, outline);
            }

            float SplitLineY(float x01, float splitValue)
            {
                float baseY = lerp(_SplitLeft, _SplitRight, x01);
                float f = saturate((splitValue + 1.0) * 0.5);
                float offset = lerp(0.6, -0.6, f);
                return baseY + offset;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float2 uv01 = IN.uv;
                float t = _Time.y;

                float2 duv = float2(_RectAspect, 1.0);
                float2 offset = 0;
                float embossSum = 0;

                [loop]
                for (int i = 0; i < _MaxSources; i++)
                {
                    float4 src = _RippleSources[i];
                    float4 prm = _RippleParams[i];

                    float dt = t - src.z;
                    if (dt < 0) continue;

                    float2 d = (uv01 - src.xy) * duv;
                    float dist = length(d);

                    float travel = dist - prm.y * dt;
                    float wave = sin(travel * prm.x);
                    float ring = saturate(1.0 - abs(travel) / prm.w);
                    float atten = 1.0 / (1.0 + dist * dist * 6.0 + dt * prm.z);

                    float strength = src.w * wave * ring * atten;
                    embossSum += wave * ring * atten * src.w;

                    float2 dir = dist > 1e-4 ? d / dist : 0;
                    offset += dir * strength;
                }

                offset *= _GlobalStrength * _RippleVisibility;
                float2 paintDisturbance = offset * 6.0 * _RippleVisibility;

                float4 colA = CheckerBoard(
                    _ScreenParams.xy,
                    uv01,
                    paintDisturbance,
                    0.0,
                    _AScale,
                    _ARotation,
                    _ASpeed,
                    _ALineFrequency,
                    _ALineWobble,
                    _AColorA,
                    _AColorB,
                    _AOutlineWidth,
                    _AOutlineSoft,
                    _AOutlineColor,
                    _AOutlineStrength,
                    _AMoireStrength,
                    _AMoireFrequency,
                    _AMoireSpeed
                );

                float4 colB = CheckerBoard(
                    _ScreenParams.xy,
                    uv01,
                    paintDisturbance,
                    1.0,
                    _BScale,
                    _BRotation,
                    _BSpeed,
                    _BLineFrequency,
                    _BLineWobble,
                    _BColorA,
                    _BColorB,
                    _BOutlineWidth,
                    _BOutlineSoft,
                    _BOutlineColor,
                    _BOutlineStrength,
                    _BMoireStrength,
                    _BMoireFrequency,
                    _BMoireSpeed
                );

                float splitY = SplitLineY(uv01.x, _SplitValue);
                float feather = max(_SplitFeather, 1e-5);
                float maskB = smoothstep(splitY - feather, splitY + feather, uv01.y);

                float4 col = lerp(colA, colB, maskB);

                float distLine = abs(uv01.y - splitY);
                float lw = max(_SplitLineWidth, 1e-5);
                float ls = max(_SplitLineSoft, 1e-5);
                float ln = 1.0 - smoothstep(lw, lw + ls, distLine);
                col = lerp(col, _SplitLineColor, saturate(ln * _SplitLineStrength));

                float3 lightDir = normalize(float3(-0.4, 0.6, 0.7));
                float3 fakeNormal = normalize(float3(offset * 40.0, 1.0));
                float lighting = dot(fakeNormal, lightDir);
                col.rgb += embossSum * lighting * 0.08 * _RippleVisibility;

                return col;
            }

            ENDHLSL
        }
    }
}
