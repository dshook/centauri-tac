Shader "Custom/TileGlowEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        _RimColor("Rim Color", Color) = (1, 0, 0, 1)
        _HighlightColor("Highlight Color", Color) = (1, 1, 1, 1)
        _BorderWidth("Border Width", Range(0.0, 2.0)) = 0.05
        _RimFalloff("Rim Falloff", Range(0.0, 2.0)) = 3.0
    }
    SubShader
    {
        // No culling or depth
        //Cull Off ZWrite On ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct Vertex
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            #include "UnityCG.cginc"

            struct Frag
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            Frag vert (Vertex v)
            {
                Frag o;
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _RimColor;
            float _RimFalloff;
            float _BorderWidth;
            float4 _HighlightColor;

            fixed4 frag (Frag i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                float dist = i.uv.x + i.uv.y;
                if( dist <= _BorderWidth && i.uv.x <= 0.33 ){
                    float vig = _RimFalloff * (1/dist);
                    col = col * (_RimColor * vig);
                }

                return col * _HighlightColor;

            }


            ENDCG
        }
    }
    FallBack "VertexLit"
}
