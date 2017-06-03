Shader "Custom/ExtractPosition"
{
	SubShader
	{
		Tags { "Extract" = "Source" }
		Pass
		{
			Cull Off ZWrite Off ZTest Always
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal: NORMAL;
			};

			struct v2f {
				float4 position : SV_POSITION;
				float3 texcoord: TEXCOORD0;
				float3 normal: NORMAL;
				float psize : PSIZE;
			};

			struct fragout {
				float4 position: SV_TARGET0;
				float4 normal: SV_TARGET1;
			};

			v2f vert (appdata v) {
				v2f o;
				o.position = float4(v.texcoord.x * 2 - 1, 0, 0, 1);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.texcoord = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.psize = 1;
				return o;
			}
			
			fragout frag (v2f i) : SV_TARGET0 {
				fragout o;
				o.position = float4(i.texcoord, 1.0);
				o.normal = float4(i.normal, 1.0);
				return o;
			}
			ENDCG
		}
	}
}
