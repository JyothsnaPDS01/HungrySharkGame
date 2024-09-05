Shader "Custom/CombinedShader"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1) // New parameter for base texture color
        _BaseIntensity ("Base Intensity", Range(0, 2)) = 1.0 // New parameter for base texture intensity
        _CausticsTex ("Caustics Texture", 2D) = "white" {}
        _CausticsColor ("Caustics Color", Color) = (1,1,1,1)
        _CausticsIntensity ("Caustics Intensity", Range(0, 2)) = 1.0
        _Speed ("Caustics Speed", Range(0.025, 10.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _MainTex;
        sampler2D _CausticsTex;
        fixed4 _BaseColor; // New variable for base color
        float _BaseIntensity; // New variable for base intensity
        fixed4 _CausticsColor;
        float _CausticsIntensity;
        float _Speed;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_CausticsTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Base texture
            fixed4 baseColor = tex2D(_MainTex, IN.uv_MainTex) * _BaseColor;
            baseColor.rgb *= _BaseIntensity; // Apply intensity to base texture

            // Caustics texture
            float2 causticsUV = IN.uv_CausticsTex;
            causticsUV.y += _Time.y * _Speed;
            fixed4 causticsColor = tex2D(_CausticsTex, causticsUV) * _CausticsColor;

            // Combine the caustics with the base texture
            baseColor.rgb += causticsColor.rgb * _CausticsIntensity;

            o.Albedo = baseColor.rgb;
            o.Alpha = baseColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
