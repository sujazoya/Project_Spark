
Shader "ProjectSpark/VFX/SparkAdvancedGlow"
{
    Properties
    {
        // ============================================================
        // BASE
        // ============================================================

        [Header(Base)]

        [MainTexture]
        _BaseMap ("Base Map", 2D) = "white" {}

        [MainColor]
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        _Alpha ("Alpha", Range(0,1)) = 1


        // ============================================================
        // GLOW
        // ============================================================

        [Header(Glow)]

        _GlowColor ("Glow Color", Color) = (0,1,1,1)

        _GlowIntensity ("Glow Intensity", Range(0,20)) = 4


        // ============================================================
        // PULSE
        // ============================================================

        [Header(Pulse)]

        _Pulse ("Pulse", Range(0,1)) = 0

        _PulseIntensity ("Pulse Intensity", Range(0,20)) = 3


        // ============================================================
        // FLASH
        // ============================================================

        [Header(Flash)]

        _Flash ("Flash", Range(0,1)) = 0

        _FlashColor ("Flash Color", Color) = (1,1,1,1)

        _FlashIntensity ("Flash Intensity", Range(0,20)) = 5


        // ============================================================
        // SCAN
        // ============================================================

        [Header(Scan)]

        _ScanEnabled ("Scan Enabled", Float) = 1

        _ScanColor ("Scan Color", Color) = (0,1,1,1)

        _ScanSpeed ("Scan Speed", Range(-10,10)) = 2

        _ScanScale ("Scan Scale", Range(0.1,50)) = 8

        _ScanWidth ("Scan Width", Range(0.001,1)) = 0.1

        _ScanIntensity ("Scan Intensity", Range(0,20)) = 4


        // ============================================================
        // SWEEP
        // ============================================================

        [Header(Sweep)]

        _SweepEnabled ("Sweep Enabled", Float) = 1

        _SweepColor ("Sweep Color", Color) = (0,1,1,1)

        _SweepPosition ("Sweep Position", Range(0,1)) = 0

        _SweepWidth ("Sweep Width", Range(0.001,1)) = 0.1

        _SweepIntensity ("Sweep Intensity", Range(0,20)) = 5

        _SweepSpeed ("Sweep Speed", Range(-10,10)) = 1


        // ============================================================
        // NOISE
        // ============================================================

        [Header(Noise)]

        _NoiseEnabled ("Noise Enabled", Float) = 0

        _NoiseMap ("Noise Map", 2D) = "gray" {}

        _NoiseColor ("Noise Color", Color) = (0,1,1,1)

        _NoiseScale ("Noise Scale", Range(0.1,50)) = 5

        _NoiseSpeed ("Noise Speed", Vector) = (1,1,0,0)

        _NoiseStrength ("Noise Strength", Range(0,1)) = 0.1

        _NoiseIntensity ("Noise Intensity", Range(0,20)) = 2


        // ============================================================
        // DISSOLVE
        // ============================================================

        [Header(Dissolve)]

        _DissolveEnabled ("Dissolve Enabled", Float) = 0

        _DissolveMap ("Dissolve Map", 2D) = "gray" {}

        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0

        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0.001,1)) = 0.05

        _DissolveEdgeColor ("Dissolve Edge Color", Color) = (0,1,1,1)

        _DissolveEdgeIntensity ("Dissolve Edge Intensity", Range(0,20)) = 5


        // ============================================================
        // FRESNEL
        // ============================================================

        [Header(Fresnel)]

        _FresnelColor ("Fresnel Color", Color) = (0,1,1,1)

        _FresnelPower ("Fresnel Power", Range(0.1,10)) = 3

        _FresnelIntensity ("Fresnel Intensity", Range(0,20)) = 2


        // ============================================================
        // DISTORTION
        // ============================================================

        [Header(Distortion)]

        _DistortionEnabled ("Distortion Enabled", Float) = 0

        _DistortionMap ("Distortion Map", 2D) = "gray" {}

        _DistortionSpeed ("Distortion Speed", Vector) = (1,1,0,0)

        _DistortionStrength ("Distortion Strength", Range(0,1)) = 0.02


        // ============================================================
        // RENDERING
        // ============================================================

        [Header(Rendering)]

        [Enum(UnityEngine.Rendering.BlendMode)]
        _SrcBlend ("Source Blend", Float) = 1

        [Enum(UnityEngine.Rendering.BlendMode)]
        _DstBlend ("Destination Blend", Float) = 10

        [Enum(UnityEngine.Rendering.CullMode)]
        _Cull ("Cull", Float) = 2

        [Enum(Off,0,On,1)]
        _ZWrite ("Z Write", Float) = 0
    }


    // ================================================================
    // SUBSHADER
    // ================================================================

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "UniversalMaterialType" = "Unlit"
            "IgnoreProjector" = "True"
        }


        // ============================================================
        // MAIN PASS
        // ============================================================

        Pass
        {
            Name "SparkAdvancedGlow"

            Tags
            {
                "LightMode" = "UniversalForward"
            }


            Blend [_SrcBlend] [_DstBlend]

            ZWrite [_ZWrite]

            Cull [_Cull]

            ZTest LEqual


            HLSLPROGRAM

            #pragma target 3.5

            #pragma vertex Vert

            #pragma fragment Frag


            // ========================================================
            // URP
            // ========================================================

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


            TEXTURE2D(_DistortionMap);

            SAMPLER(sampler_DistortionMap);


            // ========================================================
            // MATERIAL PROPERTIES
            //
            // IMPORTANT:
            // All properties written by MaterialPropertyBlock are
            // included in UnityPerMaterial.
            // ========================================================

            CBUFFER_START(UnityPerMaterial)

                // Base

                float4 _BaseColor;

                float _Alpha;


                // Glow

                float4 _GlowColor;

                float _GlowIntensity;


                // Pulse

                float _Pulse;

                float _PulseIntensity;


                // Flash

                float _Flash;

                float4 _FlashColor;

                float _FlashIntensity;


                // Scan

                float _ScanEnabled;

                float4 _ScanColor;

                float _ScanSpeed;

                float _ScanScale;

                float _ScanWidth;

                float _ScanIntensity;


                // Sweep

                float _SweepEnabled;

                float4 _SweepColor;

                float _SweepPosition;

                float _SweepWidth;

                float _SweepIntensity;

                float _SweepSpeed;


                // Noise

                float _NoiseEnabled;

                float4 _NoiseColor;

                float _NoiseScale;

                float4 _NoiseSpeed;

                float _NoiseStrength;

                float _NoiseIntensity;


                // Dissolve

                float _DissolveEnabled;

                float _DissolveAmount;

                float _DissolveEdgeWidth;

                float4 _DissolveEdgeColor;

                float _DissolveEdgeIntensity;


                // Fresnel

                float4 _FresnelColor;

                float _FresnelPower;

                float _FresnelIntensity;


                // Distortion

                float _DistortionEnabled;

                float4 _DistortionSpeed;

                float _DistortionStrength;


                // Rendering

                float4 _BaseMap_ST;

                float4 _NoiseMap_ST;

                float4 _DissolveMap_ST;

                float4 _DistortionMap_ST;


            CBUFFER_END


            // ========================================================
            // VERTEX INPUT
            // ========================================================

            struct Attributes
            {
                float4 positionOS : POSITION;

                float3 normalOS : NORMAL;

                float2 uv : TEXCOORD0;
            };


            // ========================================================
            // VERTEX OUTPUT
            // ========================================================

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;

                float2 uv : TEXCOORD0;

                float3 positionWS : TEXCOORD1;

                float3 normalWS : TEXCOORD2;

                float3 viewDirWS : TEXCOORD3;
            };


            // ========================================================
            // VERTEX
            // ========================================================

            Varyings Vert(
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


                output.viewDirWS =
                    GetWorldSpaceViewDir(
                        positionInputs.positionWS
                    );


                output.uv =
                    TRANSFORM_TEX(
                        input.uv,
                        _BaseMap
                    );


                return output;
            }


            // ============================================================
            // SAFE SMOOTH MASK
            // ============================================================

            float SparkSoftBand(
                float value,
                float center,
                float width)
            {
                float safeWidth =
                    max(
                        width,
                        0.0001
                    );


                float distanceFromCenter =
                    abs(
                        value -
                        center
                    );


                return
                    1.0 -
                    smoothstep(
                        0.0,
                        safeWidth,
                        distanceFromCenter
                    );
            }


            // ============================================================
            // FRAGMENT
            // ============================================================

            half4 Frag(
                Varyings input)
                : SV_Target
            {
                // ====================================================
                // NORMALIZE VIEW DATA
                // ====================================================

                float3 normalWS =
                    normalize(
                        input.normalWS
                    );


                float3 viewDirWS =
                    normalize(
                        input.viewDirWS
                    );


                // ====================================================
                // DISTORTION
                // ====================================================

                float2 distortionUV =
                    input.uv;


                if (
                    _DistortionEnabled >
                    0.5
                )
                {
                    float2 distortionOffset =
                        _Time.y *
                        _DistortionSpeed.xy;


                    float2 distortionSampleUV =
                        input.uv *
                        _DistortionMap_ST.xy +
                        _DistortionMap_ST.zw +
                        distortionOffset;


                    float2 distortionSample =
                        SAMPLE_TEXTURE2D(
                            _DistortionMap,
                            sampler_DistortionMap,
                            distortionSampleUV
                        ).rg;


                    distortionSample =
                        distortionSample *
                        2.0 -
                        1.0;


                    distortionUV +=
                        distortionSample *
                        _DistortionStrength;
                }


                // ====================================================
                // BASE TEXTURE
                // ====================================================

                float4 baseSample =
                    SAMPLE_TEXTURE2D(
                        _BaseMap,
                        sampler_BaseMap,
                        distortionUV
                    );


                float3 baseColor =
                    baseSample.rgb *
                    _BaseColor.rgb;


                float alpha =
                    baseSample.a *
                    _BaseColor.a *
                    _Alpha;


                // ====================================================
                // DISSOLVE
                // ====================================================

                float dissolveValue =
                    SAMPLE_TEXTURE2D(
                        _DissolveMap,
                        sampler_DissolveMap,
                        input.uv
                    ).r;


                float dissolveEdge =
                    0.0;


                if (
                    _DissolveEnabled >
                    0.5
                )
                {
                    // The dissolve threshold is controlled by
                    // _DissolveAmount.
                    //
                    // Pixels below the threshold disappear.
                    //
                    // The edge mask is centered around the threshold
                    // rather than covering the entire visible area.

                    clip(
                        dissolveValue -
                        _DissolveAmount
                    );


                    float edgeDistance =
                        abs(
                            dissolveValue -
                            _DissolveAmount
                        );


                    dissolveEdge =
                        1.0 -
                        smoothstep(
                            0.0,
                            max(
                                _DissolveEdgeWidth,
                                0.0001
                            ),
                            edgeDistance
                        );
                }


                // ====================================================
                // FRESNEL
                // ====================================================

                float fresnel =
                    1.0 -
                    saturate(
                        dot(
                            normalWS,
                            viewDirWS
                        )
                    );


                fresnel =
                    pow(
                        fresnel,
                        max(
                            _FresnelPower,
                            0.0001
                        )
                    );


                float3 fresnelGlow =
                    _FresnelColor.rgb *
                    fresnel *
                    _FresnelIntensity;


                // ====================================================
                // PULSE
                // ====================================================

                float3 pulseGlow =
                    _GlowColor.rgb *
                    _Pulse *
                    _PulseIntensity;


                // ====================================================
                // FLASH
                // ====================================================

                float3 flashGlow =
                    _FlashColor.rgb *
                    _Flash *
                    _FlashIntensity;


                // ====================================================
                // SCAN
                // ====================================================

                float scanMask =
                    0.0;


                if (
                    _ScanEnabled >
                    0.5
                )
                {
                    float scanPosition =
                        frac(
                            input.uv.y *
                            _ScanScale +
                            _Time.y *
                            _ScanSpeed
                        );


                    scanMask =
                        SparkSoftBand(
                            scanPosition,
                            0.5,
                            _ScanWidth
                        );
                }


                float3 scanGlow =
                    _ScanColor.rgb *
                    scanMask *
                    _ScanIntensity *
                    _ScanEnabled;


                // ====================================================
                // SWEEP
                // ====================================================

                float sweepMask =
                    0.0;


                if (
                    _SweepEnabled >
                    0.5
                )
                {
                    float sweepPosition =
                        frac(
                            _SweepPosition +
                            _Time.y *
                            _SweepSpeed
                        );


                    sweepMask =
                        SparkSoftBand(
                            input.uv.x,
                            sweepPosition,
                            _SweepWidth
                        );
                }


                float3 sweepGlow =
                    _SweepColor.rgb *
                    sweepMask *
                    _SweepIntensity *
                    _SweepEnabled;


                // ====================================================
                // NOISE
                // ====================================================

                float noiseValue =
                    0.0;


                if (
                    _NoiseEnabled >
                    0.5
                )
                {
                    float2 noiseUV =
                        input.uv *
                        _NoiseScale *
                        _NoiseMap_ST.xy;


                    noiseUV +=
                        _NoiseMap_ST.zw;


                    noiseUV +=
                        _Time.y *
                        _NoiseSpeed.xy;


                    float noiseSample =
                        SAMPLE_TEXTURE2D(
                            _NoiseMap,
                            sampler_NoiseMap,
                            noiseUV
                        ).r;


                    noiseValue =
                        (
                            noiseSample -
                            0.5
                        ) *
                        2.0;


                    noiseValue *=
                        _NoiseStrength;
                }


                float3 noiseGlow =
                    _NoiseColor.rgb *
                    noiseValue *
                    _NoiseIntensity *
                    _NoiseEnabled;


                // ====================================================
                // DISSOLVE EDGE
                // ====================================================

                float3 dissolveGlow =
                    _DissolveEdgeColor.rgb *
                    dissolveEdge *
                    _DissolveEdgeIntensity *
                    _DissolveEnabled;


                // ====================================================
                // BASE GLOW
                // ====================================================

                float3 glow =
                    _GlowColor.rgb *
                    _GlowIntensity;


                // ====================================================
                // FINAL COLOR
                // ====================================================

                float3 finalColor =
                    baseColor;


                finalColor +=
                    glow;


                finalColor +=
                    fresnelGlow;


                finalColor +=
                    pulseGlow;


                finalColor +=
                    flashGlow;


                finalColor +=
                    scanGlow;


                finalColor +=
                    sweepGlow;


                finalColor +=
                    noiseGlow;


                finalColor +=
                    dissolveGlow;


                // ====================================================
                // NOISE MODULATION
                // ====================================================

                float noiseModulation =
                    1.0 +
                    noiseValue;


                finalColor *=
                    max(
                        noiseModulation,
                        0.0
                    );


                // ====================================================
                // OUTPUT
                // ====================================================

                return half4(
                    finalColor,
                    alpha
                );
            }

            ENDHLSL
        }
    }


    // ================================================================
    // FALLBACK
    // ================================================================

    FallBack Off
}

