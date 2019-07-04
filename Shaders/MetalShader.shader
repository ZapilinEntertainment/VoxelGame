Shader "Custom/Metal Shader"
{ // taken from unity examples
	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
		[NoScaleOffset] _EffectTex("Effect texture", 2D) = "white" {}
		[NoScaleOffset] _EffectVisibilityTex("Effect visibility texture", 2D) = "white"{}
		_MainColor("Main color", Color) = (1,1,1,1)
		_Illumination("Illumination", float) = 0.9
			_Speed("Speed", float) = 0.16
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

				v2f vert(appdata_base v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.texcoord;
					half3 worldNormal = UnityObjectToWorldNormal(v.normal);
					half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					o.diff = nl * _LightColor0;
					o.diff.rgb += ShadeSH9(half4(worldNormal,1));
					return o;
				}

				sampler2D _MainTex, _EffectTex, _EffectVisibilityTex;
				half4 _MainColor;
				float _Illumination, _Speed;

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.uv);
					col.rgb *= i.diff;
					col.rgb *= _MainColor;
					col.rgb *= _Illumination;
					float a = tex2D(_EffectVisibilityTex, i.uv + (0,1) * _Time * _Speed).w ;
					fixed4 effectColor = tex2D(_EffectTex, i.uv);
					col.rgb = col.rgb * (1 - effectColor.w * a) + effectColor.rgb * a * effectColor.w;
					// 
					return col;
				}
				ENDCG
			}
		}
}