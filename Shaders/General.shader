Shader "Custom/General(Unlit)"
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
		float3 blockPos : TEXCOORD1;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	uniform sampler3D _GlobalLightmap;
	uniform int chunkSize;

	v2f vert(appdata_base v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);		
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.blockPos = mul(unity_ObjectToWorld, v.vertex + v.normal * 0.5);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 texcol = tex2D(_MainTex, i.uv) ; 
		texcol *=  tex3D(_GlobalLightmap, (0.03125 + i.blockPos.x * 0.0625, 0.03125 + i.blockPos.z * 0.0625, 0.03125 + i.blockPos.y * 0.0625) ).w;
		//texcol.xyz *= tex3D(_GlobalLightmap, i.blockPos / 16.0).w ;
		return texcol;
	}
		ENDCG
	}
	}
}
