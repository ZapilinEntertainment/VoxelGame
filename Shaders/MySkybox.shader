//original: https://github.com/keijiro/UnitySkyboxShaders/blob/master/Assets/Skybox%20Shaders/Gradient%20Skybox.shader
Shader "Custom/MySkybox" 
{
	Properties
	{
		[NoScaleOffset] _layer0("Distortion map", CUBE) = "white" {}

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
	samplerCUBE _layer0;

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

		i.texcoord.x = i.texcoord.x * 3 + _Time * 0.12;
		i.texcoord.y = i.texcoord.y ;
		fixed4 dmap = texCUBE(_layer0, i.texcoord);
		i.texcoord.x = i.texcoord.x * 3 - _Time * 0.08;
		fixed4 dmap2 = texCUBE(_layer0, i.texcoord);
		half4 col =
			//_TopColor * p1 +
			_HorizonColor * dmap.a * dmap2.a *p2 * _Saturation * 1.5;
			//+ _BottomColor * p3;
		
		//col.r = col.r * col0.a;
		//col.g = col.g * col0.a;
		//col.b = col.b * col0.a;
		return col;
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