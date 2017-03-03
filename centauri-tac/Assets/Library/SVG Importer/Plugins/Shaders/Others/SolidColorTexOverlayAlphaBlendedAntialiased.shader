// Copyright (C) 2015 Jaroslav Stehlik - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

Shader "SVG Importer/SolidColor/SolidColorTexOverlayAlphaBlendedAntialiased" {
	
	Properties {
		_MainTex ("Texture", 2D) = "white" { }
	}
	
	SubShader
	{
		Tags {"RenderType"="Transparent" "Queue"="Transparent"}
		LOD 200	
		Lighting Off	
		Blend SrcAlpha OneMinusSrcAlpha			
		ZWrite Off
		Cull Off	
		Fog { Mode Off }	
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vertexColor
			#pragma fragment fragmentColor
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			float2 SVG_SOLID_ANTIALIASING_WIDTH;
			
			struct vertex_input
			{
			    float4 vertex : POSITION;			    
			    half4 color : COLOR;		
			    half2 uv: TEXCOORD0;	    
			    float3 normal : NORMAL;
			};
			
			struct vertex_output
			{
			    float4 vertex : SV_POSITION;			    
			    half4 color : COLOR;	
			    half2 uv: TEXCOORD0;	    		    
			};	
			
			vertex_output vertexColor(vertex_input v)
			{
			    vertex_output o;
			    
			    if(UNITY_MATRIX_P[3][3] == 0)
				{
					float4 vertex = v.vertex;
					float objSpaceLength = length(ObjSpaceViewDir(vertex));
					vertex.x += v.normal.x * objSpaceLength * SVG_SOLID_ANTIALIASING_WIDTH.x;
					vertex.y += v.normal.y * objSpaceLength * SVG_SOLID_ANTIALIASING_WIDTH.y;
					o.vertex = mul(UNITY_MATRIX_MVP, vertex);
				// Orthographic Camera
				} else {
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.vertex.x += v.normal.x * SVG_SOLID_ANTIALIASING_WIDTH.x;
					o.vertex.y += v.normal.y * SVG_SOLID_ANTIALIASING_WIDTH.y;
				} 
			    
			    o.color = v.color;
			    o.uv = v.uv;
			    return o;
			}
			
			half4 fragmentColor(vertex_output i) : COLOR
			{
				return i.color * tex2D(_MainTex, i.uv);
			}
			
			ENDCG
        }
	}
}
