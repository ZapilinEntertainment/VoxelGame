Shader "Custom/vertexLitOutlined"
{ // taken from unity examples
	Properties
	{
		[NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
		_MainColor("Main color", Color) = (1,1,1,1)
		_Illumination("Illumination", float) = 0.9
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, 2)) = 0.3
	}

		CGINCLUDE
#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc"	

			struct v2f
		{
			float2 uv : TEXCOORD0;
			fixed4 diff : COLOR0;
			float4 vertex : SV_POSITION;
		};

		uniform float _Outline;
		uniform float4 _OutlineColor;
		float _Illumination;		
		ENDCG

			SubShader
		{
			Pass { // outline
				Name "OUTLINE"
				Tags { "LightMode" = "Always" }
				Cull Off
				ZWrite Off
				ColorMask RGB // alpha not used

				// you can choose what kind of blending mode you want for the outline
				Blend SrcAlpha OneMinusSrcAlpha // Normal
				//Blend One One // Additive
				//Blend One OneMinusDstColor // Soft Additive
				//Blend DstColor Zero // Multiplicative
				//Blend DstColor SrcColor // 2x Multiplicative

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				v2f vert(appdata_base v) {
					// just make a copy of incoming vertex data but scaled according to normal direction
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);

					float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
					float2 offset = TransformViewToProjection(norm.xy);

					o.vertex.xy += offset * o.vertex.z * _Outline;
					o.diff = _OutlineColor;
					return o;
				}

				half4 frag(v2f i) : COLOR {
					return i.diff;
				}
				ENDCG
			}
			Pass //normal drawing
			{
				Tags {"LightMode" = "ForwardBase"}

				CGPROGRAM		
					#pragma vertex vert
				#pragma fragment frag

				v2f vert(appdata_base v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.texcoord;
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
					col.rgb *= _Illumination;
					return col;
				}
				ENDCG
			}
		}
}