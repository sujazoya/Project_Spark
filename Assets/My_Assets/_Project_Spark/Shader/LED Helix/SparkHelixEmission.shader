Shader "ProjectSpark/HelixEmission"
{
    Properties
    {
        _BaseColor ("Unpowered Filament Color", Color) = (0.08, 0.035, 0.012, 1)
        [HDR] _EmissionColor ("Emission Color (controlled by SparkLEDHelixGlow)", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "HelixForward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _EmissionColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Unlit, emissive filament; output HDR values above 1 for Bloom.
                half3 color = _BaseColor.rgb + _EmissionColor.rgb;
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
