Shader "Custom/TMP Whole Text Vertical Neon Local Space"
{
    Properties
    {
        [PerRendererData]
        _MainTex ("TMP Font Atlas", 2D) = "white" {}

        // =========================================================
        // WHOLE TEXT VERTICAL GRADIENT
        // =========================================================

        [HDR]
        _GradientTop ("Gradient Top", Color) =
            (0, 1, 1, 1)

        [HDR]
        _GradientMiddle ("Gradient Middle", Color) =
            (0, 0.2, 1, 1)

        [HDR]
        _GradientBottom ("Gradient Bottom", Color) =
            (1, 0, 1, 1)


        // =========================================================
        // FACE
        // =========================================================

        [HDR]
        _FaceColor ("Face Color", Color) =
            (1, 1, 1, 1)


        // =========================================================
        // OUTLINE
        // =========================================================

        [HDR]
        _OutlineColor ("Outline Color", Color) =
            (0, 1, 1, 1)

        _OutlineWidth ("Outline Width", Range(0, 1)) =
            0.15


        // =========================================================
        // EMISSION
        // =========================================================

        [HDR]
        _EmissionColor ("Emission Color", Color) =
            (0, 1, 1, 1)

        _EmissionIntensity ("Emission Intensity", Range(0, 20)) =
            2


        // =========================================================
        // GLOW
        // =========================================================

        [HDR]
        _GlowColor ("Glow Color", Color) =
            (0, 1, 1, 1)

        _GlowStrength ("Glow Strength", Range(0, 10)) =
            1

        _GlowSoftness ("Glow Softness", Range(0.001, 1)) =
            0.1


        // =========================================================
        // WHOLE TEXT LOCAL-SPACE BOUNDS
        // =========================================================

        _TextMinY ("Text Local Min Y", Float) = 0

        _TextMaxY ("Text Local Max Y", Float) = 1


        // =========================================================
        // UI STENCIL
        // =========================================================

        _StencilComp ("Stencil Comparison", Float) = 8

        _Stencil ("Stencil ID", Float) = 0

        _StencilOp ("Stencil Operation", Float) = 0

        _StencilWriteMask ("Stencil Write Mask", Float) = 255

        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [HideInInspector]
        _ClipRect ("Clip Rect", Vector) =
            (-32767, -32767, 32767, 32767)
    }


    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "PreviewType" = "Plane"
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
            Name "TMP Whole Text Vertical Neon"


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
            // MATERIAL VARIABLES
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


                float _TextMinY;

                float _TextMaxY;


                float4 _ClipRect;

            CBUFFER_END


            // =====================================================
            // VERTEX INPUT
            // =====================================================

            struct Attributes
            {
                float4 positionOS : POSITION;

                float4 color : COLOR;

                float2 uv : TEXCOORD0;
            };


            // =====================================================
            // VERTEX OUTPUT
            // =====================================================

            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                float4 color : COLOR;

                float2 atlasUV : TEXCOORD0;

                // LOCAL SPACE Y
                float localY : TEXCOORD1;

                // LOCAL SPACE POSITION
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
                // TRANSFORM TO CLIP SPACE
                // -------------------------------------------------

                output.positionCS =
                    TransformObjectToHClip(
                        input.positionOS.xyz
                    );


                // -------------------------------------------------
                // TMP VERTEX COLOR
                // -------------------------------------------------

                output.color =
                    input.color;


                // -------------------------------------------------
                // FONT ATLAS UV
                // -------------------------------------------------

                output.atlasUV =
                    TRANSFORM_TEX(
                        input.uv,

                        _MainTex
                    );


                // =================================================
                // IMPORTANT
                //
                // This is BEFORE:
                //
                // Object -> World
                // World -> Screen
                //
                // Therefore this is LOCAL TEXT SPACE.
                //
                // Moving the UI object on screen does not change
                // this value.
                // =================================================

                output.localY =
                    input.positionOS.y;


                output.localPosition =
                    input.positionOS.xy;


                return output;
            }


            // =====================================================
            // 3-COLOR VERTICAL GRADIENT
            // =====================================================

            float3 GetVerticalGradient(
                float t
            )
            {
                t =
                    saturate(
                        t
                    );


                // -------------------------------------------------
                // BOTTOM -> MIDDLE
                // -------------------------------------------------

                if (
                    t <
                    0.5
                )
                {
                    float localT =
                        t *
                        2.0;


                    return lerp(
                        _GradientBottom.rgb,

                        _GradientMiddle.rgb,

                        localT
                    );
                }


                // -------------------------------------------------
                // MIDDLE -> TOP
                // -------------------------------------------------

                float localT =
                    (
                        t -
                        0.5
                    )
                    *
                    2.0;


                return lerp(
                    _GradientMiddle.rgb,

                    _GradientTop.rgb,

                    localT
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
                // SAMPLE TMP FONT SDF
                // =================================================

                float sdf =
                    SAMPLE_TEXTURE2D(
                        _MainTex,

                        sampler_MainTex,

                        input.atlasUV
                    ).a;


                // =================================================
                // SDF ANTI-ALIASING
                // =================================================

                float smoothing =
                    max(
                        fwidth(
                            sdf
                        ),

                        0.0001
                    );


                // =================================================
                // FACE ALPHA
                // =================================================

                float faceAlpha =
                    smoothstep(
                        0.5 -
                        smoothing,

                        0.5 +
                        smoothing,

                        sdf
                    );


                // =================================================
                // WHOLE TEXT LOCAL-SPACE GRADIENT
                // =================================================

                float textHeight =
                    max(
                        _TextMaxY -
                        _TextMinY,

                        0.0001
                    );


                // IMPORTANT:
                //
                // input.localY
                //
                // _TextMinY
                //
                // _TextMaxY
                //
                // ARE ALL IN THE SAME LOCAL SPACE.
                //
                // Therefore moving the TMP object around the
                // Canvas does NOT change the gradient.

                float verticalT =
                    (
                        input.localY -
                        _TextMinY
                    )
                    /
                    textHeight;


                verticalT =
                    saturate(
                        verticalT
                    );


                // =================================================
                // GET GRADIENT COLOR
                // =================================================

                float3 gradientColor =
                    GetVerticalGradient(
                        verticalT
                    );


                // =================================================
                // OUTLINE
                // =================================================

                float outlineThreshold =
                    0.5 -
                    _OutlineWidth;


                float outlineAlpha =
                    smoothstep(
                        outlineThreshold -
                        smoothing,

                        outlineThreshold +
                        smoothing,

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
                // FACE
                // =================================================

                float3 faceColor =
                    gradientColor
                    *
                    _FaceColor.rgb
                    *
                    input.color.rgb;


                // =================================================
                // EMISSION
                // =================================================

                faceColor +=
                    gradientColor
                    *
                    _EmissionColor.rgb
                    *
                    _EmissionIntensity;


                // =================================================
                // FINAL COLOR
                // =================================================

                float3 finalColor =
                    faceColor
                    *
                    faceAlpha;


                // =================================================
                // OUTLINE COLOR
                // =================================================

                finalColor +=
                    _OutlineColor.rgb
                    *
                    _OutlineColor.a
                    *
                    outlineOnly;


                // =================================================
                // GLOW COLOR
                // =================================================

                finalColor +=
                    _GlowColor.rgb
                    *
                    glow;


                // =================================================
                // FINAL ALPHA
                // =================================================

                float finalAlpha =
                    max(
                        faceAlpha,

                        outlineOnly
                    );


                finalAlpha =
                    max(
                        finalAlpha,

                        glow *
                        0.5
                    );


                finalAlpha *=
                    input.color.a;


                // =================================================
                // UI RECT CLIPPING
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