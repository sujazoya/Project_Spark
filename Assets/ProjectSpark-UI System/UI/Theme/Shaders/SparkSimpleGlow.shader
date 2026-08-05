Shader "ProjectSpark/Simple Glow"
{
    Properties
    {
        [MainTexture]
        _BaseMap ("Base Map", 2D) = "white" {}

        [MainColor]
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        _GlowColor ("Glow Color", Color) = (0,0.8,1,1)

        _GlowIntensity ("Glow Intensity", Range(0,10)) = 0

        _Pulse ("Pulse", Range(0,1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "SimpleGlow"

            Tags
            {
                "LightMode"="UniversalForward"
            }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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
                float3 normalWS : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)

                float4 _BaseColor;
                float4 _GlowColor;

                float _GlowIntensity;
                float _Pulse;

            CBUFFER_END


            Varyings vert(
                Attributes input
            )
            {
                Varyings output;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(
                        input.positionOS.xyz
                    );

                output.positionHCS =
                    positionInputs.positionCS;

                output.uv =
                    input.uv;

                output.normalWS =
                    TransformObjectToWorldNormal(
                        input.normalOS
                    );

                return output;
            }


            half4 frag(
                Varyings input
            ) : SV_Target
            {
                half4 baseTexture =
                    SAMPLE_TEXTURE2D(
                        _BaseMap,
                        sampler_BaseMap,
                        input.uv
                    );

                half3 baseColor =
                    baseTexture.rgb *
                    _BaseColor.rgb;


                half glow =
                    _GlowIntensity *
                    _Pulse;


                half3 finalColor =
                    baseColor +
                    (
                        _GlowColor.rgb *
                        glow
                    );


                return half4(
                    finalColor,
                    _BaseColor.a
                );
            }

            ENDHLSL
        }
    }
}