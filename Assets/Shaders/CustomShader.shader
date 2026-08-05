Shader "ProjectSpark/ElectronicComponent"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.5,0.5,0.5,1)
        _Metallic ("Metallic", Range(0,1)) = 0.5
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _EmissionIntensity ("Emission Intensity", Float) = 1.0
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _RoughnessMap ("Roughness Map", 2D) = "white" {}
        _AO ("Ambient Occlusion", Float) = 1.0
        _Clearcoat ("Clearcoat", Float) = 0.0
        _ClearcoatSmoothness ("Clearcoat Smoothness", Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

        // Declare properties
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float  _Metallic;
            float  _Smoothness;
            float4 _EmissionColor;
            float  _EmissionIntensity;
            float  _AO;
        CBUFFER_END

        TEXTURE2D(_NormalMap);     SAMPLER(sampler_NormalMap);
        TEXTURE2D(_RoughnessMap);  SAMPLER(sampler_RoughnessMap);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS   : NORMAL;
            float4 tangentOS  : TANGENT;
            float2 uv         : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv         : TEXCOORD0;
            float3 positionWS : TEXCOORD1;
            float3 normalWS   : TEXCOORD2;
            float4 tangentWS  : TEXCOORD3;  // w holds sign
            float3 viewDirWS  : TEXCOORD4;
            float4 shadowCoord: TEXCOORD5;
        };
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(output.positionWS);
                output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                // Sample textures
                half3 normalMap = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
                half3 roughnessMap = SAMPLE_TEXTURE2D(_RoughnessMap, sampler_RoughnessMap, input.uv).r;

                // Build tangent-space to world-space matrix
                float3 normalWS = normalize(input.normalWS);
                float3 tangentWS = normalize(input.tangentWS.xyz);
                float3 bitangentWS = normalize(cross(normalWS, tangentWS) * input.tangentWS.w);
                float3x3 TBN = float3x3(tangentWS, bitangentWS, normalWS);
                normalWS = normalize(mul(normalMap, TBN));

                // Surface data
                float3 albedo = _BaseColor.rgb;
                float metallic = _Metallic;
                float smoothness = _Smoothness * (1.0 - roughnessMap); // combine with roughness map
                float ao = _AO;
                float3 emission = _EmissionColor.rgb * _EmissionIntensity;

                // Prepare BRDF data
                InputData inputData;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = input.viewDirWS;
                inputData.shadowCoord = input.shadowCoord;
                inputData.fogCoord = 0;
                inputData.vertexLighting = 0;
                inputData.bakedGI = 0;
                inputData.normalizedScreenSpaceUV = 0;
                inputData.shadowMask = 0;

                SurfaceData surfaceData;
                surfaceData.albedo = albedo;
                surfaceData.alpha = 1.0;
                surfaceData.metallic = metallic;
                surfaceData.smoothness = smoothness;
                surfaceData.normalTS = normalMap;
                surfaceData.emission = emission;
                surfaceData.occlusion = ao;
                surfaceData.specular = 0;
                surfaceData.clearCoatMask = 0;
                surfaceData.clearCoatSmoothness = 0;

                // Apply lighting
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                return color;
            }
            ENDHLSL
        }
    }
}