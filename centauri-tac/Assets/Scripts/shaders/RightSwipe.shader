// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/RightSwipe" {
    
    Properties {
        //_Params ("Params", Vector) = (1.0, 1.0, 1.0, 1.0)
        
        _Color ("Tint", Color) = (1,1,1,1)
    }
    
    SubShader
    {
        Tags {"RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector" = "True"}
        LOD 200
        Lighting Off
        ZTest [unity_GUIZTestMode]
        ZWrite Off
        Cull Off
        
        Blend SrcAlpha OneMinusSrcAlpha
        Fog { Mode Off }

        
        Pass
        {
            CGPROGRAM
            #pragma vertex vertexGradients
            #pragma fragment fragmentGradientsAlphaBlended
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma glsl_no_auto_normalization 
            #pragma enable_d3d11_debug_symbols
            #include "UnityCG.cginc"			
            #include "UnityUI.cginc"

                
            struct vertex_input
            {
                float4 vertex : POSITION;	
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float3 normal : NORMAL;
                half4 color : COLOR;
            };

            struct vertex_output
            {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 localPosition : TEXCOORD3;
                half4 color : COLOR;
            };
            sampler2D _GradientColor;
            float4 _Color;

            vertex_output vertexGradients(vertex_input v)
            {
                vertex_output o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.localPosition = v.vertex;
                o.color = v.color;
                
                return o;
            }

            fixed4 fragmentGradientsAlphaBlended(vertex_output i) : COLOR
            {
                float2 st = i.vertex.xy/_ScreenParams.xy;
                fixed4 output = i.color * _Color * st.x;

                return output;
            }

            ENDCG
        }
    }
}
