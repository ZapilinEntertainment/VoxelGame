Shader "Custom/Billboard"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_MainColor("Color", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
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
#pragma multi_compile _ SHADOWS_SCREEN
#include "UnityCG.cginc"
#include "AutoLight.cginc"

		struct appdata_t
	{
		float3 vertex   : POSITION;
		float2 texcoord : TEXCOORD0;
	};

	struct v2f
	{
		float4 vertex   : SV_POSITION;
		half2 texcoord  : TEXCOORD0;
#if defined(SHADOWS_SCREEN)
		float4 shadowCoordinates : TEXCOORD5;
#endif
	};


	v2f vert(appdata_t IN)
	{
		v2f OUT;
#if defined(SHADOWS_SCREEN)
		OUT.shadowCoordinates = UnityObjectToClipPos(IN.vertex);
#endif
		OUT.vertex = mul(UNITY_MATRIX_P,
			mul(UNITY_MATRIX_MV, float4(0, 0, 0, 1))
			- float4(-IN.vertex.x, -IN.vertex.y, 0, 0));

		OUT.texcoord = IN.texcoord;
#ifdef PIXELSNAP_ON
		OUT.vertex = UnityPixelSnap(OUT.vertex);
#endif

		return OUT;
	}

	sampler2D _MainTex;
	half4 _MainColor;

	fixed4 frag(v2f IN) : SV_Target
	{
		fixed4 c = tex2D(_MainTex, IN.texcoord);
		c.rgb *= _MainColor ;
		c.rgb *= c.a;
		return c;
	}
		ENDCG
	}
	}
}