Shader "Custom/LocationLighted" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="TransparentCutout" "DisableBatching" = "True" "RenderType" = "Transparent" "ForceNoShadowCasting" = "True" }
		LOD 200
		Blend One OneMinusSrcAlpha

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf SimpleLambert vertex:vert alphatest:_Cutoff

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};
		fixed4 _Color;

		half4 LightingSimpleLambert(SurfaceOutput s, half3 lightDir, half atten) {
			half4 c;
			c.rgb = s.Albedo * 0.5f  *   atten;
			c.a = s.Alpha;
			return c;
		}

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}

		void vert(inout appdata_full v, out Input o)
			//void vert(inout appdata_full v)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			// get the camera basis vectors
			float3 forward = -normalize(UNITY_MATRIX_V._m20_m21_m22);
			float3 up = float3(0, 1, 0); //normalize(UNITY_MATRIX_V._m10_m11_m12);
			float3 right = normalize(UNITY_MATRIX_V._m00_m01_m02);

			// rotate to face camera
			float4x4 rotationMatrix = float4x4(right, 0,
				up, 0,
				forward, 0,
				0, 0, 0, 1);

			//float offset = _Object2World._m22 / 2;
			float offset = 0;
			v.vertex = mul(v.vertex + float4(0, offset, 0, 0), rotationMatrix) + float4(0, -offset, 0, 0);
			v.normal = mul(v.normal, rotationMatrix);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
