// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/HpBar"
{
    Properties
    {
        _Color("Color", Color) = (1, 0, 0, 1)
        _LineColor("Line Color", Color) = (0, 0, 1, 1)
        _LineWidth("Line Width", Range(0.0, 0.5)) = 0.05
        _Slope("Slope", Range(0.0, 1.0)) = 0.5
        _StartOffset("Start Offset", Range(0.0, 1.0)) = 0.5
        _EndOffset("End Offset", Range(0.0, 1.0)) = 0.0
        _EndLineWidth("End Line Width", Range(0.0, 0.5)) = 0.0
        _EndColor("End Color", Color) = (0, 0, 0, 0)
        _CurrentHp("Current HP", float) = 2
        _MaxHp("Max HP", float) = 2
    }
    SubShader
    {
        // No culling or depth
        //Cull Off ZWrite On ZTest Always
         Tags{"Queue"="Transparent"}
         Blend SrcAlpha OneMinusSrcAlpha
         Lighting Off	
         ZWrite Off
         Cull Off	

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
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 _Color;
            float4 _LineColor;
            float4 _EndColor;
            float _LineWidth;
            float _Slope;
            float _StartOffset;
            float _EndOffset;
            float _EndLineWidth;
            float _CurrentHp;
            float _MaxHp;

            float antiAlias(float x, float lineWidth) {
                float width = 1 - lineWidth;
                float blur = 6.0;
                return (x-(width-blur/_ScreenParams.y))*(_ScreenParams.y/blur);
            }

            float4 drawLine( in float2 p, in float2 a, in float2 b, in float4 bgColor, in float4 lineColor, float lineWidth)
            {
                float2 pa = -p - a;
                float2 ba = b - a;
                float h = clamp( dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
                float d = length( pa - ba*h );
                
                float lineC = clamp(antiAlias(1.0 - d, lineWidth), 0, 1);

                float4 c = 0;
                c -= lineC * bgColor;
                c += lineC * lineColor;
                return c;
            }

            fixed4 frag (Frag i) : SV_Target
            {
                fixed4 c = _Color;
                float spacing = (1.0 - _Slope - _StartOffset - _EndOffset) / _MaxHp;

                //cut off beginning section
                if(i.uv.x - _StartOffset - (i.uv.y * _Slope) + _LineWidth < 0){
                    return fixed4(0,0,0,0);
                }

                //cut off end diagonal
                if(
                    i.uv.x 
                    + ((_MaxHp - _CurrentHp) * spacing ) //factor for how much hp is missing
                    + ((1 - i.uv.y) * _Slope) //angle it
                    + _EndOffset 
                    - (_LineWidth * 2) //give the last line some breathing room
                    > 1
                ){
                    c = fixed4(0,0,0,0);
                }

                //invert coords for line
                i.uv = 1 - i.uv;
                float2 p = i.uv - 1.0;

                for(int itr = 0; itr < _CurrentHp; itr++){
                    float xPos = itr * spacing + _StartOffset;
                    c += drawLine(p, float2(xPos, 0), float2(xPos + _Slope, 1), c, _LineColor, _LineWidth);
                }
                //draw final line
                float xPos = _CurrentHp * spacing + _StartOffset + _LineWidth;
                c += drawLine(p, float2(xPos, 0), float2(xPos + _Slope, 1), c, _LineColor, _LineWidth);

                //draw end color line
                c += drawLine(p, float2(1 - _EndOffset - _EndLineWidth, 0), float2(1 - _EndOffset + _Slope - _EndLineWidth, 1), c, _EndColor, _EndLineWidth);

                return c;
            }

            ENDCG
        }
    }
    FallBack "VertexLit"
}
