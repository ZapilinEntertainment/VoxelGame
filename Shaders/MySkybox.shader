//original https://github.com/keijiro/UnitySkyboxShaders/blob/master/Assets/Skybox%20Shaders/Gradient%20Skybox.shader
Shader "Custom/MySkybox" 
{
	Properties
	{
		_BottomColor("Bottom color", Color) = (1, 1, 1, 0)
		_TopColor("Top color", Color) = (1, 1, 1, 0)
		_HorizonColor("Horizon color", Color) = (1,1,1,0)
		_UpVector("Up Vector", Vector) = (0, 1, 0, 0)
		_HorizonCompression("Horizon Compression", Float) = 1
		_TopExponent("Top exponent", Float) = 1.0
		_BottomExponent("Bottom exponent", Float) = 1.0

		_Saturation("Saturation", Range(0,1)) = 1
	}

		CGINCLUDE

#include "UnityCG.cginc"

		struct appdata
	{
		float4 position : POSITION;
		float3 texcoord : TEXCOORD0;
	};

	struct v2f
	{
		float4 position : SV_POSITION;
		float3 texcoord : TEXCOORD0;
	};

	half4 _TopColor, _BottomColor, _HorizonColor;
	half4 _UpVector;
	half _HorizonCompression, _Saturation;
	half _TopExponent, _BottomExponent;

	v2f vert(appdata v)
	{
		v2f o;		
		o.position = UnityObjectToClipPos(v.position);
		o.texcoord = v.texcoord;
		return o;
	}

	fixed4 frag(v2f i) : COLOR
	{ 
		float3 v = normalize(i.texcoord);

		float p = v.y;
		float p1 = 1 - pow(min(1, 1 - p), _TopExponent);
		float p3 = 1 - pow(min(1, 1 + p), _BottomExponent);
		float p2 = 1 - p1 - p3;
		p2 *= pow(1 - sin(saturate(abs(p))), _HorizonCompression);

		half4 c_sky = _TopColor * p1 * _Saturation + _HorizonColor * p2 *sin (_Saturation * 1.58) + _BottomColor * p3*_Saturation;
		return c_sky;
	}

		ENDCG

		SubShader
	{
		Tags{ "RenderType" = "Background" "Queue" = "Background" }
			Pass
		{
			ZWrite Off
			Cull Off
			Fog { Mode Off }
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}