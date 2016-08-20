Shader "Custom/HpBar"
{
    Properties
    {
        _Color("Color", Color) = (1, 0, 0, 1)
        _LineColor("Line Color", Color) = (0, 0, 1, 1)
        _LineWidth("Line Width", Range(0.0, 0.5)) = 0.05
        _Slope("Slope", Range(0.0, 1.0)) = 0.5
        _StartOffset("Start Offset", Range(0.0, 1.0)) = 0.5
        _Hp("HP", Int) = 2
    }
    SubShader
    {
        // No culling or depth
        //Cull Off ZWrite On ZTest Always
         Tags{"Queue"="Transparent"}
         Blend SrcAlpha OneMinusSrcAlpha

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

            float _LineWidth;

            float antiAlias(float x) {
                float width = 0.999 - _LineWidth;
                return (x-(width-2.0/_ScreenParams.y))*(_ScreenParams.y/2.0);
            }

            float4 drawLine( in float2 p, in float2 a, in float2 b, in float4 bgColor, in float4 lineColor)
            {
                float2 pa = -p - a;
                float2 ba = b - a;
                float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
                float d = length( pa - ba*h );
                
                float lineC = clamp(antiAlias(1.0 - d), 0, 1);

                float4 c = 0;
                c -= lineC * bgColor;
                c += lineC * lineColor;
                return c;
            }

            float4 _Color;
            float4 _LineColor;
            float _Slope;
            float _StartOffset;
            int _Hp;

            fixed4 frag (Frag i) : SV_Target
            {
                fixed4 c = _Color;
                //invert coords
                i.uv = 1 - i.uv;
                float2 p = i.uv - 1.0;

                float spacing = (1.0 - _Slope - _StartOffset) / _Hp;

                for(int i = 0; i < _Hp; i++){
                    float xPos = i * spacing + _StartOffset;
                    c += drawLine(p, float2(xPos, 0), float2(xPos + _Slope, 1), _Color, _LineColor);
                }
                //draw final line
                float xPos = _Hp * spacing + _StartOffset;
                c += drawLine(p, float2(xPos, 0), float2(xPos + _Slope, 1), _Color, _LineColor);

                return c;
            }

            ENDCG
        }
    }
    FallBack "VertexLit"
}
