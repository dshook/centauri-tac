  Shader "Custom/Tile Simple" {
    Properties {
      _MainTex ("Texture", 2D) = "white" {}
      _Color("Color", Color) = (1, 1, 1, 1)
      _UseColor("UseColor", int) = 0 
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM

      #pragma surface surf Lambert

      struct Input {
          float2 uv_MainTex;
      };

      sampler2D _MainTex;
      float4 _Color;
      int _UseColor;

      void surf (Input IN, inout SurfaceOutput o) {
        if(_UseColor == 1){
          o.Albedo = _Color;
        }else{
          o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb * _Color;
        }
      }


      ENDCG
    } 
    Fallback "Diffuse"
  }