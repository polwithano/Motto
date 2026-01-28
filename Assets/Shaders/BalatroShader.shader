Shader "Custom/BalatroShader"
{
    Properties
    {
        _SpinRotation ("Spin Rotation", Float) = -2.0
        _SpinSpeed ("Spin Speed", Float) = 7.0
        _Offset ("Offset", Vector) = (0,0,0,0)

        _Colour1 ("Colour 1", Color) = (0.871, 0.267, 0.231, 1)
        _Colour2 ("Colour 2", Color) = (0.0, 0.42, 0.706, 1)
        _Colour3 ("Colour 3", Color) = (0.086, 0.137, 0.145, 1)

        _Contrast ("Contrast", Float) = 3.5
        _Lighting ("Lighting", Float) = 0.4
        _SpinAmount ("Spin Amount", Float) = 0.25
        _PixelFilter ("Pixel Filter", Float) = 745.0
        _SpinEase ("Spin Ease", Float) = 1.0
        _IsRotate ("Is Rotate (0 or 1)", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "BalatroPass"

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

            float _SpinRotation;
            float _SpinSpeed;
            float2 _Offset;

            float4 _Colour1;
            float4 _Colour2;
            float4 _Colour3;

            float _Contrast;
            float _Lighting;
            float _SpinAmount;
            float _PixelFilter;
            float _SpinEase;
            float _IsRotate;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 effect(float2 screenSize, float2 screen_coords)
            {
                float pixel_size = length(screenSize) / _PixelFilter;
                float2 uv = (floor(screen_coords * (1.0 / pixel_size)) * pixel_size - 0.5 * screenSize)
                            / length(screenSize) - _Offset;

                float uv_len = length(uv);

                float speed = (_SpinRotation * _SpinEase * 0.2);
                if (_IsRotate > 0.5)
                    speed = _Time.y * speed;

                speed += 302.2;

                float angle = atan2(uv.y, uv.x) + speed
                    - _SpinEase * 20.0 * (_SpinAmount * uv_len + (1.0 - _SpinAmount));

                float2 mid = (screenSize / length(screenSize)) * 0.5;
                uv = float2(
                    uv_len * cos(angle) + mid.x,
                    uv_len * sin(angle) + mid.y
                ) - mid;

                uv *= 30.0;
                speed = _Time.y * _SpinSpeed;

                float2 uv2 = uv.x + uv.y;

                for (int i = 0; i < 5; i++)
                {
                    uv2 += sin(max(uv.x, uv.y)) + uv;
                    uv += 0.5 * float2(
                        cos(5.1123314 + 0.353 * uv2.y + speed * 0.131121),
                        sin(uv2.x - 0.113 * speed)
                    );
                    uv -= cos(uv.x + uv.y) - sin(uv.x * 0.711 - uv.y);
                }

                float contrast_mod = (0.25 * _Contrast + 0.5 * _SpinAmount + 1.2);
                float paint_res = saturate(length(uv) * 0.035 * contrast_mod);

                float c1p = max(0, 1 - contrast_mod * abs(1 - paint_res));
                float c2p = max(0, 1 - contrast_mod * abs(paint_res));
                float c3p = 1 - saturate(c1p + c2p);

                float light = (_Lighting - 0.2) * max(c1p * 5 - 4, 0)
                            + _Lighting * max(c2p * 5 - 4, 0);

                return (0.3 / _Contrast) * _Colour1 +
                    (1 - 0.3 / _Contrast) *
                    (_Colour1 * c1p + _Colour2 * c2p + float4(c3p * _Colour3.rgb, c3p)) +
                    light;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                float2 resolution = _ScreenParams.xy;
                float2 fragCoord = IN.uv * resolution;
                return effect(resolution, fragCoord);
            }

            ENDHLSL
        }
    }
}
