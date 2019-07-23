Shader "Custom/AdvancedMetalShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _EffectTex("Effect texture", 2D) = "white" {}
		[NoScaleOffset] _EffectVisibilityTex("Effect visibility texture", 2D) = "white"{}
		[NoScaleOffset] _PropertiesMap("Properties map", 2D) = "white" {} 
		// r - metallic
		// g - smoothness
		_Speed("Speed", float) = 0.16
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex, _EffectTex, _EffectVisibilityTex, _PropertiesMap;
		float _Speed;

        struct Input
        {
            float2 uv_MainTex;
        };
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			fixed4 col = tex2D(_MainTex, IN.uv_MainTex)  * _Color;
			float a = tex2D(_EffectVisibilityTex, IN.uv_MainTex + (0, 1) * _Time * _Speed).w;
			fixed4 effectColor = tex2D(_EffectTex, IN.uv_MainTex);
			col.rgb = col.rgb * (1 - effectColor.w * a) + effectColor.rgb * a * effectColor.w;
			o.Albedo = col;

			fixed4 emap = tex2D(_PropertiesMap, IN.uv_MainTex);
            o.Metallic = emap.r * (1- 0.3 * a);
            o.Smoothness = emap.g ;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
