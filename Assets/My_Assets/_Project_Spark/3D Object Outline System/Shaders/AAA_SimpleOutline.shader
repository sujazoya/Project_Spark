Shader "AAA/Simple Outline"
{
    Properties
    {
        [HDR]
        _OutlineColor
        (
            "Outline Color",
            Color
        ) = (0, 1, 1, 1)

        _OutlineWidth
        (
            "Outline Width",
            Range(0, 0.2)
        ) = 0.02

        _OutlineIntensity
        (
            "Outline Intensity",
            Range(0, 10)
        ) = 1

        _FresnelPower
        (
            "Fresnel Power",
            Range(0.01, 10)
        ) = 2

        _FresnelStrength
        (
            "Fresnel Strength",
            Range(0, 5)
        ) = 1

        _PulseSpeed
        (
            "Pulse Speed",
            Range(0, 20)
        ) = 2

        _PulseMin
        (
            "Pulse Min",
            Range(0, 5)
        ) = 0.75

        _PulseMax
        (
            "Pulse Max",
            Range(0, 5)
        ) = 1.25

        _ThroughWallAlpha
        (
            "Through Wall Alpha",
            Range(0, 1)
        ) = 0.35

        [HideInInspector]
        _ZTest
        (
            "Z Test",
            Float
        ) = 4
    }


    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }


        Pass
        {
            Name "Outline"

            Cull Front

            ZWrite Off

            ZTest [_ZTest]

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)

                float4 _OutlineColor;

                float _OutlineWidth;

                float _OutlineIntensity;

                float _FresnelPower;

                float _FresnelStrength;

                float _PulseSpeed;

                float _PulseMin;

                float _PulseMax;

                float _ThroughWallAlpha;

            CBUFFER_END


            struct Attributes
            {
                float3 positionOS : POSITION;

                float3 normalOS : NORMAL;
            };


            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                float3 normalWS : TEXCOORD0;

                float3 viewDirectionWS : TEXCOORD1;
            };


            Varyings Vert(
                Attributes input)
            {
                Varyings output;

                float3 positionWS =
                    TransformObjectToWorld(
                        input.positionOS);

                float3 normalWS =
                    TransformObjectToWorldNormal(
                        input.normalOS);

                positionWS +=
                    normalWS *
                    _OutlineWidth;

                output.positionCS =
                    TransformWorldToHClip(
                        positionWS);

                output.normalWS =
                    normalWS;

                output.viewDirectionWS =
                    GetWorldSpaceViewDir(
                        positionWS);

                return output;
            }


            half4 Frag(
                Varyings input)
                : SV_Target
            {
                float3 normalWS =
                    normalize(
                        input.normalWS);

                float3 viewDirectionWS =
                    normalize(
                        input.viewDirectionWS);

                float fresnel =
                    1.0 -
                    saturate(
                        dot(
                            normalWS,
                            viewDirectionWS));

                fresnel =
                    pow(
                        fresnel,
                        _FresnelPower);

                float fresnelFactor =
                    lerp(
                        1.0,
                        1.0 +
                        fresnel *
                        _FresnelStrength,
                        step(
                            0.001,
                            _FresnelStrength));

                float intensity =
                    _OutlineIntensity *
                    fresnelFactor;

                float3 finalColor =
                    _OutlineColor.rgb *
                    intensity;

                float finalAlpha =
                    saturate(
                        _OutlineColor.a *
                        intensity *
                        _ThroughWallAlpha);

                return half4(
                    finalColor,
                    finalAlpha);
            }

            ENDHLSL
        }
    }


    FallBack Off
}