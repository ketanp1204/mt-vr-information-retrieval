Shader "Custom/StereoPortalShader"
{
    Properties{
      _MainTex("Mono Texture (unused)", 2D) = "white" {}
      _LeftEyeTexture("Left Eye Texture", 2D) = "white" {}
      _RightEyeTexture("Right Eye Texture", 2D) = "white" {}
    }
    SubShader{
        Tags { "RenderType" = "Opaque" }

        CGPROGRAM

        #pragma surface surf Lambert

        struct Input {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        sampler2D _LeftEyeTexture;
        sampler2D _RightEyeTexture;
        
        void surf(Input IN, inout SurfaceOutput o) {
            o.Albedo = unity_StereoEyeIndex == 0 ? tex2D(_LeftEyeTexture, IN.uv_MainTex).rgb : tex2D(_RightEyeTexture, IN.uv_MainTex).rgb;
        }

        ENDCG
    }
    Fallback "Diffuse"
}
