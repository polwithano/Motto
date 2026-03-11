Shader "Custom/FireMeshAnimated"
{
    Properties
    {
        [Header(Colors)]
        _BottomColor  ("Bottom Color", Color) = (0.85, 0.20, 0.02, 1)
        _MidColor     ("Mid Color",    Color) = (1.00, 0.55, 0.03, 1)
        _TopColor     ("Top Color",    Color) = (1.00, 0.85, 0.10, 1)
        _OutlineColor ("Outline",      Color) = (0.38, 0.08, 0.01, 1)
        _OutlineWidth ("Outline Width",Range(0, 0.05)) = 0.015

        [Header(Scale)]
        _Height       ("Height (score)", Range(0, 3))   = 1.0   // scale global Y
        _FireAlpha    ("Alpha",          Range(0, 1))   = 1.0

        [Header(Animation)]
        _Speed        ("Speed",          Range(0, 50))   = 1.2
        _Intensity    ("Intensity",      Range(0, 100))   = 0.5   // amplitude des vagues
        _TurbFreq     ("Turbulence Freq",Range(0, 1000))  = 3.0   // fréquence spatiale
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
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
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float  height      : TEXCOORD1;  // hauteur normalisée [0,1] passée au frag
            };

            float4 _BottomColor, _MidColor, _TopColor, _OutlineColor;
            float  _OutlineWidth, _Height, _FireAlpha;
            float  _Speed, _Intensity, _TurbFreq;

            // ── Vertex shader : scale + animation ───────────────────────
            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 pos = IN.positionOS.xyz;

                // 1. Scale vertical selon le score
                //    uv.y est normalisé [0,1] depuis le mesh (0=base, 1=sommet)
                //    On scale Y mais on garde la base fixe (uv.y = 0 ne bouge pas)
                pos.y *= _Height;

                // 2. Animation : offset horizontal + vertical
                //    L'amplitude augmente avec la hauteur du vertex (les pointes bougent plus)
                float heightFactor = IN.uv.y;  // [0,1], 0=base immobile
                float t            = _Time.y * _Speed;

                // Vague principale
                float wave = sin(t + pos.x * _TurbFreq) * _Intensity * heightFactor;

                // Vague secondaire décalée (donne un aspect plus chaotique)
                float wave2 = sin(t * 1.3 + pos.x * _TurbFreq * 1.7 + 2.0)
                              * _Intensity * 0.4 * heightFactor;

                pos.x += wave  * 0.3;   // léger balancement latéral
                pos.y += wave2 * 0.15;  // légère pulsation verticale

                OUT.positionHCS = TransformObjectToHClip(pos);
                OUT.uv          = IN.uv;
                OUT.height      = IN.uv.y;  // hauteur normalisée pour le gradient

                return OUT;
            }

            // ── Fragment shader : gradient 3 couleurs ───────────────────
            float4 frag(Varyings IN) : SV_Target
            {
                float h = IN.height;  // [0,1]

                // Gradient bas → milieu → haut
                float4 col;
                if (h < 0.5)
                    col = lerp(_BottomColor, _MidColor, h * 2.0);
                else
                    col = lerp(_MidColor, _TopColor, (h - 0.5) * 2.0);

                col.a = _FireAlpha;
                return col;
            }

            ENDHLSL
        }

        // ── Passe outline : extrude les normales vers l'extérieur ────────
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Front   // inverse : dessine l'envers légèrement plus grand

            HLSLPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct Varyings   { float4 positionHCS:SV_POSITION; };

            float4 _OutlineColor;
            float  _OutlineWidth, _Height, _Speed, _Intensity, _TurbFreq;

            Varyings vertOutline(Attributes IN)
            {
                Varyings OUT;
                float3 pos = IN.positionOS.xyz;

                // Même scale + animation que la passe principale
                float heightFactor = IN.uv.y;
                float t            = _Time.y * _Speed;
                float wave         = sin(t + pos.x * _TurbFreq) * _Intensity * heightFactor;
                float wave2        = sin(t * 1.3 + pos.x * _TurbFreq * 1.7 + 2.0)
                                     * _Intensity * 0.4 * heightFactor;

                pos.y *= _Height;
                pos.x += wave  * 0.3;
                pos.y += wave2 * 0.15;

                // Extrude le long de la normale
                pos += IN.normalOS * _OutlineWidth;

                OUT.positionHCS = TransformObjectToHClip(pos);
                return OUT;
            }

            float4 fragOutline(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}