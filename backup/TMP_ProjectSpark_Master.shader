Shader "ProjectSpark/UI/TMP ProjectSpark Master"
{
    Properties
    {
        // ============================================================
        // TMP SDF
        // ============================================================

        [PerRendererData]
        _MainTex ("Font Atlas", 2D) = "white" {}

        _FaceColor ("Face Color", Color) = (1,1,1,1)

        _VertexColor ("Vertex Color Multiplier", Color) = (1,1,1,1)

        _ScaleRatioA ("Scale Ratio A", Float) = 1
        _ScaleRatioB ("Scale Ratio B", Float) = 1
        _ScaleRatioC ("Scale Ratio C", Float) = 1


        // ============================================================
        // TEXT
        // ============================================================

        _FaceDilate ("Face Dilate", Range(-1,1)) = 0

        _Softness ("Text Softness", Range(0,1)) = 0

        _PerspectiveFilter ("Perspective Filter", Range(0,1)) = 0.875


        // ============================================================
        // OUTLINE
        // ============================================================

        [Toggle]
        _EnableOutline ("Enable Outline", Float) = 1

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)

        _OutlineWidth ("Outline Width", Range(0,1)) = 0.1

        _OutlineSoftness ("Outline Softness", Range(0,1)) = 0

        _OutlineDilate ("Outline Dilate", Range(-1,1)) = 0


        // ============================================================
        // GRADIENT
        // ============================================================

        [Toggle]
        _EnableGradient ("Enable Gradient", Float) = 0

        _GradientTop ("Gradient Top", Color) = (1,1,1,1)

        _GradientBottom ("Gradient Bottom", Color) = (0,0.7,1,1)

        _GradientOffset ("Gradient Offset", Range(-2,2)) = 0

        _GradientScale ("Gradient Scale", Range(0.1,10)) = 1


        // ============================================================
        // ANIMATED GRADIENT
        // ============================================================

        [Toggle]
        _EnableGradientAnimation ("Animated Gradient", Float) = 0

        _GradientSpeed ("Gradient Speed", Range(-10,10)) = 1

        _GradientDirection ("Gradient Direction", Range(-1,1)) = 1


        // ============================================================
        // EMISSION
        // ============================================================

        [Toggle]
        _EnableEmission ("Enable Emission", Float) = 0

        [HDR]
        _EmissionColor ("Emission Color", Color) = (0,1,1,1)

        _EmissionStrength ("Emission Strength", Range(0,20)) = 1


        // ============================================================
        // EMISSION PULSE
        // ============================================================

        [Toggle]
        _EnablePulse ("Enable Pulse", Float) = 0

        _PulseSpeed ("Pulse Speed", Range(0,20)) = 2

        _PulseMin ("Pulse Minimum", Range(0,1)) = 0.5

        _PulseMax ("Pulse Maximum", Range(0,5)) = 1


        // ============================================================
        // EMISSION WAVE
        // ============================================================

        [Toggle]
        _EnableEmissionWave ("Enable Emission Wave", Float) = 0

        _WaveSpeed ("Wave Speed", Range(-10,10)) = 2

        _WaveWidth ("Wave Width", Range(0.01,2)) = 0.2

        _WaveStrength ("Wave Strength", Range(0,10)) = 1


        // ============================================================
        // SHIMMER
        // ============================================================

        [Toggle]
        _EnableShimmer ("Enable Shimmer", Float) = 0

        _ShimmerSpeed ("Shimmer Speed", Range(-10,10)) = 2

        _ShimmerWidth ("Shimmer Width", Range(0.01,1)) = 0.15

        _ShimmerStrength ("Shimmer Strength", Range(0,5)) = 1


        // ============================================================
        // SCANLINE
        // ============================================================

        [Toggle]
        _EnableScanline ("Enable Scanline", Float) = 0

        _ScanlineSpeed ("Scanline Speed", Range(-10,10)) = 1

        _ScanlineWidth ("Scanline Width", Range(0.01,1)) = 0.1

        _ScanlineStrength ("Scanline Strength", Range(0,5)) = 1


        // ============================================================
        // FLICKER
        // ============================================================

        [Toggle]
        _EnableFlicker ("Enable Flicker", Float) = 0

        _FlickerSpeed ("Flicker Speed", Range(0,50)) = 10

        _FlickerAmount ("Flicker Amount", Range(0,1)) = 0.1


        // ============================================================
        // UNDERLAY / SHADOW
        // ============================================================

        [Toggle]
        _EnableUnderlay ("Enable Underlay", Float) = 0

        _UnderlayColor ("Underlay Color", Color) = (0,0,0,0.5)

        _UnderlayOffsetX ("Underlay Offset X", Range(-1,1)) = 0.1

        _UnderlayOffsetY ("Underlay Offset Y", Range(-1,1)) = -0.1

        _UnderlayDilate ("Underlay Dilate", Range(-1,1)) = 0

        _UnderlaySoftness ("Underlay Softness", Range(0,1)) = 0.1


        // ============================================================
        // GLOBAL FADE
        // ============================================================

        _GlobalAlpha ("Global Alpha", Range(0,1)) = 1


        // ============================================================
        // ANIMATED FADE
        // ============================================================

        [Toggle]
        _EnableFadeAnimation ("Animated Fade", Float) = 0

        _FadeSpeed ("Fade Speed", Range(0,10)) = 1

        _FadeMin ("Fade Minimum", Range(0,1)) = 0.2

        _FadeMax ("Fade Maximum", Range(0,1)) = 1


        // ============================================================
        // UI CLIPPING
        // ============================================================

        _ClipRect ("Clip Rect", Vector) = (-32767,-32767,32767,32767)

        _MaskSoftnessX ("Mask Softness X", Float) = 0

        _MaskSoftnessY ("Mask Softness Y", Float) = 0


        // ============================================================
        // STENCIL
        // ============================================================

        _StencilComp ("Stencil Comparison", Float) = 8

        _Stencil ("Stencil ID", Float) = 0

        _StencilOp ("Stencil Operation", Float) = 0

        _StencilWriteMask ("Stencil Write Mask", Float) = 255

        _StencilReadMask ("Stencil Read Mask", Float) = 255


        // ============================================================
        // COLOR MASK
        // ============================================================

        _ColorMask ("Color Mask", Float) = 15
    }


    SubShader
    {
        Tags
        {
            "Queue"="Transparent"

            "IgnoreProjector"="True"

            "RenderType"="Transparent"

            "PreviewType"="Plane"

            "CanUseSpriteAtlas"="True"
        }


        // ============================================================
        // STENCIL
        // ============================================================

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

        Blend SrcAlpha OneMinusSrcAlpha

        ColorMask [_ColorMask]


        Pass
        {
            CGPROGRAM

            #pragma vertex vert

            #pragma fragment frag

            #pragma target 3.0


            #include "UnityCG.cginc"

            #include "UnityUI.cginc"


            // ========================================================
            // STRUCTURES
            // ========================================================

            struct appdata_t
            {
                float4 vertex : POSITION;

                float4 color : COLOR;

                float2 uv : TEXCOORD0;
            };


            struct v2f
            {
                float4 vertex : SV_POSITION;

                float4 color : COLOR;

                float2 uv : TEXCOORD0;

                float4 worldPosition : TEXCOORD1;

                float4 screenPosition : TEXCOORD2;
            };


            // ========================================================
            // TEXTURE
            // ========================================================

            sampler2D _MainTex;

            float4 _MainTex_TexelSize;


            // ========================================================
            // COLORS
            // ========================================================

            float4 _FaceColor;

            float4 _VertexColor;


            // ========================================================
            // TEXT
            // ========================================================

            float _FaceDilate;

            float _Softness;

            float _PerspectiveFilter;


            // ========================================================
            // OUTLINE
            // ========================================================

            float _EnableOutline;

            float4 _OutlineColor;

            float _OutlineWidth;

            float _OutlineSoftness;

            float _OutlineDilate;


            // ========================================================
            // GRADIENT
            // ========================================================

            float _EnableGradient;

            float4 _GradientTop;

            float4 _GradientBottom;

            float _GradientOffset;

            float _GradientScale;


            // ========================================================
            // GRADIENT ANIMATION
            // ========================================================

            float _EnableGradientAnimation;

            float _GradientSpeed;

            float _GradientDirection;


            // ========================================================
            // EMISSION
            // ========================================================

            float _EnableEmission;

            float4 _EmissionColor;

            float _EmissionStrength;


            // ========================================================
            // PULSE
            // ========================================================

            float _EnablePulse;

            float _PulseSpeed;

            float _PulseMin;

            float _PulseMax;


            // ========================================================
            // WAVE
            // ========================================================

            float _EnableEmissionWave;

            float _WaveSpeed;

            float _WaveWidth;

            float _WaveStrength;


            // ========================================================
            // SHIMMER
            // ========================================================

            float _EnableShimmer;

            float _ShimmerSpeed;

            float _ShimmerWidth;

            float _ShimmerStrength;


            // ========================================================
            // SCANLINE
            // ========================================================

            float _EnableScanline;

            float _ScanlineSpeed;

            float _ScanlineWidth;

            float _ScanlineStrength;


            // ========================================================
            // FLICKER
            // ========================================================

            float _EnableFlicker;

            float _FlickerSpeed;

            float _FlickerAmount;


            // ========================================================
            // UNDERLAY
            // ========================================================

            float _EnableUnderlay;

            float4 _UnderlayColor;

            float _UnderlayOffsetX;

            float _UnderlayOffsetY;

            float _UnderlayDilate;

            float _UnderlaySoftness;


            // ========================================================
            // ALPHA
            // ========================================================

            float _GlobalAlpha;


            float _EnableFadeAnimation;

            float _FadeSpeed;

            float _FadeMin;

            float _FadeMax;


            // ========================================================
            // CLIPPING
            // ========================================================

            float4 _ClipRect;

            float _MaskSoftnessX;

            float _MaskSoftnessY;


            // ========================================================
            // VERTEX
            // ========================================================

            v2f vert(appdata_t v)
            {
                v2f o;


                o.vertex =
                    UnityObjectToClipPos(v.vertex);


                o.uv =
                    v.uv;


                o.color =
                    v.color
                    *
                    _VertexColor;


                o.worldPosition =
                    v.vertex;


                o.screenPosition =
                    ComputeScreenPos(
                        o.vertex
                    );


                return o;
            }


            // ========================================================
            // SDF SAMPLE
            // ========================================================

            float SampleSDF(
                float2 uv
            )
            {
                return tex2D(
                    _MainTex,
                    uv
                ).a;
            }


            // ========================================================
            // OUTLINE
            // ========================================================

            float CalculateOutline(
                float2 uv
            )
            {
                float2 pixel =
                    _MainTex_TexelSize.xy
                    *
                    _OutlineWidth
                    *
                    10;


                float a0 =
                    SampleSDF(
                        uv
                        + float2(
                            pixel.x,
                            0
                        )
                    );


                float a1 =
                    SampleSDF(
                        uv
                        - float2(
                            pixel.x,
                            0
                        )
                    );


                float a2 =
                    SampleSDF(
                        uv
                        + float2(
                            0,
                            pixel.y
                        )
                    );


                float a3 =
                    SampleSDF(
                        uv
                        - float2(
                            0,
                            pixel.y
                        )
                    );


                float outline =
                    max(
                        max(a0,a1),
                        max(a2,a3)
                    );


                return saturate(
                    outline
                );
            }


            // ========================================================
            // FRAGMENT
            // ========================================================

            fixed4 frag(
                v2f i
            ) : SV_Target
            {
                // ====================================================
                // SAMPLE FONT
                // ====================================================

                float sdf =
                    SampleSDF(
                        i.uv
                    );


                // ====================================================
                // SDF EDGE
                // ====================================================

                float width =
                    fwidth(
                        sdf
                    );


                float face =
                    smoothstep(
                        0.5
                        - width
                        - _Softness,

                        0.5
                        + width
                        + _Softness,

                        sdf
                    );


                // ====================================================
                // OUTLINE
                // ====================================================

                float outline =
                    CalculateOutline(
                        i.uv
                    );


                float outlineMask =
                    outline
                    *
                    _EnableOutline;


                outlineMask =
                    saturate(
                        outlineMask
                        - face
                    );


                // ====================================================
                // GRADIENT
                // ====================================================

                float gradientPosition =
                    saturate(
                        (
                            i.uv.y
                            +
                            _GradientOffset
                        )
                        *
                        _GradientScale
                    );


                if (
                    _EnableGradientAnimation
                    >
                    0.5
                )
                {
                    gradientPosition =
                        frac(
                            gradientPosition
                            +
                            _Time.y
                            *
                            _GradientSpeed
                            *
                            _GradientDirection
                        );
                }


                float4 gradientColor =
                    lerp(
                        _GradientBottom,

                        _GradientTop,

                        gradientPosition
                    );


                float4 textColor =
                    lerp(
                        _FaceColor,

                        gradientColor,

                        _EnableGradient
                    );


                textColor *=
                    i.color;


                // ====================================================
                // BASE COLOR
                // ====================================================

                float3 finalRGB =
                    textColor.rgb;


                // ====================================================
                // OUTLINE COLOR
                // ====================================================

                finalRGB =
                    lerp(
                        _OutlineColor.rgb,

                        finalRGB,

                        face
                    );


                // ====================================================
                // UNDERLAY
                // ====================================================

                if (
                    _EnableUnderlay
                    >
                    0.5
                )
                {
                    float2 underlayUV =
                        i.uv
                        +
                        float2(
                            _UnderlayOffsetX,

                            _UnderlayOffsetY
                        )
                        *
                        _MainTex_TexelSize.xy
                        *
                        10;


                    float underlay =
                        SampleSDF(
                            underlayUV
                        );


                    float underlayAlpha =
                        smoothstep(
                            0.5
                            -
                            _UnderlaySoftness,

                            0.5
                            +
                            _UnderlaySoftness,

                            underlay
                        );


                    finalRGB =
                        lerp(
                            _UnderlayColor.rgb,

                            finalRGB,

                            saturate(
                                face
                                +
                                outlineMask
                            )
                        );
                }


                // ====================================================
                // EMISSION POWER
                // ====================================================

                float emissionPower =
                    _EmissionStrength;


                // ====================================================
                // PULSE
                // ====================================================

                if (
                    _EnablePulse
                    >
                    0.5
                )
                {
                    float pulse =
                        sin(
                            _Time.y
                            *
                            _PulseSpeed
                        );


                    pulse =
                        pulse
                        *
                        0.5
                        +
                        0.5;


                    emissionPower *=
                        lerp(
                            _PulseMin,

                            _PulseMax,

                            pulse
                        );
                }


                // ====================================================
                // EMISSION WAVE
                // ====================================================

                if (
                    _EnableEmissionWave
                    >
                    0.5
                )
                {
                    float wave =
                        sin(
                            (
                                i.uv.y
                                *
                                10
                            )
                            -
                            (
                                _Time.y
                                *
                                _WaveSpeed
                            )
                        );


                    wave =
                        wave
                        *
                        0.5
                        +
                        0.5;


                    emissionPower +=
                        wave
                        *
                        _WaveStrength;
                }


                // ====================================================
                // SHIMMER
                // ====================================================

                if (
                    _EnableShimmer
                    >
                    0.5
                )
                {
                    float shimmerPosition =
                        frac(
                            i.uv.x
                            +
                            _Time.y
                            *
                            _ShimmerSpeed
                        );


                    float shimmer =
                        smoothstep(
                            0,

                            _ShimmerWidth,

                            shimmerPosition
                        )
                        *
                        smoothstep(
                            1,

                            1
                            -
                            _ShimmerWidth,

                            shimmerPosition
                        );


                    finalRGB +=
                        _EmissionColor.rgb
                        *
                        shimmer
                        *
                        _ShimmerStrength;
                }


                // ====================================================
                // SCANLINE
                // ====================================================

                if (
                    _EnableScanline
                    >
                    0.5
                )
                {
                    float scan =
                        frac(
                            i.uv.y
                            +
                            _Time.y
                            *
                            _ScanlineSpeed
                        );


                    float scanMask =
                        smoothstep(
                            0,

                            _ScanlineWidth,

                            scan
                        )
                        *
                        smoothstep(
                            1,

                            1
                            -
                            _ScanlineWidth,

                            scan
                        );


                    finalRGB +=
                        _EmissionColor.rgb
                        *
                        scanMask
                        *
                        _ScanlineStrength;
                }


                // ====================================================
                // EMISSION
                // ====================================================

                if (
                    _EnableEmission
                    >
                    0.5
                )
                {
                    finalRGB +=
                        _EmissionColor.rgb
                        *
                        emissionPower
                        *
                        face;
                }


                // ====================================================
                // FLICKER
                // ====================================================

                float flicker =
                    1;


                if (
                    _EnableFlicker
                    >
                    0.5
                )
                {
                    flicker =
                        sin(
                            _Time.y
                            *
                            _FlickerSpeed
                        );


                    flicker =
                        flicker
                        *
                        0.5
                        +
                        0.5;


                    flicker =
                        lerp(
                            1
                            -
                            _FlickerAmount,

                            1,

                            flicker
                        );
                }


                finalRGB *=
                    flicker;


                // ====================================================
                // ALPHA
                // ====================================================

                float alpha =
                    max(
                        face,

                        outlineMask
                    );


                alpha *=
                    textColor.a;


                // ====================================================
                // GLOBAL FADE
                // ====================================================

                float globalFade =
                    _GlobalAlpha;


                if (
                    _EnableFadeAnimation
                    >
                    0.5
                )
                {
                    float fade =
                        sin(
                            _Time.y
                            *
                            _FadeSpeed
                        );


                    fade =
                        fade
                        *
                        0.5
                        +
                        0.5;


                    globalFade =
                        lerp(
                            _FadeMin,

                            _FadeMax,

                            fade
                        );
                }


                alpha *=
                    globalFade;


                // ====================================================
                // UI CLIPPING
                // ====================================================

                float2 clipPosition =
                    i.worldPosition.xy;


                float2 inside =
                    step(
                        _ClipRect.xy,

                        clipPosition
                    )
                    *
                    step(
                        clipPosition,

                        _ClipRect.zw
                    );


                alpha *=
                    inside.x
                    *
                    inside.y;


                // ====================================================
                // OUTPUT
                // ====================================================

                return fixed4(
                    finalRGB,

                    alpha
                );
            }

            ENDCG
        }
    }


    FallBack "TextMeshPro/Distance Field"
}