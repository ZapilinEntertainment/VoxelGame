
Shader "Custom/ENV_Coloured"
{
	Properties
	{
		_MainColor("Color", Color) = (0,1,1)
		_VisibilityRadius("Visibility radius", float) = 9
		_BottomVisibilityBorder("Bottom visibility border", float) = -1
		_SaturationMultiplier("SaturationMultiplier", float) = 0.0562327
		_FogColor("Fog color", Color) = (1,1,1)
	}
		SubShader
		{
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
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
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float distance : TEXCOORD1;
				};

				half3 _MainColor;
				half _VisibilityRadius, _BottomVisibilityBorder;
				float _SaturationMultiplier;
				half3 _FogColor;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					float rangeSaturate = sqrt(worldPos.x * worldPos.x + worldPos.z * worldPos.z) / _VisibilityRadius;
					float heightSaturate = (worldPos.y - _BottomVisibilityBorder);
					o.distance = (1 - saturate(rangeSaturate)) * saturate(heightSaturate);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{					
					float a = i.distance * (3.55 - 2.55 * i.distance);
					fixed3 col = _MainColor * a + _FogColor * (1 - a);
					a = saturate(i.distance / _SaturationMultiplier);
					return fixed4(col,a);
				}
				ENDCG
			}
		}
}
