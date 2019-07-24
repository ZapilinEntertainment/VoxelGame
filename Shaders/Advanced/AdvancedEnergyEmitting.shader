Shader "Custom/AdvancedEnergyEmitting" {
	Properties{
		_Color("Color", Color) = (0,1,1,1)
	}

		CGINCLUDE
#include "UnityCG.cginc"
		sampler2D _FirstRender;
		half4 _Color;
	ENDCG

		SubShader{
			Pass {
				//Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
			Tags { "RenderType" = "Opaque" }
				//ZWrite Off
			//Blend One OneMinusSrcAlpha
			Lighting Off

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				struct appdata {
					float4 vertex : POSITION;					
				};
				struct v2f {
					float4 vertex : POSITION;
					float4 realpos : TEXCOORD;
				};

				v2f vert(appdata v) {
					v2f o;
					o.realpos = UnityObjectToClipPos(v.vertex);
					float3 viewCenter = mul(unity_CameraToWorld, float3(0.5, 0.5, 1));
					o.vertex = mul(UNITY_MATRIX_MV, float4(v.vertex.xyz,1));
					//o.vertex.xyz += normalize(ObjSpaceViewDir(o.vertex));
					o.vertex.xyz += 
					o.vertex = mul(UNITY_MATRIX_P, o.vertex);
					return o;
				}
				half4 frag(v2f v) : SV_Target {
					half4 col = _Color;
					return col;
				}
				ENDCG
			}
	}
}