Shader "Custom/ExtractedPositionTest" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		
		#pragma surface surf Standard vertex:vert fullforwardshadows
		#pragma target 3.0

		sampler2D _PositionTex;

		struct Input {
			float2 uv_MainTex;
			float psize:PSIZE;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void vert(inout appdata_full v, out Input o){
			UNITY_INITIALIZE_OUTPUT(Input, o);

			float3 pos = tex2Dlod(_PositionTex, float4(v.texcoord.xy, 0, 0)).xyz;
			v.vertex.xyz = pos;
			o.psize = 3.0;
		}

		UNITY_INSTANCING_CBUFFER_START(Props)
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c =  _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
