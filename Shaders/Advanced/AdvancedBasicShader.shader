Shader "Custom/AdvancedBasicShader"
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

        sampler2D _MainTex, _PropertiesMap, _OcclusionMap;

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
			fixed4 props = tex2D(_PropertiesMap, IN.uv_MainTex);
			//o.Metallic = props.r;
			o.Smoothness = props.g;
			o.Occlusion = tex2D(_OcclusionMap, IN.uv_MainTex);
			//o.Emission = 1 - props.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
