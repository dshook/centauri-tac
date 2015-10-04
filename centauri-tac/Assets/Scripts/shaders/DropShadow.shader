Shader "Sprites/Drop Shadow"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0

        _ShadowColor("Shadow Color", Color) = (0,0,0,0.5)
        _ShadowX("Shadow X Offset", Range(-32,32)) = 4
        _ShadowY("Shadow Y Offset", Range(-32,32)) = 4
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Fog{ Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile DUMMY PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color : COLOR;
                half2 texcoord  : TEXCOORD0;
            };

            fixed4 _Color;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
#ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
#endif

                return OUT;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _ShadowColor;
            float _ShadowX;
            float _ShadowY;

            float4 sampleColor(float2 coord)
            {
                return tex2D(_MainTex, coord) * _Color; // Always apply _Color multiplier
            }

            fixed4 frag(v2f IN) : COLOR
            {
                // Original color
                float4 color = sampleColor(IN.texcoord);

                // Shadow color
                float2 shadowOffset = _MainTex_TexelSize.xy * float2(_ShadowX, -_ShadowY);
                float shadowSample = sampleColor(IN.texcoord - shadowOffset).a;
                float4 shadowColor = _ShadowColor;
                shadowColor.a *= shadowSample;

                // Final color
                // Alpha blend src (color) with dest (shadowColor)
                float4 finalColor;
                finalColor.a = color.a + shadowColor.a * (1 - color.a);
                finalColor.rgb = (color.rgb * color.a + shadowColor.rgb * shadowColor.a * (1 - color.a)) / finalColor.a;

                return finalColor;
            }
            ENDCG
        }
    }
}