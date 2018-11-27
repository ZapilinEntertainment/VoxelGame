Shader "Custom/StarsBillboard"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_Tick("Tick", float) = 0.9
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

		float _Tick;
		v2f vert(appdata_t IN)
		{
			v2f OUT;
			float t = sin(_Time * _Tick + sin (IN.vertex.x) + sin(IN.vertex.y) + sin(IN.vertex.z)) * 0.3 + 0.7;
			OUT.vertex = mul(UNITY_MATRIX_P,
				mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
				- float4(-IN.vertex.x * t, -IN.vertex.y * t, 0.0, 0.0)
				* float4(1, 1, 1.0, 1.0)) ;

			OUT.texcoord = IN.texcoord;
	#ifdef PIXELSNAP_ON
			OUT.vertex = UnityPixelSnap(OUT.vertex);
	#endif

			return OUT;
		}

		sampler2D _MainTex;

		fixed4 frag(v2f IN) : SV_Target
		{
			fixed4 c = tex2D(_MainTex, IN.texcoord);
		c.rgb *= c.a;
		return c;
		}
			ENDCG
		}
		}
}