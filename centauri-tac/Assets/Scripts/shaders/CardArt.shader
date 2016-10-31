Shader "Custom/CardArt"
{
	Properties
	{
		_MainTex("Texture", 2D) = "transparent" {}
		_DisplaceTex("Displacement Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		//Cull Off ZWrite Off ZTest Always
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

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
			sampler2D _DisplaceTex;

			float4 frag (v2f i) : SV_Target
			{

				float4 disp = tex2D(_DisplaceTex, i.uv);

				float4 col = tex2D(_MainTex, i.uv);
				return col * disp * col.a;
			}
			ENDCG
		}
	}
}