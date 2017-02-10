  Shader "Custom/Tile Simple" {
    Properties {
      _MainTex ("Texture", 2D) = "white" {}
      _Color("Color", Color) = (1, 1, 1, 1)
      _UseColor("UseColor", float) = 0 
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
      float _UseColor;

      void surf (Input IN, inout SurfaceOutput o) {
        o.Albedo = lerp(tex2D (_MainTex, IN.uv_MainTex).rgba, _Color, _UseColor);
        //if(_UseColor == 1){
        //}else{
        //  o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb * _Color;
        //}
      }


      ENDCG
    } 
    Fallback "Diffuse"
  }