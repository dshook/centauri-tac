Shader "CloverSwatch/BinstonBarrier"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (0,0,0,0)
	}

	SubShader
	{
		Blend One One
		ZWrite Off
		Cull Off

		Tags
		{
			"RenderType"="Transparent"
			"Queue"="Transparent"
		}

		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 viewDir : TEXCOORD2;
				float3 objectPos : TEXCOORD3;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				o.objectPos = v.vertex.xyz;		
				o.normal = UnityObjectToWorldNormal(v.normal);
				//o.viewDir = normalize(UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex)));

				return o;
			}
			
			fixed4 _Color;

			float triWave(float t, float offset, float yOffset)
			{
				return saturate(abs(frac(offset + t) * 2 - 1) + yOffset);
				//return sin(t + offset + yOffset);
			}

			fixed4 texColor(v2f i, float rim)
			{
				fixed4 mainTex = tex2D(_MainTex, i.uv);
				mainTex.r *= triWave(_Time.x * 2, i.objectPos.y, -0.7) * 6;
				// I ended up saturaing the rim calculation because negative values caused weird artifacts
				mainTex.g *= saturate(rim) * (sin((_Time.z + mainTex.b * 2) / 0.8) * 0.5 + 1);
				return mainTex.r * _Color + mainTex.g * _Color;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float intersect = 0;

				//float rim = 1 - abs(dot(i.normal, normalize(i.viewDir))) * 2;
				float rim = 0;
				float glow = max(intersect, rim);

				//fixed4 glowColor = fixed4(lerp(_Color.rgb, fixed3(1, 1, 1), pow(glow, 4)), 1);
				fixed4 glowColor = _Color;
				
				fixed4 hexes = texColor(i, rim);

				fixed4 col = _Color * _Color.a + glowColor * glow + hexes;
				return col;
			}
			ENDCG
		}
	}
}
