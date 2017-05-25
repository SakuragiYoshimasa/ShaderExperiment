 Shader "Custom/Chessboard" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Resolution ("Resolution", Float) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			//float2 uv_MainTex;
			float4 color;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _Resolution;

		int mod(float a, float b){
			return int(a - floor(a / b) * b);
		}

		void vert(inout appdata_full v, out Input data){
			UNITY_INITIALIZE_OUTPUT(Input, data);
			//data.uv_MainTex = v.texcoord;
			fixed4 c = float4(0, 0, 0, 0);
			float2 uv = v.texcoord.xy;
			int f = int(uv.x * 2.0) % 2;

			if(f  == 1){
				c = float4(1.0, 1.0, 1.0, 1.0);
			}

			data.color = c;
			//data.color = float4(uv, 1.0, 1.0);
		}

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			//fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 c = IN.color;
			//c = float4(1.0, 0, 0, 0);
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
