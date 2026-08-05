Shader "ProjectSpark/VFX/Spark Advanced Glow"
{
    Properties
    {
        // ============================================================
        // BASE
        // ============================================================

        [MainTexture]
        _BaseMap ("Base Texture", 2D) = "white" {}

        [MainColor]
        [HDR]
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        [HDR]
        _GlowColor ("Glow Color", Color) = (0,0.8,1,1)

        _GlowIntensity ("Glow Intensity", Range(0,20)) = 4


        // ============================================================
        // FRESNEL
        // ============================================================

        _FresnelEnabled ("Fresnel Enabled", Range(0,1)) = 1

        [HDR]
        _FresnelColor ("Fresnel Color", Color) = (0,0.5,1,1)

        _FresnelPower ("Fresnel Power", Range(0.1,10)) = 3

        _FresnelIntensity ("Fresnel Intensity", Range(0,20)) = 3


        // ============================================================
        // PULSE
        // ============================================================

        _Pulse ("Pulse", Range(0,1)) = 0

        _PulseIntensity ("Pulse Intensity", Range(0,10)) = 3


        // ============================================================
        // SCAN
        // ============================================================

        _ScanEnabled ("Scan Enabled", Range(0,1)) = 1

        [HDR]
        _ScanColor ("Scan Color", Color) = (0,1,1,1)

        _ScanSpeed ("Scan Speed", Range(-10,10)) = 2

        _ScanScale ("Scan Scale", Range(1,100)) = 20

        _ScanWidth ("Scan Width", Range(0.001,1)) = 0.12

        _ScanIntensity ("Scan Intensity", Range(0,20)) = 4


        // ============================================================
        // ENERGY SWEEP
        // ============================================================

        _SweepEnabled ("Sweep Enabled", Range(0,1)) = 1

        [HDR]
        _SweepColor ("Sweep Color", Color) = (1,1,1,1)

        _SweepPosition ("Sweep Position", Range(-2,2)) = -1

        _SweepWidth ("Sweep Width", Range(0.01,1)) = 0.2

        _SweepIntensity ("Sweep Intensity", Range(0,20)) = 5

        _SweepSpeed ("Sweep Speed", Range(-10,10)) = 1


        // ============================================================
        // NOISE
        // ============================================================

        _NoiseEnabled ("Noise Enabled", Range(0,1)) = 0

        [NoScaleOffset]
        _NoiseMap ("Noise Texture", 2D) = "white" {}

        _NoiseScale ("Noise Scale", Range(0.1,30)) = 5

        _NoiseSpeed ("Noise Speed", Range(-10,10)) = 1

        _NoiseStrength ("Noise Strength", Range(0,1)) = 0.25

        [HDR]
        _NoiseColor ("Noise Color", Color) = (0,0.5,1,1)

        _NoiseIntensity ("Noise Intensity", Range(0,20)) = 2


        // ============================================================
        // DISSOLVE
        // ============================================================

        _DissolveEnabled ("Dissolve Enabled", Range(0,1)) = 0

        [NoScaleOffset]
        _DissolveMap ("Dissolve Texture", 2D) = "white" {}

        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0

        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0.001,0.5)) = 0.05

        [HDR]
        _DissolveEdgeColor ("Dissolve Edge Color", Color) = (0,1,1,1)

        _DissolveEdgeIntensity ("Dissolve Edge Intensity", Range(0,30)) = 8


        // ============================================================
        // DISTORTION
        // ============================================================

        _DistortionEnabled ("Distortion Enabled", Range(0,1)) = 0

        _DistortionStrength ("Distortion Strength", Range(0,0.2)) = 0.02

        _DistortionSpeed ("Distortion Speed", Range(-10,10)) = 1


        // ============================================================
        // RENDER
        // ============================================================

        _Alpha ("Alpha", Range(0,1)) = 1
    }


    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }


        Pass
        {
            Name "SparkAdvancedGlow"

            Tags
            {
                "LightMode" = "UniversalForward"
            }


            Blend SrcAlpha OneMinusSrcAlpha

            ZWrite Off

            Cull Back


            HLSLPROGRAM

            #pragma vertex vert

            #pragma fragment frag

            #pragma target 3.5


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            // ========================================================
            // TEXTURES
            // ========================================================

            TEXTURE2D(_BaseMap);

            SAMPLER(sampler_BaseMap);


            TEXTURE2D(_NoiseMap);

            SAMPLER(sampler_NoiseMap);


            TEXTURE2D(_DissolveMap);

            SAMPLER(sampler_DissolveMap);


            // ========================================================
            // PROPERTIES
            // ========================================================

            CBUFFER_START(UnityPerMaterial)

                float4 _BaseMap_ST;

                float4 _BaseColor;

                float4 _GlowColor;

                float _GlowIntensity;


                float _FresnelEnabled;

                float4 _FresnelColor;

                float _FresnelPower;

                float _FresnelIntensity;


                float _Pulse;

                float _PulseIntensity;


                float _ScanEnabled;

                float4 _ScanColor;

                float _ScanSpeed;

                float _ScanScale;

                float _ScanWidth;

                float _ScanIntensity;


                float _SweepEnabled;

                float4 _SweepColor;

                float _SweepPosition;

                float _SweepWidth;

                float _SweepIntensity;

                float _SweepSpeed;


                float _NoiseEnabled;

                float _NoiseScale;

                float _NoiseSpeed;

                float _NoiseStrength;

                float4 _NoiseColor;

                float _NoiseIntensity;


                float _DissolveEnabled;

                float _DissolveAmount;

                float _DissolveEdgeWidth;

                float4 _DissolveEdgeColor;

                float _DissolveEdgeIntensity;


                float _DistortionEnabled;

                float _DistortionStrength;

                float _DistortionSpeed;


                float _Alpha;

            CBUFFER_END


            // ========================================================
            // VERTEX
            // ========================================================

            struct Attributes
            {
                float4 positionOS : POSITION;

                float3 normalOS : NORMAL;

                float2 uv : TEXCOORD0;
            };


            struct Varyings
            {
                float4 positionHCS : SV_POSITION;

                float2 uv : TEXCOORD0;

                float3 positionWS : TEXCOORD1;

                float3 normalWS : TEXCOORD2;
            };


            // ========================================================
            // VERTEX
            // ========================================================

            Varyings vert(
                Attributes input)
            {
                Varyings output;


                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(
                        input.positionOS.xyz
                    );


                VertexNormalInputs normalInputs =
                    GetVertexNormalInputs(
                        input.normalOS
                    );


                output.positionHCS =
                    positionInputs.positionCS;


                output.positionWS =
                    positionInputs.positionWS;


                output.normalWS =
                    NormalizeNormalPerVertex(
                        normalInputs.normalWS
                    );


                output.uv =
                    TRANSFORM_TEX(
                        input.uv,
                        _BaseMap
                    );


                // ----------------------------------------------------
                // VERTEX DISTORTION
                // ----------------------------------------------------

                if (
                    _DistortionEnabled >
                    0.5
                )
                {
                    float2 distortionUV =
                        input.uv *
                        3.0;


                    distortionUV +=
                        _Time.y *
                        _DistortionSpeed;


                    float noise =
                        SAMPLE_TEXTURE2D_LOD(
                            _NoiseMap,
                            sampler_NoiseMap,
                            distortionUV,
                            0
                        ).r;


                    float3 offset =
                        input.normalOS *
                        (
                            noise -
                            0.5
                        ) *
                        _DistortionStrength;


                    float3 distortedPosition =
                        input.positionOS.xyz +
                        offset;


                    positionInputs =
                        GetVertexPositionInputs(
                            distortedPosition
                        );


                    output.positionHCS =
                        positionInputs.positionCS;


                    output.positionWS =
                        positionInputs.positionWS;
                }


                return output;
            }


            // ========================================================
            // FRAGMENT
            // ========================================================

            half4 frag(
                Varyings input)
                : SV_Target
            {
                // ----------------------------------------------------
                // BASE
                // ----------------------------------------------------

                float4 baseSample =
                    SAMPLE_TEXTURE2D(
                        _BaseMap,
                        sampler_BaseMap,
                        input.uv
                    );


                float3 baseColor =
                    baseSample.rgb *
                    _BaseColor.rgb;


                float alpha =
                    baseSample.a *
                    _BaseColor.a *
                    _Alpha;


                // ----------------------------------------------------
                // VIEW DIRECTION
                // ----------------------------------------------------

                float3 viewDirection =
                    normalize(
                        GetWorldSpaceViewDir(
                            input.positionWS
                        )
                    );


                float3 normal =
                    normalize(
                        input.normalWS
                    );


                // ----------------------------------------------------
                // FRESNEL
                // ----------------------------------------------------

                float fresnel =
                    1.0 -
                    saturate(
                        dot(
                            normal,
                            viewDirection
                        )
                    );


                fresnel =
                    pow(
                        fresnel,
                        _FresnelPower
                    );


                float3 fresnelGlow =
                    _FresnelColor.rgb *
                    fresnel *
                    _FresnelIntensity *
                    _FresnelEnabled;


                // ----------------------------------------------------
                // PULSE
                // ----------------------------------------------------

                float3 pulseGlow =
                    _GlowColor.rgb *
                    _Pulse *
                    _PulseIntensity;


                // ----------------------------------------------------
                // SCAN
                // ----------------------------------------------------

                float scanPosition =
                    frac(
                        input.uv.y *
                        _ScanScale +
                        _Time.y *
                        _ScanSpeed
                    );


                float scan =
                    1.0 -
                    smoothstep(
                        0.0,
                        _ScanWidth,
                        abs(
                            scanPosition -
                            0.5
                        )
                    );


                float3 scanGlow =
                    _ScanColor.rgb *
                    scan *
                    _ScanIntensity *
                    _ScanEnabled;


                // ----------------------------------------------------
                // ENERGY SWEEP
                // ----------------------------------------------------

                float sweepPosition =
                    frac(
                        input.uv.x *
                        0.5 +
                        _Time.y *
                        _SweepSpeed
                    );


                float sweepDistance =
                    abs(
                        sweepPosition -
                        0.5 -
                        _SweepPosition
                    );


                float sweep =
                    1.0 -
                    smoothstep(
                        0.0,
                        _SweepWidth,
                        sweepDistance
                    );


                float3 sweepGlow =
                    _SweepColor.rgb *
                    sweep *
                    _SweepIntensity *
                    _SweepEnabled;


                // ----------------------------------------------------
                // NOISE
                // ----------------------------------------------------

                float2 noiseUV =
                    input.uv *
                    _NoiseScale;


                noiseUV +=
                    _Time.y *
                    _NoiseSpeed;


                float noise =
                    SAMPLE_TEXTURE2D(
                        _NoiseMap,
                        sampler_NoiseMap,
                        noiseUV
                    ).r;


                float noiseValue =
                    (
                        noise -
                        0.5
                    ) *
                    _NoiseStrength;


                float3 noiseGlow =
                    _NoiseColor.rgb *
                    noise *
                    _NoiseIntensity *
                    _NoiseEnabled;


                // ----------------------------------------------------
                // DISSOLVE
                // ----------------------------------------------------

                float dissolve =
                    SAMPLE_TEXTURE2D(
                        _DissolveMap,
                        sampler_DissolveMap,
                        input.uv
                    ).r;


                float dissolveEdge =
                    1.0 -
                    smoothstep(
                        _DissolveAmount,
                        _DissolveAmount +
                        _DissolveEdgeWidth,
                        dissolve
                    );


                float3 dissolveGlow =
                    _DissolveEdgeColor.rgb *
                    dissolveEdge *
                    _DissolveEdgeIntensity *
                    _DissolveEnabled;


                if (
                    _DissolveEnabled >
                    0.5
                )
                {
                    clip(
                        dissolve -
                        _DissolveAmount
                    );
                }


                // ----------------------------------------------------
                // FINAL GLOW
                // ----------------------------------------------------

                float3 finalColor =
                    baseColor;


                finalColor +=
                    _GlowColor.rgb *
                    _GlowIntensity;


                finalColor +=
                    fresnelGlow;


                finalColor +=
                    pulseGlow;


                finalColor +=
                    scanGlow;


                finalColor +=
                    sweepGlow;


                finalColor +=
                    noiseGlow;


                finalColor +=
                    dissolveGlow;


                // ----------------------------------------------------
                // NOISE COLOR DISTORTION
                // ----------------------------------------------------

                finalColor *=
                    1.0 +
                    noiseValue;


                return half4(
                    finalColor,
                    alpha
                );
            }

            ENDHLSL
        }
    }


    FallBack Off
}