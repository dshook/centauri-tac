﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Card"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_DisplaceTex("Displacement Texture", 2D) = "white" {}
		_RarityMask("Rarity Mask", 2D) = "black" {}
		_RarityColor("Rarity Color", Color) = (0, 0, 1, 1)
	}
	SubShader
	{
		// No culling or depth
		//Cull Off ZWrite Off ZTest Always

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
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _DisplaceTex;
			sampler2D _RarityMask;
			float4 _RarityColor;

			float4 frag (v2f i) : SV_Target
			{

				float4 disp = tex2D(_DisplaceTex, i.uv);
				float4 rarity = tex2D(_RarityMask, i.uv) * _RarityColor;

				float4 col = tex2D(_MainTex, i.uv);

				return (col + rarity) * disp;
			}
			ENDCG
		}
	}
}