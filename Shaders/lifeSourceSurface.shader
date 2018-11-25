// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/lifeSourceSurface"
{
	Properties
	{
		[NoScaleOffset] _streamTexture("stream texture", 2D) = "white" {}
		[NoScaleOffset] _backgroundTexture("background texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _streamTexture, _backgroundTexture;

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 effectUV : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				o.effectUV = length(v.vertex) * (1,1) * sin (0.03 * _Time);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 streamcol = tex2D(_streamTexture, i.effectUV);
				fixed4 col = tex2D(_backgroundTexture, i.uv);
				col.xyz = col.xyz * (1 - streamcol.w) + streamcol.xyz * streamcol.w ;
				return col;
			}
			ENDCG
		}
	}
}
