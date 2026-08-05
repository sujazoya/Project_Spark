Shader "Custom/TMP Whole Text Vertical Gradient URP"
{
    Properties
    {
        [PerRendererData]
        _MainTex ("Font Atlas", 2D) = "white" {}

        [HDR]
        _GradientTop ("Gradient Top", Color) = (0,1,1,1)

        [HDR]
        _GradientMiddle ("Gradient Middle", Color) = (0,0.2,1,1)

        [HDR]
        _GradientBottom ("Gradient Bottom", Color) = (1,0,1,1)

        [HDR]
        _FaceColor ("Face Color", Color) = (1,1,1,1)

        [HDR]
        _OutlineColor ("Outline Color", Color) = (0,1,1,1)

        _OutlineWidth ("Outline Width", Range(0,1)) = 0.15

        [HDR]
        _EmissionColor ("Emission Color", Color) = (0,1,1,1)

        _EmissionIntensity ("Emission Intensity", Range(0,20)) = 2

        [HDR]
        _GlowColor ("Glow Color", Color) = (0,1,1,1)

        _GlowStrength ("Glow Strength", Range(0,10)) = 1

        _GlowSoftness ("Glow Softness", Range(0.001,1)) = 0.1

        _StencilComp ("Stencil Comparison", Float) = 8

        _Stencil ("Stencil ID", Float) = 0

        _StencilOp ("Stencil Operation", Float) = 0

        _StencilWriteMask ("Stencil Write Mask", Float) = 255

        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [HideInInspector]
        _ClipRect ("Clip Rect", Vector) =
            (-32767,-32767,32767,32767)
    }


    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"

            "IgnoreProjector" = "True"

            "RenderType" = "Transparent"

            "RenderPipeline" = "UniversalPipeline"
        }


        Stencil
        {
            Ref [_Stencil]

            Comp [_StencilComp]

            Pass [_StencilOp]

            ReadMask [_StencilReadMask]

            WriteMask [_StencilWriteMask]
        }


        Cull Off

        Lighting Off

        ZWrite Off

        ZTest [unity_GUIZTestMode]

        Blend One OneMinusSrcAlpha

        ColorMask [_ColorMask]


        Pass
        {
            Name "TMP Vertical Gradient"


            HLSLPROGRAM


            #pragma target 3.0

            #pragma vertex Vert

            #pragma fragment Frag


            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            // =====================================================
            // TEXTURE
            // =====================================================

            TEXTURE2D(_MainTex);

            SAMPLER(sampler_MainTex);


            // =====================================================
            // MATERIAL
            // =====================================================

            CBUFFER_START(UnityPerMaterial)

                float4 _MainTex_ST;

                float4 _GradientTop;

                float4 _GradientMiddle;

                float4 _GradientBottom;

                float4 _FaceColor;

                float4 _OutlineColor;

                float _OutlineWidth;

                float4 _EmissionColor;

                float _EmissionIntensity;

                float4 _GlowColor;

                float _GlowStrength;

                float _GlowSoftness;

                float4 _ClipRect;

            CBUFFER_END


            // =====================================================
            // INPUT
            // =====================================================

            struct Attributes
            {
                float4 positionOS : POSITION;

                float4 color : COLOR;

                float2 uv : TEXCOORD0;

                // C# WRITES WHOLE-TEXT NORMALIZED Y HERE
                float2 gradientUV : TEXCOORD1;
            };


            // =====================================================
            // OUTPUT
            // =====================================================

            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                float4 color : COLOR;

                float2 atlasUV : TEXCOORD0;

                // 0 = BOTTOM
                // 0.5 = MIDDLE
                // 1 = TOP

                float gradientY : TEXCOORD1;

                float2 localPosition : TEXCOORD2;
            };


            // =====================================================
            // VERTEX
            // =====================================================

            Varyings Vert(
                Attributes input
            )
            {
                Varyings output;


                // -------------------------------------------------
                // NORMAL TMP POSITION
                // -------------------------------------------------

                output.positionCS =
                    TransformObjectToHClip(
                        input.positionOS.xyz
                    );


                // -------------------------------------------------
                // TMP COLOR
                // -------------------------------------------------

                output.color =
                    input.color;


                // -------------------------------------------------
                // FONT ATLAS
                // -------------------------------------------------

                output.atlasUV =
                    TRANSFORM_TEX(
                        input.uv,

                        _MainTex
                    );


                // -------------------------------------------------
                // IMPORTANT
                //
                // THIS VALUE WAS CALCULATED BY C#
                // FROM THE COMPLETE TMP MESH.
                //
                // IT IS NOT SCREEN SPACE.
                //
                // IT DOES NOT CHANGE WHEN THE TEXT MOVES.
                // -------------------------------------------------

                output.gradientY =
                    input.gradientUV.y;


                output.localPosition =
                    input.positionOS.xy;


                return output;
            }


            // =====================================================
            // 3 COLOR GRADIENT
            // =====================================================

            float3 GetGradient(
                float t
            )
            {
                t =
                    saturate(
                        t
                    );


                if (
                    t <
                    0.5
                )
                {
                    return lerp(
                        _GradientBottom.rgb,

                        _GradientMiddle.rgb,

                        t * 2.0
                    );
                }


                return lerp(
                    _GradientMiddle.rgb,

                    _GradientTop.rgb,

                    (t - 0.5) * 2.0
                );
            }


            // =====================================================
            // FRAGMENT
            // =====================================================

            half4 Frag(
                Varyings input
            )
                : SV_Target
            {


                // =================================================
                // TMP SDF
                // =================================================

                float sdf =
                    SAMPLE_TEXTURE2D(
                        _MainTex,

                        sampler_MainTex,

                        input.atlasUV
                    ).a;


                // =================================================
                // SDF SMOOTHING
                // =================================================

                float smoothing =
                    max(
                        fwidth(
                            sdf
                        ),

                        0.0001
                    );


                // =================================================
                // FACE
                // =================================================

                float faceAlpha =
                    smoothstep(
                        0.5 - smoothing,

                        0.5 + smoothing,

                        sdf
                    );


                // =================================================
                // WHOLE TEXT GRADIENT
                //
                // DIRECTLY FROM UV2
                //
                // NOT SCREEN SPACE
                // =================================================

                float gradientPosition =
                    saturate(
                        input.gradientY
                    );


                float3 gradientColor =
                    GetGradient(
                        gradientPosition
                    );


                // =================================================
                // OUTLINE
                // =================================================

                float outlineThreshold =
                    0.5 -
                    _OutlineWidth;


                float outlineAlpha =
                    smoothstep(
                        outlineThreshold - smoothing,

                        outlineThreshold + smoothing,

                        sdf
                    );


                float outlineOnly =
                    saturate(
                        outlineAlpha -
                        faceAlpha
                    );


                // =================================================
                // GLOW
                // =================================================

                float glow =
                    exp(
                        -abs(
                            sdf -
                            0.5
                        )
                        /
                        max(
                            _GlowSoftness,

                            0.001
                        )
                    );


                glow *=
                    _GlowStrength;


                glow *=
                    1.0 -
                    faceAlpha;


                // =================================================
                // FACE COLOR
                // =================================================

                float3 finalColor =
                    gradientColor
                    *
                    _FaceColor.rgb;


                // =================================================
                // EMISSION
                // =================================================

                finalColor +=
                    gradientColor
                    *
                    _EmissionColor.rgb
                    *
                    _EmissionIntensity;


                finalColor *=
                    faceAlpha;


                // =================================================
                // OUTLINE
                // =================================================

                finalColor +=
                    _OutlineColor.rgb
                    *
                    _OutlineColor.a
                    *
                    outlineOnly;


                // =================================================
                // GLOW
                // =================================================

                finalColor +=
                    _GlowColor.rgb
                    *
                    glow;


                // =================================================
                // ALPHA
                // =================================================

                float finalAlpha =
                    max(
                        faceAlpha,

                        outlineOnly
                    );


                finalAlpha =
                    max(
                        finalAlpha,

                        glow * 0.5
                    );


                finalAlpha *=
                    input.color.a;


                // =================================================
                // UI CLIPPING
                // =================================================

                #if defined(UNITY_UI_CLIP_RECT)

                    float2 clipFactor =
                        step(
                            _ClipRect.xy,

                            input.localPosition
                        )
                        *
                        step(
                            input.localPosition,

                            _ClipRect.zw
                        );


                    finalAlpha *=
                        clipFactor.x
                        *
                        clipFactor.y;

                #endif


                // =================================================
                // ALPHA CLIP
                // =================================================

                #if defined(UNITY_UI_ALPHACLIP)

                    clip(
                        finalAlpha -
                        0.001
                    );

                #endif


                // =================================================
                // PREMULTIPLIED ALPHA
                // =================================================

                finalColor *=
                    finalAlpha;


                return half4(
                    finalColor,

                    finalAlpha
                );
            }


            ENDHLSL
        }
    }
}