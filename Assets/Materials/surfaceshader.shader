Shader "Example/Splat Texture" {
    Properties {
      _MainTex ("Grass", 2D) = "white" {}
      _MudTex ("Mud",2D) = "white" {}
      _RockTex ("Rock",2D) = "white" {}
      _SplatTex ("Splat",2D) = "white" {}
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Lambert
      struct Input {
          float2 uv_MainTex;
          float3 worldPos;
      };
      sampler2D _MainTex,_MudTex,_SplatTex,_RockTex;

      void surf (Input IN, inout SurfaceOutput o) {
      	  fixed3 c=tex2D(_SplatTex, IN.uv_MainTex).rgb;
          o.Albedo =  c.r * tex2D (_MudTex, IN.worldPos.xz/16).rgb + c.g*tex2D(_MainTex,IN.worldPos.xz/16).rgb + c.b*tex2D(_RockTex,IN.worldPos.xz/16);
      }
      ENDCG
    } 
    Fallback "Diffuse"
  }
