Shader "ProjectSpark/FX/Outline Emission"
{
    Properties
    {
        // ============================================================
        // SURFACE
        // ============================================================

        [MainTexture]
        _BaseMap ("Base Map", 2D) = "white" {}

        [MainColor]
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5

        [Toggle(_ALPHATEST_ON)]
        _AlphaClip ("Alpha Clipping", Float) = 0

        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5


        // ============================================================
        // SURFACE EMISSION
        // ============================================================

        [HDR]
        _EmissionColor ("Emission Color", Color) = (0,0,0,1)

        _EmissionStrength ("Emission Strength", Range(0,20)) = 0

        [Toggle]
        _EmissionPulse ("Emission Pulse", Float) = 0

        _EmissionPulseSpeed ("Emission Pulse Speed", Range(0,10)) = 2

        _EmissionPulseAmount ("Emission Pulse Amount", Range(0,1)) = 0.25

        _EmissionFresnel ("Emission Fresnel", Range(0,5)) = 0


        // ============================================================
        // OUTLINE
        // ============================================================

        [HDR]
        _OutlineColor ("Outline Color", Color) = (0,1,1,1)

        _OutlineWidth ("Outline Width", Range(0,0.1)) = 0.015

        _OutlineMinWidth ("Minimum Width", Range(0,0.1)) = 0.002

        _OutlineMaxWidth ("Maximum Width", Range(0,0.2)) = 0.05

        [Toggle]
        _OutlineEnabled ("Outline Enabled", Float) = 1

        [Toggle]
        _OutlinePulse ("Outline Pulse", Float) = 0

        _OutlinePulseSpeed ("Outline Pulse Speed", Range(0,10)) = 2

        _OutlinePulseAmount ("Outline Pulse Amount", Range(0,1)) = 0.2

        _OutlineEmission ("Outline Emission", Range(0,30)) = 5


        // ============================================================
        // OUTLINE FRESNEL
        // ============================================================

        [Toggle]
        _OutlineFresnel ("Outline Fresnel", Float) = 1

        _FresnelPower ("Fresnel Power", Range(0.1,10)) = 3

        _FresnelStrength ("Fresnel Strength", Range(0,5)) = 1


        // ============================================================
        // DISTANCE FADE
        // ============================================================

        [Toggle]
        _DistanceFade ("Distance Fade", Float) = 1

        _FadeStart ("Fade Start", Float) = 5

        _FadeEnd ("Fade End", Float) = 50


        // ============================================================
        // RENDERING
        // ============================================================

        [Enum(UnityEngine.Rendering.CullMode)]
        _Cull ("Surface Cull", Float) = 2

        [Enum(UnityEngine.Rendering.BlendMode)]
        _SrcBlend ("Source Blend", Float) = 1

        [Enum(UnityEngine.Rendering.BlendMode)]
        _DstBlend ("Destination Blend", Float) = 0

        [Enum(UnityEngine.Rendering.CompareFunction)]
        _ZTest ("Depth Test", Float) = 4

        [Toggle]
        _ZWrite ("Z Write", Float) = 1
    }


    // ================================================================
    // SUBSHADER
    // ================================================================

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "UniversalMaterialType"="Lit"
        }


        // ============================================================
        // FORWARD LIT PASS
        // ============================================================

        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode"="UniversalForward"
            }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Cull [_Cull]

            HLSLPROGRAM

            #pragma target 3.0

            #pragma vertex Vert
            #pragma fragment Frag

            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #pragma multi_compile_instancing

            #pragma multi_compile_fog

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN

            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


            // ========================================================
            // MATERIAL BUFFER
            // ========================================================

            CBUFFER_START(UnityPerMaterial)

                float4 _BaseColor;

                float _Metallic;
                float _Smoothness;

                float _Cutoff;

                float4 _EmissionColor;
                float _EmissionStrength;

                float _EmissionPulse;
                float _EmissionPulseSpeed;
                float _EmissionPulseAmount;

                float _EmissionFresnel;

                float4 _OutlineColor;

                float _OutlineWidth;
                float _OutlineMinWidth;
                float _OutlineMaxWidth;

                float _OutlineEnabled;

                float _OutlinePulse;
                float _OutlinePulseSpeed;
                float _OutlinePulseAmount;

                float _OutlineEmission;

                float _OutlineFresnel;
                float _FresnelPower;
                float _FresnelStrength;

                float _DistanceFade;
                float _FadeStart;
                float _FadeEnd;

            CBUFFER_END


            // ========================================================
            // TEXTURE
            // ========================================================

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);


            // ========================================================
            // VERTEX DATA
            // ========================================================

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;

                float2 uv : TEXCOORD2;

                float3 viewDirWS : TEXCOORD3;

                float fogFactor : TEXCOORD4;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            // ========================================================
            // VERTEX
            // ========================================================

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(IN.positionOS.xyz);

                VertexNormalInputs normalInputs =
                    GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = positionInputs.positionCS;

                OUT.positionWS = positionInputs.positionWS;

                OUT.normalWS =
                    normalize(normalInputs.normalWS);

                OUT.uv = IN.uv;

                OUT.viewDirWS =
                    GetWorldSpaceNormalizeViewDir(
                        positionInputs.positionWS
                    );

                OUT.fogFactor =
                    ComputeFogFactor(positionInputs.positionCS.z);

                return OUT;
            }


            // ========================================================
            // FRESNEL
            // ========================================================

            float CalculateFresnel(
                float3 normalWS,
                float3 viewDirWS
            )
            {
                float NdotV =
                    saturate(dot(normalWS, viewDirWS));

                return pow(
                    1.0 - NdotV,
                    _FresnelPower
                );
            }


            // ========================================================
            // EMISSION PULSE
            // ========================================================

            float CalculateEmissionPulse()
            {
                float wave =
                    sin(_Time.y * _EmissionPulseSpeed);

                wave =
                    wave * 0.5 + 0.5;

                return lerp(
                    1.0,
                    lerp(
                        1.0 - _EmissionPulseAmount,
                        1.0 + _EmissionPulseAmount,
                        wave
                    ),
                    _EmissionPulse
                );
            }


            // ========================================================
            // SURFACE EMISSION
            // ========================================================

            float3 CalculateEmission(
                float3 normalWS,
                float3 viewDirWS
            )
            {
                float pulse =
                    CalculateEmissionPulse();

                float fresnel =
                    CalculateFresnel(
                        normalWS,
                        viewDirWS
                    );

                float fresnelContribution =
                    lerp(
                        1.0,
                        1.0 +
                        fresnel *
                        _EmissionFresnel,
                        saturate(_EmissionFresnel)
                    );

                return
                    _EmissionColor.rgb *
                    _EmissionStrength *
                    pulse *
                    fresnelContribution;
            }


            // ========================================================
            // MAIN LIGHTING
            // ========================================================

            float3 CalculateLighting(
                float3 positionWS,
                float3 normalWS,
                float3 viewDirWS,
                float3 albedo
            )
            {
                float3 color = 0;

                // ----------------------------------------------------
                // MAIN LIGHT
                // ----------------------------------------------------

                Light mainLight =
                    GetMainLight();

                float NdotL =
                    saturate(
                        dot(
                            normalWS,
                            mainLight.direction
                        )
                    );

                float3 diffuse =
                    albedo *
                    mainLight.color *
                    NdotL *
                    mainLight.distanceAttenuation *
                    mainLight.shadowAttenuation;

                color += diffuse;


                // ----------------------------------------------------
                // AMBIENT
                // ----------------------------------------------------

                float3 ambient =
                    SampleSH(normalWS);

                color +=
                    albedo *
                    ambient;


                // ----------------------------------------------------
                // ADDITIONAL LIGHTS
                // ----------------------------------------------------

                #ifdef _ADDITIONAL_LIGHTS

                uint lightCount =
                    GetAdditionalLightsCount();

                for (
                    uint i = 0;
                    i < lightCount;
                    i++
                )
                {
                    Light light =
                        GetAdditionalLight(
                            i,
                            positionWS
                        );

                    float NdotL2 =
                        saturate(
                            dot(
                                normalWS,
                                light.direction
                            )
                        );

                    color +=
                        albedo *
                        light.color *
                        NdotL2 *
                        light.distanceAttenuation *
                        light.shadowAttenuation;
                }

                #endif


                // ----------------------------------------------------
                // SIMPLE SPECULAR
                // ----------------------------------------------------

                float3 halfDir =
                    normalize(
                        mainLight.direction +
                        viewDirWS
                    );

                float NdotH =
                    saturate(
                        dot(
                            normalWS,
                            halfDir
                        )
                    );

                float specPower =
                    lerp(
                        8.0,
                        256.0,
                        _Smoothness
                    );

                float spec =
                    pow(
                        NdotH,
                        specPower
                    );

                float3 specular =
                    spec *
                    mainLight.color *
                    _Smoothness;

                color += specular;


                return color;
            }


            // ========================================================
            // FRAGMENT
            // ========================================================

            half4 Frag(Varyings IN)
                : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                // ----------------------------------------------------
                // SAMPLE BASE
                // ----------------------------------------------------

                float4 baseSample =
                    SAMPLE_TEXTURE2D(
                        _BaseMap,
                        sampler_BaseMap,
                        IN.uv
                    );

                float4 baseColor =
                    baseSample *
                    _BaseColor;


                // ----------------------------------------------------
                // ALPHA CLIPPING
                // ----------------------------------------------------

                #ifdef _ALPHATEST_ON

                clip(
                    baseColor.a -
                    _Cutoff
                );

                #endif


                // ----------------------------------------------------
                // NORMAL
                // ----------------------------------------------------

                float3 normalWS =
                    normalize(IN.normalWS);

                float3 viewDirWS =
                    normalize(IN.viewDirWS);


                // ----------------------------------------------------
                // LIGHTING
                // ----------------------------------------------------

                float3 lighting =
                    CalculateLighting(
                        IN.positionWS,
                        normalWS,
                        viewDirWS,
                        baseColor.rgb
                    );


                // ----------------------------------------------------
                // EMISSION
                // ----------------------------------------------------

                float3 emission =
                    CalculateEmission(
                        normalWS,
                        viewDirWS
                    );


                // ----------------------------------------------------
                // FINAL
                // ----------------------------------------------------

                float3 finalColor =
                    lighting +
                    emission;


                // ----------------------------------------------------
                // FOG
                // ----------------------------------------------------

                finalColor =
                    MixFog(
                        finalColor,
                        IN.fogFactor
                    );


                return half4(
                    finalColor,
                    baseColor.a
                );
            }

            ENDHLSL
        }


        // ============================================================
        // OUTLINE PASS
        // ============================================================

        Pass
        {
            Name "Outline"
            Tags
            {
                "LightMode"="SRPDefaultUnlit"
            }

            Cull Front

            ZWrite Off

            ZTest LEqual

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

            #pragma target 3.0

            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag

            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            // ========================================================
            // MATERIAL BUFFER
            // ========================================================

            CBUFFER_START(UnityPerMaterial)

                float4 _OutlineColor;

                float _OutlineWidth;
                float _OutlineMinWidth;
                float _OutlineMaxWidth;

                float _OutlineEnabled;

                float _OutlinePulse;
                float _OutlinePulseSpeed;
                float _OutlinePulseAmount;

                float _OutlineEmission;

                float _OutlineFresnel;
                float _FresnelPower;
                float _FresnelStrength;

                float _DistanceFade;
                float _FadeStart;
                float _FadeEnd;

            CBUFFER_END


            // ========================================================
            // VERTEX
            // ========================================================

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                float3 normalWS : TEXCOORD0;

                float3 viewDirWS : TEXCOORD1;

                float3 positionWS : TEXCOORD2;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            // ========================================================
            // OUTLINE WIDTH
            // ========================================================

            float CalculateOutlineWidth(
                float3 positionWS
            )
            {
                // ----------------------------------------------------
                // Camera distance
                // ----------------------------------------------------

                float cameraDistance =
                    distance(
                        positionWS,
                        GetCameraPositionWS()
                    );


                // ----------------------------------------------------
                // Distance compensation
                // ----------------------------------------------------

                float width =
                    _OutlineWidth *
                    max(
                        cameraDistance,
                        1.0
                    );


                // ----------------------------------------------------
                // Clamp
                // ----------------------------------------------------

                width =
                    clamp(
                        width,
                        _OutlineMinWidth,
                        _OutlineMaxWidth
                    );


                // ----------------------------------------------------
                // Pulse
                // ----------------------------------------------------

                float wave =
                    sin(
                        _Time.y *
                        _OutlinePulseSpeed
                    );

                wave =
                    wave * 0.5 + 0.5;

                float pulse =
                    lerp(
                        1.0,
                        lerp(
                            1.0 -
                            _OutlinePulseAmount,

                            1.0 +
                            _OutlinePulseAmount,

                            wave
                        ),

                        _OutlinePulse
                    );

                width *= pulse;


                return width;
            }


            // ========================================================
            // VERTEX
            // ========================================================

            Varyings OutlineVert(
                Attributes IN
            )
            {
                Varyings OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);


                // ----------------------------------------------------
                // WORLD POSITION
                // ----------------------------------------------------

                float3 positionWS =
                    TransformObjectToWorld(
                        IN.positionOS.xyz
                    );


                // ----------------------------------------------------
                // WORLD NORMAL
                // ----------------------------------------------------

                float3 normalWS =
                    TransformObjectToWorldNormal(
                        IN.normalOS
                    );

                normalWS =
                    normalize(normalWS);


                // ----------------------------------------------------
                // OUTLINE WIDTH
                // ----------------------------------------------------

                float width =
                    CalculateOutlineWidth(
                        positionWS
                    );


                // ----------------------------------------------------
                // EXPAND MESH
                // ----------------------------------------------------

                positionWS +=
                    normalWS *
                    width;


                // ----------------------------------------------------
                // OUTPUT
                // ----------------------------------------------------

                OUT.positionWS =
                    positionWS;

                OUT.normalWS =
                    normalWS;

                OUT.viewDirWS =
                    GetWorldSpaceNormalizeViewDir(
                        positionWS
                    );

                OUT.positionCS =
                    TransformWorldToHClip(
                        positionWS
                    );


                return OUT;
            }


            // ========================================================
            // DISTANCE FADE
            // ========================================================

            float CalculateDistanceFade(
                float3 positionWS
            )
            {
                if (_DistanceFade < 0.5)
                    return 1.0;

                float distanceToCamera =
                    distance(
                        positionWS,
                        GetCameraPositionWS()
                    );

                float fade =
                    1.0 -
                    smoothstep(
                        _FadeStart,
                        _FadeEnd,
                        distanceToCamera
                    );

                return fade;
            }


            // ========================================================
            // FRAGMENT
            // ========================================================

            half4 OutlineFrag(
                Varyings IN
            )
                : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);


                // ----------------------------------------------------
                // ENABLE
                // ----------------------------------------------------

                if (_OutlineEnabled < 0.5)
                    discard;


                // ----------------------------------------------------
                // FRESNEL
                // ----------------------------------------------------

                float3 normalWS =
                    normalize(
                        IN.normalWS
                    );

                float3 viewDirWS =
                    normalize(
                        IN.viewDirWS
                    );


                float fresnel =
                    saturate(
                        pow(
                            1.0 -
                            saturate(
                                dot(
                                    normalWS,
                                    viewDirWS
                                )
                            ),

                            _FresnelPower
                        )
                    );


                float fresnelFactor =
                    lerp(
                        1.0,

                        1.0 +
                        fresnel *
                        _FresnelStrength,

                        _OutlineFresnel
                    );


                // ----------------------------------------------------
                // PULSE
                // ----------------------------------------------------

                float wave =
                    sin(
                        _Time.y *
                        _OutlinePulseSpeed
                    );

                wave =
                    wave * 0.5 +
                    0.5;


                float pulse =
                    lerp(
                        1.0,

                        lerp(
                            1.0 -
                            _OutlinePulseAmount,

                            1.0 +
                            _OutlinePulseAmount,

                            wave
                        ),

                        _OutlinePulse
                    );


                // ----------------------------------------------------
                // DISTANCE FADE
                // ----------------------------------------------------

                float fade =
                    CalculateDistanceFade(
                        IN.positionWS
                    );


                // ----------------------------------------------------
                // FINAL EMISSION
                // ----------------------------------------------------

                float3 color =
                    _OutlineColor.rgb *
                    _OutlineEmission *
                    fresnelFactor *
                    pulse;


                color *= fade;


                // ----------------------------------------------------
                // ALPHA
                // ----------------------------------------------------

                float alpha =
                    _OutlineColor.a *
                    fade;


                return half4(
                    color,
                    alpha
                );
            }

            ENDHLSL
        }


        // ============================================================
        // SHADOW CASTER
        // ============================================================

        Pass
        {
            Name "ShadowCaster"

            Tags
            {
                "LightMode"="ShadowCaster"
            }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM

            #pragma target 3.0

            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"


            CBUFFER_START(UnityPerMaterial)

                float4 _BaseColor;
                float _Cutoff;

            CBUFFER_END


            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);


            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            Varyings ShadowVert(
                Attributes IN
            )
            {
                Varyings OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(
                        IN.positionOS.xyz
                    );

                OUT.positionCS =
                    positionInputs.positionCS;

                OUT.uv =
                    IN.uv;

                return OUT;
            }


            half4 ShadowFrag(
                Varyings IN
            )
                : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                #ifdef _ALPHATEST_ON

                float alpha =
                    SAMPLE_TEXTURE2D(
                        _BaseMap,
                        sampler_BaseMap,
                        IN.uv
                    ).a *
                    _BaseColor.a;

                clip(
                    alpha -
                    _Cutoff
                );

                #endif

                return 0;
            }

            ENDHLSL
        }


        // ============================================================
        // DEPTH ONLY
        // ============================================================

        Pass
        {
            Name "DepthOnly"

            Tags
            {
                "LightMode"="DepthOnly"
            }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM

            #pragma target 3.0

            #pragma vertex DepthVert
            #pragma fragment DepthFrag

            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            struct Attributes
            {
                float4 positionOS : POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            Varyings DepthVert(
                Attributes IN
            )
            {
                Varyings OUT;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                OUT.positionCS =
                    TransformObjectToHClip(
                        IN.positionOS.xyz
                    );

                return OUT;
            }


            half4 DepthFrag(
                Varyings IN
            )
                : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                return 0;
            }

            ENDHLSL
        }
    }


    // ================================================================
    // FALLBACK
    // ================================================================

    FallBack "Universal Render Pipeline/Lit"
}