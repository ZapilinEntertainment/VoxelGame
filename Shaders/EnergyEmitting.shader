Shader "Custom/EnergyEmitting"
{
	Properties
	{
		_MainColor("Main color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			half4 _MainColor;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 rpos : COLOR;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.rpos = (v.vertex.x, v.vertex.y, v.vertex.z);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{		
				fixed4 col= lerp(_MainColor, (1,1,1,1), sin ((10 * _Time + sin(i.rpos.x) + cos(i.rpos.y) + sin (i.rpos.z)) * 6 ) );
				return col;
			}
			ENDCG
		}
	}
}
