Shader "Unlit/General"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"


	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
		fixed4 col : COLOR;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	uniform sampler2D _GlobalLightmap;

	v2f vert(appdata_base v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);		
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		float3 normal = UnityObjectToWorldNormal(v.normal);
		float2 lightUV = float2(normal.x, normal.z);
		if (normal.y < 0) normal.y = 1;
		lightUV = float2(0.5, 0.5) + lightUV * normal.y / 2;
		o.col = tex2Dlod(_GlobalLightmap, float4(lightUV.x, lightUV.y, 0,0));
		
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 texcol = tex2D(_MainTex, i.uv) ; 
		return texcol * (0.3f + i.col.w * 0.7f);
	}
		ENDCG
	}
	}
}
