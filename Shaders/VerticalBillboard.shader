Shader "Custom/VerticalBillboard"
{
	// AXIS-ALIGNED
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_MainColor("Color", Color) = (1,1,1,1)
	}

		SubShader
		{
			Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
			"DisableBatching" = "True"
		}

			Cull Off
			Lighting Off
			ZWrite Off
			Fog{ Mode Off }
			Blend One OneMinusSrcAlpha

			Pass
		{
			CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma multi_compile DUMMY PIXELSNAP_ON
	#include "UnityCG.cginc"

			struct appdata_t
		{
			float3 vertex   : POSITION;
			float2 texcoord : TEXCOORD0;
		};

		struct v2f
		{
			float4 vertex   : SV_POSITION;
			half2 texcoord  : TEXCOORD0;
		};

		v2f vert(appdata_t IN)
		{
			v2f OUT;
			float4 invertex = mul(UNITY_MATRIX_MV, float4(0, IN.vertex.y, 0, 1));
			invertex.x -= IN.vertex.x;
			invertex.z -= IN.vertex.z;
			OUT.vertex = mul(UNITY_MATRIX_P, invertex);
			OUT.texcoord = IN.texcoord;
			return OUT;
		}

		sampler2D _MainTex;
		half4 _MainColor;

		fixed4 frag(v2f IN) : SV_Target
		{
			fixed4 c = tex2D(_MainTex, IN.texcoord);
			c.rgb *= _MainColor;
			c.rgb *= c.a;
		return c;
		}
			ENDCG
		}
		}
}