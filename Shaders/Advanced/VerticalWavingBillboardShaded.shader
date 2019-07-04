Shader "Custom/VerticalWavingBillboardShaded" {
	//AXIS-ALIGNED WAVING SHADED
Properties{
	_Color("Color", Color) = (1,1,1,1)
	_MainTex("Albedo (RGB)", 2D) = "white" {}
}
SubShader{
Tags
{
	"RenderType" = "Transparent"
	"Queue" = "Transparent"
	"ForceNoShadowCasting" = "True"
}
	LOD 200
	Cull Off
	Blend One OneMinusSrcAlpha

	CGPROGRAM
	#pragma surface surf Lambert vertex:vert alpha:fade
	#include "AutoLight.cginc"

	uniform float _Windpower;
	sampler2D _MainTex;
struct v2f
{
	float2 uv : TEXCOORD0;
	float4 vertex : SV_POSITION;
};
	struct Input {
		half3 vertex;
		float2 uv_MainTex;
	};

	fixed4 _Color;

	void vert(inout appdata_base IN)
	{
		//float4 invertex = mul(UNITY_MATRIX_MV, float4(0, IN.vertex.y, 0, 0));
		//invertex.x -= IN.vertex.x + IN.vertex.y * sin(_Time * 10) * _Windpower * 0.2f;
		//invertex.z -= IN.vertex.z - IN.vertex.y *sin(_Time * 10) * _Windpower * 0.2f;
		//invertex.x += IN.vertex.x;
		//IN.vertex = mul(UNITY_MATRIX_T_MV, invertex);
		float4 invertex = mul(UNITY_MATRIX_MV, float4(0, IN.vertex.y, 0, 0));
		invertex.x += IN.vertex.x + IN.vertex.y * sin(_Time * 10) * _Windpower * 0.02f;
		invertex.x += IN.vertex.x;
		IN.vertex = mul(UNITY_MATRIX_T_MV, invertex);
	}

	void surf(Input IN, inout SurfaceOutput o) {
		// Albedo comes from a texture tinted by color
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
		o.Albedo = c.rgb;
		o.Alpha = c.a;
	}
	ENDCG


}
FallBack "VerticalBillboard"
}
