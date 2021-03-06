﻿Shader "Custom/Environment_Advanced_Metal"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _PropertiesMap("Properties map", 2D) = "white" {}
		// r - metallic
		// g - smoothness
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

        sampler2D _MainTex,_PropertiesMap;
		half _VisibilityRadius, _BottomVisibilityBorder;
		float _SaturationMultiplier;
		half3 _FogColor;

        struct Input
        {
            float2 uv_MainTex;
			float distance : TEXCOORD0;
        };

        fixed4 _Color;

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
			fixed4 col = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			float a = IN.distance * (3.55 - 2.55 * IN.distance);
			o.Albedo = col.rgb * a + _FogColor * (1 - a);
			o.Alpha = saturate(IN.distance / _SaturationMultiplier);
			fixed4 emap = tex2D(_PropertiesMap, IN.uv_MainTex);
			o.Metallic = emap.r * (1 - 0.3 * a);
			o.Smoothness = emap.g;
        }
        ENDCG
    }
    FallBack "EnvironmentObject"
}
