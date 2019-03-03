//https://www.youtube.com/watch?v=SlTkBe4YNbo
Shader "Custom/AdvancedEnergyEmitting" {
	Properties{
		_Color ("Color", Color) = (0,1,1,1)
		_Thickness("Halo thickness", float) = 1
		_Intensity("Intensity", float) = 1
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	half4 _Color;
	float _Intensity, _Thickness;
	ENDCG

	SubShader{
		//outline
		Pass {
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
			ZWrite Off
		Blend One OneMinusSrcAlpha
		Lighting Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata {
				float4 vertex : POSITION;				
			};
				struct v2f {
					float4 vertex : POSITION;
					float distance : COLOR;
				};

			v2f vert(appdata v) {
				float impact = sin(_Time * 5 * ((v.vertex.x + v.vertex.y + v.vertex.z) * 3.14));
				v.vertex *= (1 + _Thickness + impact * _Intensity);
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.distance = sin((impact + 1)) / 2;
				return o;
			}
			half4 frag(v2f v) : SV_Target {
				half4 col = _Color;
				col.a = v.distance;
				return col;
			}
			ENDCG
		}
		//object itself
		Pass
		{
			ZWrite On
			Material {
				Diffuse[_Color]
			}
			Lighting Off
		}
	}
}