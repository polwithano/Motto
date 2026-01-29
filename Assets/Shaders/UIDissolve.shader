Shader "UI/Dissolve"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise", 2D) = "white" {}

        _Dissolve ("Dissolve", Range(0,1)) = 0
        _EdgeWidth ("Edge Width", Range(0,0.2)) = 0.05
        _EdgeColor ("Edge Color", Color) = (1,0.5,0,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;

            float _Dissolve;
            float _EdgeWidth;
            float4 _EdgeColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                float noise = tex2D(_NoiseTex, i.uv).r;

                float dissolve = smoothstep(
                    _Dissolve - _EdgeWidth,
                    _Dissolve + _EdgeWidth,
                    noise
                );

                // Edge glow
                float edge = smoothstep(_Dissolve, _Dissolve + _EdgeWidth, noise)
                           - smoothstep(_Dissolve + _EdgeWidth, _Dissolve + _EdgeWidth * 2, noise);

                col.rgb = lerp(col.rgb, _EdgeColor.rgb, edge);
                col.a *= dissolve;

                return col;
            }
            ENDHLSL
        }
    }
}
