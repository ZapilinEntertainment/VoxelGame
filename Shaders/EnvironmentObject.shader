
Shader "Custom/EnvironmentObject"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_VisibilityRadius("Visibility radius", float) = 9
		_BottomVisibilityBorder("Bottom visibility border", float) = -1
		_SaturationMultiplier("SaturationMultiplier", float) = 0.0562327
		_FogColor("Fog color", Color) = (1,1,1)
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float distance : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half _VisibilityRadius, _BottomVisibilityBorder;
			float _SaturationMultiplier;
			half3 _FogColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				/* still not usable(
				float4x4 m = UNITY_MATRIX_V;
				m[0, 3] /= 5;
				m[1, 3] /= 5;
				m[2, 3] /= 5;
				o.vertex = mul(UNITY_MATRIX_P,
					mul(m, mul(UNITY_MATRIX_M,float4(v.vertex.x, v.vertex.y, v.vertex.z, 1)))
				);
				*/
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float rangeSaturate = sqrt( worldPos.x * worldPos.x + worldPos.z * worldPos.z ) / _VisibilityRadius;
				float heightSaturate = (worldPos.y - _BottomVisibilityBorder);
				o.distance = (1 - saturate(rangeSaturate)) * saturate(heightSaturate);
				return o;
				//copy to Environment_Advanced
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				float a = i.distance * (3.55 - 2.55 * i.distance);
				col.xyz = col.xyz * a + _FogColor * (1 - a);
				col.a *= saturate(i.distance / _SaturationMultiplier);
				return col;
			}
			ENDCG
		}
	}
}
