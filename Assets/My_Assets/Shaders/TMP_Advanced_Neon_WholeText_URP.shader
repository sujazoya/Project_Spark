Shader "Custom/TMP Advanced Neon Whole Text URP"
{
    Properties
    {
        [PerRendererData]
        _MainTex ("Font Atlas", 2D) = "white" {}

        [HDR]
        _FaceColor ("Face Color", Color) = (1,1,1,1)

        [HDR]
        _GradientColorA ("Gradient Color A", Color) = (0,1,1,1)

        [HDR]
        _GradientColorB ("Gradient Color B", Color) = (0,0.2,1,1)

        [HDR]
        _GradientColorC ("Gradient Color C", Color) = (1,0,1,1)

        [Enum(Horizontal,0,Vertical,1,Diagonal,2)]
        _GradientDirection ("Gradient Direction", Float) = 0

        _GradientSpeed ("Gradient Animation Speed", Float) = 0

        _GradientOffset ("Gradient Offset", Float) = 0

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

        // ============================================================
        // WHOLE TEXT BOUNDS
        // ============================================================

        _TextMin ("Text Min", Vector) = (0,0,0,0)

        _TextMax ("Text Max", Vector) = (1,1,0,0)

        // ============================================================
        // UI STENCIL
        // ============================================================

        _StencilComp ("Stencil Comparison", Float) = 8

        _Stencil ("Stencil ID", Float) = 0

        _StencilOp ("Stencil Operation", Float) = 0

        _StencilWriteMask ("Stencil Write Mask", Float) = 255

        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [HideInInspector]
        _ClipRect ("Clip Rect", Vector) = (-32767,-32767,32767,32767)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "PreviewType"="Plane"
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
            Name "TMP Advanced Neon"

            HLSLPROGRAM

            #pragma target 3.0

            #pragma vertex vert

            #pragma fragment frag

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            // ========================================================
            // TEXTURE
            // ========================================================

            TEXTURE2D(_MainTex);

            SAMPLER(sampler_MainTex);


            // ========================================================
            // MATERIAL
            // ========================================================

            CBUFFER_START(UnityPerMaterial)

                float4 _MainTex_ST;

                float4 _FaceColor;

                float4 _GradientColorA;

                float4 _GradientColorB;

                float4 _GradientColorC;

                float _GradientDirection;

                float _GradientSpeed;

                float _GradientOffset;

                float4 _OutlineColor;

                float _OutlineWidth;

                float4 _EmissionColor;

                float _EmissionIntensity;

                float4 _GlowColor;

                float _GlowStrength;

                float _GlowSoftness;

                float4 _TextMin;

                float4 _TextMax;

                float4 _ClipRect;

            CBUFFER_END


            // ========================================================
            // VERTEX
            // ========================================================

            struct Attributes
            {
                float4 positionOS : POSITION;

                float4 color : COLOR;

                float2 uv : TEXCOORD0;

                float2 uv2 : TEXCOORD1;
            };


            // ========================================================
            // VARYINGS
            // ========================================================

            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                float4 color : COLOR;

                float2 atlasUV : TEXCOORD0;

                float2 textPosition : TEXCOORD1;

                float4 worldPosition : TEXCOORD2;
            };


            // ========================================================
            // VERTEX
            // ========================================================

            Varyings vert(
                Attributes input
            )
            {
                Varyings output;

                output.positionCS =
                    TransformObjectToHClip(
                        input.positionOS.xyz
                    );

                output.color =
                    input.color;

                output.atlasUV =
                    TRANSFORM_TEX(
                        input.uv,
                        _MainTex
                    );

                // IMPORTANT:
                //
                // This is the local position of the actual
                // TMP character vertex.
                //
                // C# sends the COMPLETE text bounds.
                //
                // Therefore every character shares
                // the SAME gradient coordinate system.

                output.textPosition =
                    input.positionOS.xy;

                output.worldPosition =
                    input.positionOS;

                return output;
            }


            // ========================================================
            // WHOLE TEXT UV
            // ========================================================

            float2 GetWholeTextUV(
                float2 position
            )
            {
                float2 size =
                    _TextMax.xy -
                    _TextMin.xy;

                size =
                    max(
                        size,
                        float2(
                            0.0001,
                            0.0001
                        )
                    );

                float2 uv =
                    (
                        position -
                        _TextMin.xy
                    )
                    /
                    size;

                return saturate(
                    uv
                );
            }


            // ========================================================
            // 3 COLOR GRADIENT
            // ========================================================

            float3 GetGradient(
                float t
            )
            {
                t =
                    saturate(
                        t
                    );

                if (t < 0.5)
                {
                    float localT =
                        t *
                        2.0;

                    return lerp(
                        _GradientColorA.rgb,

                        _GradientColorB.rgb,

                        localT
                    );
                }

                float localT =
                    (
                        t -
                        0.5
                    )
                    *
                    2.0;

                return lerp(
                    _GradientColorB.rgb,

                    _GradientColorC.rgb,

                    localT
                );
            }


            // ========================================================
            // FRAGMENT
            // ========================================================

            half4 frag(
                Varyings input
            )
                : SV_Target
            {
                // ====================================================
                // TMP SDF SAMPLE
                // ====================================================

                float sdf =
                    SAMPLE_TEXTURE2D(
                        _MainTex,

                        sampler_MainTex,

                        input.atlasUV
                    ).a;


                // ====================================================
                // SDF EDGE
                // ====================================================

                float2 dx =
                    ddx(
                        input.atlasUV
                    );

                float2 dy =
                    ddy(
                        input.atlasUV
                    );

                float texelScale =
                    max(
                        length(dx),
                        length(dy)
                    );

                float smoothing =
                    max(
                        fwidth(sdf),
                        0.0001
                    );


                // ====================================================
                // FACE
                // ====================================================

                float faceAlpha =
                    smoothstep(
                        0.5 -
                        smoothing,

                        0.5 +
                        smoothing,

                        sdf
                    );


                // ====================================================
                // WHOLE TEXT GRADIENT UV
                // ====================================================

                float2 wholeTextUV =
                    GetWholeTextUV(
                        input.textPosition
                    );


                // ====================================================
                // GRADIENT POSITION
                // ====================================================

                float gradientPosition;


                if (
                    _GradientDirection
                    <
                    0.5
                )
                {
                    // HORIZONTAL

                    gradientPosition =
                        wholeTextUV.x;
                }
                else if (
                    _GradientDirection
                    <
                    1.5
                )
                {
                    // VERTICAL

                    gradientPosition =
                        wholeTextUV.y;
                }
                else
                {
                    // DIAGONAL

                    gradientPosition =
                        (
                            wholeTextUV.x
                            +
                            wholeTextUV.y
                        )
                        *
                        0.5;
                }


                // ====================================================
                // ANIMATION
                // ====================================================

                gradientPosition +=
                    _GradientOffset;

                gradientPosition +=
                    _Time.y *
                    _GradientSpeed;

                gradientPosition =
                    frac(
                        gradientPosition
                    );


                // ====================================================
                // GRADIENT COLOR
                // ====================================================

                float3 gradientColor =
                    GetGradient(
                        gradientPosition
                    );


                // ====================================================
                // OUTLINE
                // ====================================================

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


                // ====================================================
                // OUTLINE ONLY
                // ====================================================

                float outlineOnly =
                    saturate(
                        outlineAlpha -
                        faceAlpha
                    );


                // ====================================================
                // GLOW
                // ====================================================

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
                    (1.0 -
                    faceAlpha);


                // ====================================================
                // FACE COLOR
                // ====================================================

                float3 faceColor =
                    gradientColor
                    *
                    _FaceColor.rgb
                    *
                    input.color.rgb;


                // ====================================================
                // EMISSION
                // ====================================================

                faceColor +=
                    gradientColor
                    *
                    _EmissionColor.rgb
                    *
                    _EmissionIntensity;


                // ====================================================
                // OUTLINE COLOR
                // ====================================================

                float3 outlineColor =
                    _OutlineColor.rgb
                    *
                    _OutlineColor.a;


                // ====================================================
                // FINAL COLOR
                // ====================================================

                float3 finalColor =
                    faceColor
                    *
                    faceAlpha;

                finalColor +=
                    outlineColor
                    *
                    outlineOnly;

                finalColor +=
                    _GlowColor.rgb
                    *
                    glow;


                // ====================================================
                // FINAL ALPHA
                // ====================================================

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


                // ====================================================
                // UI CLIPPING
                // ====================================================

                #if defined(UNITY_UI_CLIP_RECT)

                float2 clipFactor =
                    step(
                        _ClipRect.xy,

                        input.worldPosition.xy
                    )
                    *
                    step(
                        input.worldPosition.xy,

                        _ClipRect.zw
                    );

                finalAlpha *=
                    clipFactor.x *
                    clipFactor.y;

                #endif


                // ====================================================
                // ALPHA CLIP
                // ====================================================

                #if defined(UNITY_UI_ALPHACLIP)

                clip(
                    finalAlpha -
                    0.001
                );

                #endif


                // ====================================================
                // PREMULTIPLIED ALPHA
                // ====================================================

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