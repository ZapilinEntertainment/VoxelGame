// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/MySpecular(Unlit)"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_SpecularIntensity("Specular Intensity", float) = 0
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
		fixed4 col : COLOR0;
		float3 normal : TEXCOORD1;
		float4 posWorld : TEXCOORD2;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	uniform sampler2D _GlobalLightmap;
	float _SpecularIntensity;

	v2f vert(appdata_base v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		float3 normal = UnityObjectToWorldNormal(v.normal);
		o.normal = normal;
		o.posWorld = mul(unity_ObjectToWorld, v.vertex);
		float2 lightUV = float2(normal.x, normal.z);
		if (normal.y < 0) normal.y = 1;
		lightUV = float2(0.5, 0.5) + lightUV * normal.y / 2;
		o.col = tex2Dlod(_GlobalLightmap, float4(lightUV.x, lightUV.y, 0, 0));

		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		// sample the texture
		//fixed4 col = tex2D(_MainTex, i.uv) * i.diff.x * i.diff.y;
		//return col;

		//float lookDot = clamp ( (dot(i.normal, normalize(i.posWorld - _WorldSpaceCameraPos)) -1 ) / (-2), 0, 1);
		//lookDot = pow(lookDot, _Shininess);
		fixed4 texcol = tex2D(_MainTex, i.uv);
		texcol *= (0.2f + i.col.w * 0.8f);
		//return texcol * lookDot * _SpecularIntensity;

		float dotProduct = dot(i.normal, normalize(i.posWorld - _WorldSpaceCameraPos)); // между взглядом и отражением
		dotProduct = clamp((dotProduct - 1)/ (-2), 0, 1);
		float dpt = dotProduct * dotProduct * dotProduct;
		float textureDependence = length(texcol.xyz) * length(texcol.xyz);
		textureDependence *= textureDependence;
		float4 specular =  dpt * dpt * _SpecularIntensity * textureDependence * i.col;
		return  saturate(texcol + specular);
	}
		ENDCG
	}
	}
}
