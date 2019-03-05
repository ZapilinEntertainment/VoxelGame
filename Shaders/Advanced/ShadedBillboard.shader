Shader "Custom/ShadedBillboard" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}
		SubShader{
		Tags
		{
			"RenderType" = "Opaque"
			//"Queue" = "Transparent"
			"ForceNoShadowCasting" = "True"
		}
			LOD 200

			CGPROGRAM
			#pragma vertex vert
			#pragma surface surf Lambert
			#include "AutoLight.cginc"

			sampler2D _MainTex;
		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
		};
			struct Input {
				half3 vertex;
				float2 uv_MainTex;
			};

			fixed4 _Color;

			void vert(inout appdata_base IN)
			{
				IN.vertex =
					mul(UNITY_MATRIX_MV, float4(0, 0, 0, 0))  // в отличии от обычного , w = 0 а не 1
					- float4(-IN.vertex.x, -IN.vertex.y, 0, 0);
				IN.vertex = mul(UNITY_MATRIX_T_MV, IN.vertex); // возвращает обратно к непроецированным координатам
				IN.vertex.w = 1; // работает ааааа!!!
			}

			void surf(Input IN, inout SurfaceOutput o) {
				// Albedo comes from a texture tinted by color
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				o.Alpha = c.a;
			}
			ENDCG


	}
		FallBack "BillboardShader"
}
