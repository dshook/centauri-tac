Shader "Custom/HpBar"
{
    Properties
    {
        _Color("Rim Color", Color) = (1, 0, 0, 1)
        _Slope("Slope", Range(0.0, 1.0)) = 0.5
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

            float antiAlias(float x) {return (x-(0.989-2.0/_ScreenParams.y))*(_ScreenParams.y/2.0);}

            float drawLine( in float2 p, in float2 a, in float2 b, in float4 color )
            {
                float2 pa = -p - a;
                float2 ba = b - a;
                float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
                float d = length( pa - ba*h );
                
                return clamp(antiAlias(1.0 - d), 0.0, 1.0);
            }

            float4 _Color;
            float _Slope;

            fixed4 frag (Frag i) : SV_Target
            {
                fixed4 c = 0;
                i.uv = 1 - i.uv;
                float2 p = -1.0 + 1.0 * i.uv;
                c += drawLine(p, float2(0.2, 0), float2(0.3, 1), _Color);

                return c;
            }

            ENDCG
        }
    }
    FallBack "VertexLit"
}
