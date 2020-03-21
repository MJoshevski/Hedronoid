Shader "Custom/ScreenSpaceGradient" {
    Properties{
      _MainTex("Texture", 2D) = "white" {}
      _FogColor("Fog Color", Color) = (0.3, 0.4, 0.7, 1.0)
      _WorldColor("World Color", Color) = (0.3, 0.4, 0.7, 1.0)
      _Glossiness("Smoothness", Range(0,1)) = 0.5
      _Metallic("Metallic", Range(0,1)) = 0.0
    }
        SubShader{
          Tags { "RenderType" = "Opaque" }
          LOD 300
          CGPROGRAM
          #pragma surface surf Standard 
          struct Input {
              float2 uv_MainTex;
              float4 screenPos;
              half fog;
          };


          fixed4 _FogColor;
          fixed4 _WorldColor;

          half _Glossiness;
          half _Metallic;

          sampler2D _MainTex;
          void surf(Input IN, inout SurfaceOutputStandard o) {
              fixed4 c = tex2D(_MainTex, IN.uv_MainTex)*_WorldColor;
              float2 f = IN.screenPos.xy / IN.screenPos.w;
              //half fog = min(1, dot(f.xy, f.xy) * 0.5);
               o.Albedo = lerp(_FogColor, c, f.y);

               o.Metallic = _Metallic;
               o.Smoothness = _Glossiness;
               o.Alpha = c.a;
          }
          ENDCG
      }
          Fallback "Diffuse"
}