Shader "Custom/TileGlowEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        _RimColor("Rim Color", Color) = (1, 0, 0, 1)
        _BorderWidth("Border Width", Range(0.0, 1.0)) = 0.05
        _RimFalloff("Rim Falloff", Range(0.0, 10.0)) = 3.0
        _RimPower("Rim Power", Range(0.0, 10.0)) = 3.0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _RimColor;
            float _RimPower;
            float _RimFalloff;
            float _BorderWidth;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                if( i.uv.x <= _BorderWidth
                    || i.uv.x >= 1 - (_BorderWidth)
                ){
                    return _RimColor;
                }

                if( i.uv.y <= _BorderWidth
                    || i.uv.y >= 1 - _BorderWidth
                ){
                    return _RimColor;
                }

                //float2 wcoord = length(float2(0.5,0.5) - i.uv);

                //float vig = pow(
                //    _RimFalloff * wcoord
                //    , _RimPower
                //);
                //vig = 0;
                //return lerp (col, _RimColor, vig);
                return col;

            }


            ENDCG
        }
    }
}
