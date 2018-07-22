Shader "Unlit/MySpecular"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_LightPos("Light position", Vector) = (0,0,-1)
		_SpecularIntensity("Specular Intensity", float) = 0
		_Shininess("Shininess", float) = 0
		_SpecularColor("Specular Color", Color) = (1,1,1,1)
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
		float4 diff : COLOR0;
		float3 normal : TEXCOORD1; 
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	float3 _LightPos;
	float _SpecularIntensity;
	float _Shininess;
	float4 _SpecularColor;

	v2f vert(appdata_base v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		float nl = max(0.3, dot(UnityObjectToWorldNormal(v.normal), _LightPos.xyz));;
		// factor in the light color
		o.diff.x = nl;
		float reflPower = clamp(dot(UnityObjectToWorldNormal(v.normal), _WorldSpaceCameraPos), 0.5,1);
		o.diff.y = reflPower;
		o.normal = UnityObjectToWorldNormal(v.normal);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		// sample the texture
		//fixed4 col = tex2D(_MainTex, i.uv) * i.diff.x * i.diff.y;
		//return col;
		float3 r = normalize(2 * dot(_LightPos, i.normal) * i.normal - _LightPos);

		float dotProduct = dot(r, _WorldSpaceCameraPos);
		float4 specular = _SpecularIntensity * _SpecularColor * dotProduct;

		return saturate(tex2D(_MainTex, i.uv) * i.diff.x + specular);
	}
		ENDCG
	}
	}
}
