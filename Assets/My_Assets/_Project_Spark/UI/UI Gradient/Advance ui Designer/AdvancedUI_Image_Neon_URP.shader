Shader "Custom/UI/Advanced Image Neon URP"
{
    Properties
    {
        // =========================================================
        // MAIN IMAGE
        // =========================================================

        [PerRendererData]
        _MainTex ("UI Image", 2D) = "white" {}

        [HDR]
        _Color ("Base Color", Color) = (1,1,1,1)


        // =========================================================
        // 3 COLOR GRADIENT
        // =========================================================

        [Header(Gradient)]

        [HDR]
        _GradientTop ("Gradient Top", Color) =
            (0,1,1,1)

        [HDR]
        _GradientMiddle ("Gradient Middle", Color) =
            (0,0.2,1,1)

        [HDR]
        _GradientBottom ("Gradient Bottom", Color) =
            (1,0,1,1)

        _GradientOffset ("Gradient Offset", Range(-1,1)) = 0

        _GradientScale ("Gradient Scale", Range(0.01,5)) = 1

        [KeywordEnum(Vertical,Horizontal)]
        _GradientDirection ("Gradient Direction", Float) = 0


        // =========================================================
        // ROUNDED CORNERS
        // =========================================================

        [Header(Rounded Corners)]

        _CornerRadius ("Corner Radius", Range(0,0.5)) = 0.05

        _CornerSoftness ("Corner Softness", Range(0.001,0.2)) = 0.02


        // =========================================================
        // OUTLINE
        // =========================================================

        [Header(Outline)]

        [HDR]
        _OutlineColor ("Outline Color", Color) =
            (0,1,1,1)

        _OutlineWidth ("Outline Width", Range(0,0.1)) =
            0.01

        _OutlineSoftness ("Outline Softness", Range(0.001,0.1)) =
            0.01


        // =========================================================
        // BEVEL
        // =========================================================

        [Header(Bevel)]

        _BevelEnabled ("Bevel Enabled", Float) = 1

        _BevelWidth ("Bevel Width", Range(0,0.5)) =
            0.08

        _BevelStrength ("Bevel Strength", Range(0,2)) =
            1

        _BevelSoftness ("Bevel Softness", Range(0.001,0.2)) =
            0.03

        [HDR]
        _BevelHighlightColor ("Bevel Highlight", Color) =
            (1,1,1,1)

        [HDR]
        _BevelShadowColor ("Bevel Shadow", Color) =
            (0,0,0,1)


        // =========================================================
        // INNER SHADOW
        // =========================================================

        [Header(Inner Shadow)]

        _InnerShadowEnabled ("Inner Shadow Enabled", Float) = 1

        [HDR]
        _InnerShadowColor ("Inner Shadow Color", Color) =
            (0,0,0,1)

        _InnerShadowStrength ("Inner Shadow Strength", Range(0,2)) =
            0.5

        _InnerShadowOffsetX ("Inner Shadow X", Range(-0.5,0.5)) =
            0.02

        _InnerShadowOffsetY ("Inner Shadow Y", Range(-0.5,0.5)) =
            -0.02

        _InnerShadowSoftness ("Inner Shadow Softness", Range(0.001,0.2)) =
            0.05


        // =========================================================
        // GLOW
        // =========================================================

        [Header(Glow)]

        _GlowEnabled ("Glow Enabled", Float) = 1

        [HDR]
        _GlowColor ("Glow Color", Color) =
            (0,1,1,1)

        _GlowStrength ("Glow Strength", Range(0,5)) =
            1

        _GlowSoftness ("Glow Softness", Range(0.001,0.5)) =
            0.1


        // =========================================================
        // EMISSION
        // =========================================================

        [Header(Emission)]

        _EmissionEnabled ("Emission Enabled", Float) = 1

        [HDR]
        _EmissionColor ("Emission Color", Color) =
            (0,1,1,1)

        _EmissionIntensity ("Emission Intensity", Range(0,20)) =
            2


        // =========================================================
        // FRESNEL
        // =========================================================

        [Header(Fresnel)]

        _FresnelEnabled ("Fresnel Enabled", Float) = 1

        [HDR]
        _FresnelColor ("Fresnel Color", Color) =
            (0,1,1,1)

        _FresnelStrength ("Fresnel Strength", Range(0,5)) =
            1

        _FresnelPower ("Fresnel Power", Range(0.1,10)) =
            2


        // =========================================================
        // SCANLINES
        // =========================================================

        [Header(Scanlines)]

        _ScanlineEnabled ("Scanline Enabled", Float) = 0

        [HDR]
        _ScanlineColor ("Scanline Color", Color) =
            (0,1,1,1)

        _ScanlineDensity ("Scanline Density", Range(1,200)) =
            50

        _ScanlineStrength ("Scanline Strength", Range(0,1)) =
            0.2

        _ScanlineSpeed ("Scanline Speed", Range(-10,10)) =
            1


        // =========================================================
        // DISTORTION
        // =========================================================

        [Header(Distortion)]

        _DistortionEnabled ("Distortion Enabled", Float) = 0

        _DistortionStrength ("Distortion Strength", Range(0,0.1)) =
            0.01

        _DistortionScale ("Distortion Scale", Range(1,100)) =
            20

        _DistortionSpeed ("Distortion Speed", Range(-10,10)) =
            1


        // =========================================================
        // COLOR CORRECTION
        // =========================================================

        [Header(Color Correction)]

        _Brightness ("Brightness", Range(0,3)) =
            1

        _Contrast ("Contrast", Range(0,3)) =
            1

        _Saturation ("Saturation", Range(0,3)) =
            1


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

            "CanUseSpriteAtlas" = "True"
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

        ZWrite Off

        ZTest [unity_GUIZTestMode]

        Blend One OneMinusSrcAlpha

        ColorMask [_ColorMask]


        Pass
        {
            Name "Advanced UI Image"


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

                float4 _Color;


                float4 _GradientTop;

                float4 _GradientMiddle;

                float4 _GradientBottom;

                float _GradientOffset;

                float _GradientScale;

                float _GradientDirection;


                float _CornerRadius;

                float _CornerSoftness;


                float4 _OutlineColor;

                float _OutlineWidth;

                float _OutlineSoftness;


                float _BevelEnabled;

                float _BevelWidth;

                float _BevelStrength;

                float _BevelSoftness;

                float4 _BevelHighlightColor;

                float4 _BevelShadowColor;


                float _InnerShadowEnabled;

                float4 _InnerShadowColor;

                float _InnerShadowStrength;

                float _InnerShadowOffsetX;

                float _InnerShadowOffsetY;

                float _InnerShadowSoftness;


                float _GlowEnabled;

                float4 _GlowColor;

                float _GlowStrength;

                float _GlowSoftness;


                float _EmissionEnabled;

                float4 _EmissionColor;

                float _EmissionIntensity;


                float _FresnelEnabled;

                float4 _FresnelColor;

                float _FresnelStrength;

                float _FresnelPower;


                float _ScanlineEnabled;

                float4 _ScanlineColor;

                float _ScanlineDensity;

                float _ScanlineStrength;

                float _ScanlineSpeed;


                float _DistortionEnabled;

                float _DistortionStrength;

                float _DistortionScale;

                float _DistortionSpeed;


                float _Brightness;

                float _Contrast;

                float _Saturation;


                float4 _ClipRect;

            CBUFFER_END


            // =====================================================
            // VERTEX
            // =====================================================

            struct Attributes
            {
                float4 positionOS : POSITION;

                float4 color : COLOR;

                float2 uv : TEXCOORD0;
            };


            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                float4 color : COLOR;

                float2 uv : TEXCOORD0;

                float2 localPosition : TEXCOORD1;
            };


            Varyings Vert(
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


                output.uv =
                    TRANSFORM_TEX(
                        input.uv,

                        _MainTex
                    );


                output.localPosition =
                    input.positionOS.xy;


                return output;
            }


            // =====================================================
            // ROUNDED RECTANGLE SDF
            // =====================================================

            float RoundedBox(
                float2 p,

                float2 b,

                float r
            )
            {
                float2 q =
                    abs(p) -
                    b +
                    r;


                return
                    length(
                        max(
                            q,

                            0
                        )
                    )
                    +
                    min(
                        max(
                            q.x,

                            q.y
                        ),

                        0
                    )
                    -
                    r;
            }


            // =====================================================
            // GRADIENT
            // =====================================================

            float3 GetGradient(
                float2 uv
            )
            {
                float t;


                if (
                    _GradientDirection <
                    0.5
                )
                {
                    t =
                        uv.y;
                }
                else
                {
                    t =
                        uv.x;
                }


                t =
                    (
                        t -
                        0.5
                    )
                    *
                    _GradientScale
                    +
                    0.5
                    +
                    _GradientOffset;


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

                        t * 2
                    );
                }


                return lerp(
                    _GradientMiddle.rgb,

                    _GradientTop.rgb,

                    (
                        t -
                        0.5
                    )
                    *
                    2
                );
            }


            // =====================================================
            // RGB SATURATION
            // =====================================================

            float3 ApplySaturation(
                float3 color
            )
            {
                float luminance =
                    dot(
                        color,

                        float3(
                            0.2126,

                            0.7152,

                            0.0722
                        )
                    );


                return lerp(
                    luminance.xxx,

                    color,

                    _Saturation
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
                // DISTORTION
                // =================================================

                float2 uv =
                    input.uv;


                if (
                    _DistortionEnabled >
                    0.5
                )
                {
                    float waveX =
                        sin(
                            uv.y
                            *
                            _DistortionScale
                            +
                            _Time.y
                            *
                            _DistortionSpeed
                        );


                    float waveY =
                        cos(
                            uv.x
                            *
                            _DistortionScale
                            +
                            _Time.y
                            *
                            _DistortionSpeed
                        );


                    uv +=
                        float2(
                            waveX,

                            waveY
                        )
                        *
                        _DistortionStrength;
                }


                // =================================================
                // MAIN IMAGE
                // =================================================

                float4 texColor =
                    SAMPLE_TEXTURE2D(
                        _MainTex,

                        sampler_MainTex,

                        uv
                    );


                float alpha =
                    texColor.a
                    *
                    input.color.a
                    *
                    _Color.a;


                // =================================================
                // GRADIENT
                // =================================================

                float3 gradient =
                    GetGradient(
                        input.uv
                    );


                float3 finalColor =
                    gradient
                    *
                    _Color.rgb
                    *
                    texColor.rgb
                    *
                    input.color.rgb;


                // =================================================
                // ROUNDED CORNERS
                // =================================================

                float2 centeredUV =
                    input.uv -
                    0.5;


                float2 boxSize =
                    float2(
                        0.5,

                        0.5
                    );


                float roundedDistance =
                    RoundedBox(
                        centeredUV,

                        boxSize,

                        _CornerRadius
                    );


                float roundedAlpha =
                    1.0 -
                    smoothstep(
                        0.0,

                        _CornerSoftness,

                        roundedDistance
                    );


                alpha *=
                    roundedAlpha;


                // =================================================
                // OUTLINE
                // =================================================

                float outlineMask =
                    1.0 -
                    smoothstep(
                        _OutlineWidth,

                        _OutlineWidth +
                        _OutlineSoftness,

                        abs(
                            roundedDistance
                        )
                    );


                finalColor =
                    lerp(
                        _OutlineColor.rgb,

                        finalColor,

                        saturate(
                            roundedAlpha
                        )
                    );


                finalColor +=
                    _OutlineColor.rgb
                    *
                    outlineMask
                    *
                    _OutlineColor.a;


                // =================================================
                // BEVEL
                // =================================================

                if (
                    _BevelEnabled >
                    0.5
                )
                {
                    float edge =
                        smoothstep(
                            0,

                            _BevelWidth,

                            abs(
                                roundedDistance
                            )
                        );


                    float highlight =
                        (
                            1 -
                            input.uv.y
                        )
                        *
                        edge;


                    float shadow =
                        input.uv.y
                        *
                        edge;


                    finalColor +=
                        _BevelHighlightColor.rgb
                        *
                        highlight
                        *
                        _BevelStrength;


                    finalColor -=
                        _BevelShadowColor.rgb
                        *
                        shadow
                        *
                        _BevelStrength;
                }


                // =================================================
                // INNER SHADOW
                // =================================================

                if (
                    _InnerShadowEnabled >
                    0.5
                )
                {
                    float2 shadowUV =
                        input.uv
                        -
                        float2(
                            _InnerShadowOffsetX,

                            _InnerShadowOffsetY
                        );


                    float shadowDistance =
                        RoundedBox(
                            shadowUV -
                            0.5,

                            boxSize,

                            _CornerRadius
                        );


                    float shadowMask =
                        smoothstep(
                            0,

                            _InnerShadowSoftness,

                            shadowDistance
                        );


                    finalColor =
                        lerp(
                            finalColor,

                            _InnerShadowColor.rgb,

                            shadowMask
                            *
                            _InnerShadowStrength
                        );
                }


                // =================================================
                // GLOW
                // =================================================

                if (
                    _GlowEnabled >
                    0.5
                )
                {
                    float glow =
                        exp(
                            -
                            abs(
                                roundedDistance
                            )
                            /
                            max(
                                _GlowSoftness,

                                0.001
                            )
                        );


                    finalColor +=
                        _GlowColor.rgb
                        *
                        glow
                        *
                        _GlowStrength;
                }


                // =================================================
                // EMISSION
                // =================================================

                if (
                    _EmissionEnabled >
                    0.5
                )
                {
                    finalColor +=
                        gradient
                        *
                        _EmissionColor.rgb
                        *
                        _EmissionIntensity;
                }


                // =================================================
                // FRESNEL / EDGE
                // =================================================

                if (
                    _FresnelEnabled >
                    0.5
                )
                {
                    float edge =
                        pow(
                            saturate(
                                abs(
                                    input.uv.x -
                                    0.5
                                )
                                *
                                2
                            ),

                            _FresnelPower
                        );


                    finalColor +=
                        _FresnelColor.rgb
                        *
                        edge
                        *
                        _FresnelStrength;
                }


                // =================================================
                // SCANLINES
                // =================================================

                if (
                    _ScanlineEnabled >
                    0.5
                )
                {
                    float scan =
                        sin(
                            input.uv.y
                            *
                            _ScanlineDensity
                            +
                            _Time.y
                            *
                            _ScanlineSpeed
                        );


                    scan =
                        scan *
                        0.5
                        +
                        0.5;


                    finalColor =
                        lerp(
                            finalColor,

                            finalColor
                            +
                            _ScanlineColor.rgb,

                            scan
                            *
                            _ScanlineStrength
                        );
                }


                // =================================================
                // COLOR CORRECTION
                // =================================================

                finalColor *=
                    _Brightness;


                finalColor =
                    (
                        finalColor -
                        0.5
                    )
                    *
                    _Contrast
                    +
                    0.5;


                finalColor =
                    ApplySaturation(
                        finalColor
                    );


                // =================================================
                // UI RECT MASK
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


                    alpha *=
                        clipFactor.x
                        *
                        clipFactor.y;

                #endif


                // =================================================
                // ALPHA CLIP
                // =================================================

                #if defined(UNITY_UI_ALPHACLIP)

                    clip(
                        alpha -
                        0.001
                    );

                #endif


                // =================================================
                // PREMULTIPLIED ALPHA
                // =================================================

                finalColor *=
                    alpha;


                return half4(
                    finalColor,

                    alpha
                );
            }


            ENDHLSL
        }
    }
}