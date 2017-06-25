Shader "Custom/TeamShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_AuxTex ("Auxiliary map", 2D) = "yellow" {}
		_TeamColor ("Team Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _AuxTex;

		struct Input {
			float2 uv_MainTex;
			float2 uv_AuxTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _TeamColor;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 a = tex2D(_AuxTex, IN.uv_AuxTex);
            fixed3 teamRGB = _TeamColor.rgb * a.r * a.g;
			o.Albedo = c.rgb * (1 - a.r) + teamRGB * _Color.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
