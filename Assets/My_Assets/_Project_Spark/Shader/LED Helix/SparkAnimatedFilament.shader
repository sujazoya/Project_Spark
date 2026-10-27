Shader "ProjectSpark/AnimatedFilamentThermal"
{
    Properties
    {
        [MainColor] _BaseColor ("Cold Tungsten", Color) = (0.07, 0.035, 0.022, 1)
        [HDR] _EmissionColor ("Solver Emission Drive", Color) = (0, 0, 0, 1)
        _FilamentHeat ("Solver Filament Heat", Range(0,1)) = 0
        [HDR] _WarmColor ("Warm Orange", Color) = (1.0, 0.10, 0.012, 1)
        [HDR] _HotColor ("White Hot", Color) = (1.0, 0.92, 0.70, 1)
        _PulseSpeed ("Subtle Heat Drift Speed", Range(0,20)) = 1.5
        _PulseAmount ("Heat Drift Amount", Range(0,0.25)) = 0.04
        _HotspotScale ("Hotspot Spatial Scale", Range(0,80)) = 24
        _HotspotAmount ("Hotspot Strength", Range(0,0.35)) = 0.10
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "FilamentThermalUnlit"
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionOS : TEXCOORD0; };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _EmissionColor;
                float _FilamentHeat;
                half4 _WarmColor;
                half4 _HotColor;
                float _PulseSpeed;
                float _PulseAmount;
                float _HotspotScale;
                float _HotspotAmount;
            CBUFFER_END

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionOS = IN.positionOS.xyz;
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                float heat = saturate(_FilamentHeat);
                float drive = max(_EmissionColor.r, max(_EmissionColor.g, _EmissionColor.b));

                // Slow-moving nonuniform temperature pattern along the helix.
                float3 p = IN.positionOS;
                float t = _Time.y;
                float a = sin(p.y * _HotspotScale + p.x * 7.3 - t * _PulseSpeed);
                float b = sin(p.z * (_HotspotScale * 0.7) - p.y * 5.1 + t * (_PulseSpeed * 0.61));
                float modulation = max(0.1, 1.0 + _PulseAmount * a + _HotspotAmount * b);

                // Red -> orange -> bright yellow-white as the filament heats.
                float whiteHot = smoothstep(0.32, 0.95, heat);
                float3 thermalColor = lerp(_WarmColor.rgb, _HotColor.rgb, whiteHot);

                float3 coldBody = _BaseColor.rgb;
                float3 glow = thermalColor * drive * modulation;
                return half4((half3)(coldBody + glow), 1.0h);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
