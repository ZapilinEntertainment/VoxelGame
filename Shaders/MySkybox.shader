// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

//original: https://github.com/keijiro/UnitySkyboxShaders/blob/master/Assets/Skybox%20Shaders/Gradient%20Skybox.shader
Shader "Custom/MySkybox" 
{
	Properties
	{
		[NoScaleOffset] _layer0("Horizon distortion map", 2D) = "white" {}
		[NoScaleOffset] _starlayer0("Star layer 0", 2D) = "black" {}
		[NoScaleOffset] _starlayer1("Star layer 1", 2D) = "black" {}
		[NoScaleOffset] _cloudlayer("Cloud layer", 2D) = "white" {}

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
		float2 texcoord : TEXCOORD0;
	};

	struct v2f
	{
		float4 position : SV_POSITION;
		float2 texcoord : TEXCOORD0;
		float3 originalPos :COLOR;
	};

	half4 _TopColor, _BottomColor, _HorizonColor;
	half4 _UpVector;
	float _TopExponent, _BottomExponent, _HorizonCompression, _Saturation;
	static const float _Speed = 0.05;
	sampler2D _layer0,  _starlayer0, _starlayer1,_cloudlayer;

	v2f vert(appdata v)
	{
		v2f o;		
		if (v.position.y < 0) {
			v.position.y *= 0.15;
		}
		o.position = UnityObjectToClipPos(v.position);
		o.texcoord = v.texcoord;
		o.originalPos = normalize(mul(unity_ObjectToWorld, v.position));
		return o;
	}



	fixed4 frag(v2f i) : COLOR
	{ 

		float p = i.originalPos.y;
		float p1 = 1 - pow(min(1, 1 - p), _TopExponent);
		float p3 = 1 - pow(min(1, 1 + p), _BottomExponent);
		float p2 = 1 - p1 - p3;
		p2 *= pow(1 - sin(saturate(abs(p))), _HorizonCompression);

		//horizon light distortion
		float z = i.originalPos.z, x = i.originalPos.x, y = i.originalPos.y, t = _Time * _Speed;
		float distex = tex2D(_layer0, float2(x, z));
		float distortion = sin( (z + x) * 10 + t * 10) * (0.3 + 0.3 * sin(t) ) + cos(z + x + t + 3.14) * 0.3 + distex * 0.4 ;	

		//stars layer:
		float xp = x + t , xp2 = x + t * 0.75, zp = z + t * 0.025, zp2 = z - t * 0.012;
		fixed4 sc0 = tex2D(_starlayer0, float2(xp, z)), sc1 = tex2D(_starlayer1, float2(0.5 + xp2 / 2, 0.5 +z /2));
		fixed4 starCol = _TopColor;
		float colorVal = (1 - _TopColor.rgb) * 0.9 + 0.1;
		float heightVal = (y + y * sign(y)) / 2;
		heightVal -= 0.3;
		heightVal = (heightVal + heightVal * sign(heightVal)) / 2;
		heightVal = sin(heightVal * 2.244); // x/ 0.7 * pi / 2 
		starCol.r += (sc0.r * sc0.a + sc1.r * sc1.a) * colorVal * heightVal;
		starCol.g += (sc0.g * sc0.a + sc1.g * sc1.a)* colorVal* heightVal;
		starCol.b += (sc0.b * sc0.a + sc1.b * sc1.a)* colorVal* heightVal;

		//cloud layer:
		//xp = ( i.texcoord.x  + distex * 0.01 + t * 10 ) * 3;
		//zp = (i.texcoord.y +0.25 * t * distex* 0.2) * 3 + _SinTime;

		xp = i.texcoord.x + t * 20 + distex* 0.02; zp = i.texcoord.y + t * 5 + distex * 0.1;

		fixed4 cloudCol = tex2D(_cloudlayer, float2(xp , zp) );
		fixed4 bCol = _BottomColor;
		bCol *= (1 - cloudCol.a * (1 - sin(z * 4) * sin(x * 5 + _Time * _Saturation) ) / 2 * (0.3 * distex + 0.7)) ;

		half4 col =
			starCol * i.originalPos.y * p1 +
			_HorizonColor *p2 * _Saturation  * (2 +  sin(distortion) * 0.5) / 1.4
			+ bCol * p3;

		return  col;
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