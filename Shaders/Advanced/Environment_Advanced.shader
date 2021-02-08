Shader "Custom/AdvancedEnvironment"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _PropertiesMap("Properties map", 2D) = "gray" {}
		[NoScaleOffset] _OcclusionMap("Occlusion map", 2D) = "white" {}
		// r - specular
		// g - smoothness
		// b - occlusion
		// a  - 1 - emission
		_VisibilityRadius("Visibility radius", float) = 9
		_BottomVisibilityBorder("Bottom visibility border", float) = -1
		_SaturationMultiplier("SaturationMultiplier", float) = 0.0562327
		_FogColor("Fog color", Color) = (1,1,1)
		_Cutoff("Alpha cutoff", Range(0,1)) = 0
    }
    SubShader
    {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200
		Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert alphatest:_Cutoff

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex, _PropertiesMap, _OcclusionMap;



		struct Input
		{
			float2 uv_MainTex : TEXCOORD0;
			float distance : TEXCOORD0;
		};
        fixed4 _Color;
		half _VisibilityRadius, _BottomVisibilityBorder;
		float _SaturationMultiplier;
		half3 _FogColor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			float rangeSaturate = sqrt(worldPos.x * worldPos.x + worldPos.z * worldPos.z) / _VisibilityRadius;
			float heightSaturate = (worldPos.y - _BottomVisibilityBorder);
			o.distance = (1 - saturate(rangeSaturate)) * saturate(heightSaturate);
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 col = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			float a = IN.distance * (3.55 - 2.55 * IN.distance);
			o.Albedo = col.rgb * a + _FogColor * (1 - a);
			o.Alpha = saturate(IN.distance / _SaturationMultiplier);
			//
			fixed4 props = tex2D(_PropertiesMap, IN.uv_MainTex);
			//o.Metallic = props.r;
			o.Smoothness = props.g;
			o.Occlusion = tex2D(_OcclusionMap, IN.uv_MainTex);
			//o.Emission = 1 - props.a;
        }
        ENDCG
    }
    FallBack "EnvironmentObject"
}
