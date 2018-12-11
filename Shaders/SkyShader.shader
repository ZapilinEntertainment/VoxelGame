// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/SkyShader"
{
	Properties
	{
		[NoScaleOffset] _layer0 ("Star layer 0", 2D) = "white" {}
		[NoScaleOffset] _layer1("Star layer 1", 2D) = "white" {}
		[NoScaleOffset] _layer2("Star layer 2", 2D) = "white" {}
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

			sampler2D _layer0, _layer1, _layer2, _backgroundTexture;

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 alphaChannels : COLOR0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				o.uv.x += _Time * 0.025;
				o.uv.y += _Time * 0.005;
				float l = length(o.vertex);
				o.alphaChannels.x = _SinTime / 2 + 0.5;
				o.alphaChannels.y = sin(_Time * 3 + 4 + l) / 2 + 0.5;
				o.alphaChannels.z = sin(_Time * 7 + 1.5f + l) / 2 + 0.5;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_backgroundTexture, i.uv);
				fixed4 col0 = tex2D(_layer0, i.uv);
				fixed4 col1 = tex2D(_layer1, i.uv) ;
				fixed4 col2 = tex2D(_layer2, i.uv) ;
				col.rgb += (col0.rgb * col0.a * i.alphaChannels.x + col1.rgb * col1.a * i.alphaChannels.y + col2.rgb * col2.a * i.alphaChannels.z);
				return col;
			}
			ENDCG
		}
	}
}
