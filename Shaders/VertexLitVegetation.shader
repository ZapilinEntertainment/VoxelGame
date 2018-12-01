// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Vegetation"
{ // taken from unity examples
	Properties
	{
		_MainTex("Main texture", 2D) = "white" {}
		_MainColor("Main color", Color) = (0.36,0.662,0.015,1)
		_WindVector("WindVector", Vector) = (1,0,0,0) // всегда требуются 4 компонента, нужно задать x и z
	}
		SubShader
		{
			Pass
			{
				Tags {"LightMode" = "ForwardBase"}

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				#include "UnityLightingCommon.cginc"

				struct v2f
				{
					float2 uv : TEXCOORD0;
					fixed4 diff : COLOR0;
					float4 vertex : SV_POSITION;
				};

		float4 _WindVector;
				v2f vert(appdata_base v)
				{
					v2f o;
					float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
					worldPos += sin (_Time * 2) * _WindVector * 0.02 * sin(worldPos.x * worldPos.z + _Time);
					o.vertex = mul(UNITY_MATRIX_VP, worldPos);
					o.uv = v.texcoord ;
					half3 worldNormal = UnityObjectToWorldNormal(v.normal);
					half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					o.diff = nl * _LightColor0;

					// the only difference from previous shader:
					// in addition to the diffuse lighting from the main light,
					// add illumination from ambient or light probes
					// ShadeSH9 function from UnityCG.cginc evaluates it,
					// using world space normal
					o.diff.rgb += ShadeSH9(half4(worldNormal,1));
					return o;
				}

				sampler2D _MainTex;
				half4 _MainColor;

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.uv);
					col.rgb *= i.diff;
					col.rgb *= _MainColor;
					return col;
				}
				ENDCG
			}
		}
}