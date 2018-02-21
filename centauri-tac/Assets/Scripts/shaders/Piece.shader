Shader "Custom/Piece" {
  Properties {
    _Color ("Main Color", Color) = (.5,.5,.5,1)
    _OutlineColor ("Outline Color", Color) = (0,0,0,1)
    _MainTex ("Base (RGB)", 2D) = "white" { }
    _Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
    _Atten ("Atten", Range(0.0, 1.0)) = 0.5
    _IsParalyzed("Is Paralyzed?", Float) = 0
  }

  CGINCLUDE
    #include "UnityCG.cginc"

    struct appdata {
      float4 vertex : POSITION;
      float3 normal : NORMAL;
      float2 uv : TEXCOORD0;
    };

    struct v2f {
      float4 pos : POSITION;
      float4 color : COLOR;
      float2 uv : TEXCOORD0;
    };

    uniform float4 _OutlineColor;

    v2f vert(appdata v) {
      // just make a copy of incoming vertex data but scaled according to normal direction
      v2f o;
      o.pos = UnityObjectToClipPos(v.vertex);

      o.color = _OutlineColor;
      o.uv = v.uv;
      return o;
    }
  ENDCG

  SubShader {
    Tags { "Queue" = "Transparent" }

    // note that a vertex shader is specified here but its using the one above
    Pass {
      Name "OUTLINE_OCCLUSION"
      Tags { "LightMode" = "Always" }
      Cull Off
      ZWrite Off
      ZTest Always
      ColorMask RGB // alpha not used

      // you can choose what kind of blending mode you want for the outline
      Blend SrcAlpha OneMinusSrcAlpha // Normal
      //Blend One One // Additive
      //Blend One OneMinusDstColor // Soft Additive
      //Blend DstColor Zero // Multiplicative
      //Blend DstColor SrcColor // 2x Multiplicative

      CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag

        half4 frag(v2f i) :COLOR {
          return i.color;
        }
      ENDCG
    }

    CGPROGRAM
      #pragma surface surf ToonRamp

      sampler2D _Ramp;
      float _Atten;
      float _IsParalyzed;

      // custom lighting function that uses a texture ramp based
      // on angle between light direction and normal
      #pragma lighting ToonRamp exclude_path:prepass
      inline half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half atten)
      {
        if(_IsParalyzed > 0){
          s.Albedo = float4(0.5, 0.5, 0.5, 1);
        }

        #ifndef USING_DIRECTIONAL_LIGHT
        lightDir = normalize(lightDir);
        #endif

        half d = dot (s.Normal, lightDir)*0.5 + 0.5;
        half3 ramp = tex2D (_Ramp, float2(d,d)).rgb;

        half4 c;
        c.rgb = s.Albedo * _LightColor0.rgb * (ramp * _Atten) * (atten * 2);
        c.a = 0;
        return c;
      }


      sampler2D _MainTex;
      float4 _Color;

      struct Input {
        float2 uv_MainTex : TEXCOORD0;
      };

      void surf (Input IN, inout SurfaceOutput o) {
        half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
        o.Albedo = c.rgb;
        o.Alpha = c.a;
      }
    ENDCG
  }

  Fallback "Diffuse"
}
