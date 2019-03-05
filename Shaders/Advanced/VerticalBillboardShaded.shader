Shader "Custom/VerticalBillboardShaded" {
	//AXIS-ALIGNED SHADED
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
				float4 invertex = mul(UNITY_MATRIX_M, float4(IN.vertex.x, IN.vertex.y, IN.vertex.z, 0));
				float4x4 rm = UNITY_MATRIX_V;
				//переписываем матрицу будто sin z = 0, cos z= 1
				float sinY = rm[0, 2], cosX = rm[2,2] / (rm[1,2] / sinY), sinX = sqrt(1 - cosX * cosX) ;
				rm[1, 0] = sinX * sinY;
				rm[2, 0] = -1 * cosX * sinY;
				rm[3, 0] = 0;
				rm[1, 1] = cosX;
				rm[2, 1] = sinX;
				rm[3, 1] = 0;
				rm[3, 3] = 0;

				invertex = mul(rm, invertex);
				IN.vertex = mul(UNITY_MATRIX_T_MV, invertex); // возвращает обратно к непроецированным координатам
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
