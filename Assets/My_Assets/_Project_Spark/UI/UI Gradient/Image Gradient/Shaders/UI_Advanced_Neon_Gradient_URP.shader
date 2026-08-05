Shader "Custom/UI Advanced Neon Gradient URP"
{
    Properties
    {
        [PerRendererData]
        _MainTex ("UI Image", 2D) = "white" {}

        [HDR]
        _Color ("Base Color", Color) = (1,1,1,1)

        // ============================================================
        // 3 COLOR GRADIENT
        // ============================================================

        [HDR]
        _GradientColorA ("Gradient Color A", Color) = (0,1,1,1)

        [HDR]
        _GradientColorB ("Gradient Color B", Color) = (0,0.25,1,1)

        [HDR]
        _GradientColorC ("Gradient Color C", Color) = (1,0,1,1)

        [Enum(Horizontal,0,Vertical,1,Diagonal,2)]
        _GradientDirection ("Gradient Direction", Float) = 0

        _GradientSpeed ("Gradient Animation Speed", Float) = 0

        _GradientOffset ("Gradient Offset", Range(0,1)) = 0

        // ============================================================
        // IMAGE UV SCALE
        // ============================================================

        _GradientScaleX ("Gradient Scale X", Float) = 1

        _GradientScaleY ("Gradient Scale Y", Float) = 1

        // ============================================================
        // OUTLINE
        // ============================================================

        [HDR]
        _OutlineColor ("Outline Color", Color) = (0,1,1,1)

        _OutlineWidth ("Outline Width", Range(0,0.1)) = 0.01

        // ============================================================
        // NEON
        // ============================================================

        [HDR]
        _EmissionColor ("Emission Color", Color) = (0,1,1,1)

        _EmissionIntensity ("Emission Intensity", Range(0,20)) = 2

        // ============================================================
        // GLOW
        // ============================================================

        [HDR]
        _GlowColor ("Glow Color", Color) = (0,1,1,1)

        _GlowStrength ("Glow Strength", Range(0,10)) = 1

        _GlowSize ("Glow Size", Range(0,0.25)) = 0.05

        _GlowSoftness ("Glow Softness", Range(0.001,0.25)) = 0.05

        // ============================================================
        // UI STENCIL
        // ============================================================

        _StencilComp ("Stencil Comparison", Float) = 8

        _Stencil ("Stencil ID", Float) = 0

        _StencilOp ("Stencil Operation", Float) = 0

        _StencilWriteMask ("Stencil Write Mask", Float) = 255

        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        // ============================================================
        // UI MASK
        // ============================================================

        [HideInInspector]
        _ClipRect ("Clip Rect", Vector) = (-32767,-32767,32767,32767)

        [HideInInspector]
        _UIMaskSoftnessX ("UI Mask Softness X", Float) = 0

        [HideInInspector]
        _UIMaskSoftnessY ("UI Mask Softness Y", Float) = 0
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
            "CanUseSpriteAtlas"="True"
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
            Name "UI Advanced Neon"

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
            // MATERIAL PROPERTIES
            // ========================================================

            CBUFFER_START(UnityPerMaterial)

                float4 _Color;

                float4 _GradientColorA;

                float4 _GradientColorB;

                float4 _GradientColorC;

                float _GradientDirection;

                float _GradientSpeed;

                float _GradientOffset;

                float _GradientScaleX;

                float _GradientScaleY;

                float4 _OutlineColor;

                float _OutlineWidth;

                float4 _EmissionColor;

                float _EmissionIntensity;

                float4 _GlowColor;

                float _GlowStrength;

                float _GlowSize;

                float _GlowSoftness;

                float4 _ClipRect;

                float _UIMaskSoftnessX;

                float _UIMaskSoftnessY;

            CBUFFER_END

            // ========================================================
            // VERTEX
            // ========================================================

            struct Attributes
            {
                float4 positionOS : POSITION;

                float4 color : COLOR;

                float2 uv : TEXCOORD0;
            };

            // ========================================================
            // VARYINGS
            // ========================================================

            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                float4 color : COLOR;

                float2 uv : TEXCOORD0;

                float2 localUV : TEXCOORD1;

                float4 worldPosition : TEXCOORD2;
            };

            // ========================================================
            // VERTEX SHADER
            // ========================================================

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionCS =
                    TransformObjectToHClip(
                        input.positionOS.xyz
                    );

                output.color =
                    input.color;

                output.uv =
                    input.uv;

                output.localUV =
                    input.uv;

                output.worldPosition =
                    input.positionOS;

                return output;
            }

            // ========================================================
            // GRADIENT POSITION
            // ========================================================

            float GetGradientPosition(float2 uv)
            {
                float2 scaledUV;

                scaledUV.x =
                    uv.x *
                    _GradientScaleX;

                scaledUV.y =
                    uv.y *
                    _GradientScaleY;

                // ----------------------------------------------------
                // HORIZONTAL
                // ----------------------------------------------------

                if (_GradientDirection < 0.5)
                {
                    return saturate(
                        scaledUV.x
                    );
                }

                // ----------------------------------------------------
                // VERTICAL
                // ----------------------------------------------------

                if (_GradientDirection < 1.5)
                {
                    return saturate(
                        scaledUV.y
                    );
                }

                // ----------------------------------------------------
                // DIAGONAL
                // ----------------------------------------------------

                return saturate(
                    (
                        scaledUV.x +
                        scaledUV.y
                    )
                    *
                    0.5
                );
            }

            // ========================================================
            // 3 COLOR GRADIENT
            // ========================================================

            float3 GetThreeColorGradient(float t)
            {
                t =
                    saturate(t);

                float3 result;

                if (t < 0.5)
                {
                    float localT =
                        t *
                        2.0;

                    result =
                        lerp(
                            _GradientColorA.rgb,

                            _GradientColorB.rgb,

                            localT
                        );
                }
                else
                {
                    float localT =
                        (
                            t -
                            0.5
                        )
                        *
                        2.0;

                    result =
                        lerp(
                            _GradientColorB.rgb,

                            _GradientColorC.rgb,

                            localT
                        );
                }

                return result;
            }

            // ========================================================
            // MAIN FRAGMENT
            // ========================================================

            half4 frag(Varyings input)
                : SV_Target
            {
                // ----------------------------------------------------
                // SAMPLE IMAGE
                // ----------------------------------------------------

                half4 image =
                    SAMPLE_TEXTURE2D(
                        _MainTex,

                        sampler_MainTex,

                        input.uv
                    );

                // ----------------------------------------------------
                // ORIGINAL UI COLOR
                // ----------------------------------------------------

                half4 vertexColor =
                    input.color;

                // ----------------------------------------------------
                // ANIMATED GRADIENT
                // ----------------------------------------------------

                float gradientPosition =
                    GetGradientPosition(
                        input.localUV
                    );

                gradientPosition +=
                    _GradientOffset;

                gradientPosition +=
                    _Time.y *
                    _GradientSpeed;

                gradientPosition =
                    frac(
                        gradientPosition
                    );

                // ----------------------------------------------------
                // GET 3 COLOR GRADIENT
                // ----------------------------------------------------

                float3 gradient =
                    GetThreeColorGradient(
                        gradientPosition
                    );

                // ----------------------------------------------------
                // BASE IMAGE
                //
                // The image itself controls alpha.
                // Gradient controls RGB.
                // ----------------------------------------------------

                float3 baseColor =
                    image.rgb *
                    _Color.rgb *
                    gradient;

                // ----------------------------------------------------
                // EMISSION
                // ----------------------------------------------------

                float3 emission =
                    gradient
                    *
                    _EmissionColor.rgb
                    *
                    _EmissionIntensity;

                baseColor +=
                    emission;

                // ----------------------------------------------------
                // ALPHA
                // ----------------------------------------------------

                float alpha =
                    image.a
                    *
                    _Color.a
                    *
                    vertexColor.a;

                // ----------------------------------------------------
                // APPROXIMATE OUTLINE
                //
                // Samples neighboring pixels.
                // Works well for UI PNG/Sprite edges.
                // ----------------------------------------------------

                float2 texelSize =
                    1.0 /
                    float2(
                        _ScreenParams.x,

                        _ScreenParams.y
                    );

                float2 outlineOffset =
                    texelSize *
                    _OutlineWidth *
                    100.0;

                float alphaLeft =
                    SAMPLE_TEXTURE2D(
                        _MainTex,

                        sampler_MainTex,

                        input.uv +
                        float2(
                            -outlineOffset.x,
                            0
                        )
                    ).a;

                float alphaRight =
                    SAMPLE_TEXTURE2D(
                        _MainTex,

                        sampler_MainTex,

                        input.uv +
                        float2(
                            outlineOffset.x,
                            0
                        )
                    ).a;

                float alphaUp =
                    SAMPLE_TEXTURE2D(
                        _MainTex,

                        sampler_MainTex,

                        input.uv +
                        float2(
                            0,
                            outlineOffset.y
                        )
                    ).a;

                float alphaDown =
                    SAMPLE_TEXTURE2D(
                        _MainTex,

                        sampler_MainTex,

                        input.uv +
                        float2(
                            0,
                            -outlineOffset.y
                        )
                    ).a;

                float neighborAlpha =
                    max(
                        max(
                            alphaLeft,

                            alphaRight
                        ),

                        max(
                            alphaUp,

                            alphaDown
                        )
                    );

                float outlineMask =
                    saturate(
                        neighborAlpha -
                        image.a
                    );

                outlineMask *=
                    _OutlineColor.a;

                // ----------------------------------------------------
                // OUTLINE
                // ----------------------------------------------------

                float3 outline =
                    _OutlineColor.rgb
                    *
                    outlineMask;

                // ----------------------------------------------------
                // GLOW
                // ----------------------------------------------------

                float glowEdge =
                    max(
                        0,

                        neighborAlpha -
                        image.a
                    );

                float glow =
                    smoothstep(
                        0,

                        max(
                            _GlowSoftness,

                            0.001
                        ),

                        glowEdge
                    );

                glow *=
                    _GlowStrength;

                glow *=
                    _GlowColor.a;

                baseColor +=
                    _GlowColor.rgb
                    *
                    glow;

                // ----------------------------------------------------
                // COMBINE OUTLINE
                // ----------------------------------------------------

                baseColor =
                    lerp(
                        baseColor,

                        outline,

                        saturate(
                            outlineMask
                        )
                    );

                // ----------------------------------------------------
                // FINAL ALPHA
                // ----------------------------------------------------

                alpha =
                    max(
                        alpha,

                        outlineMask
                    );

                // ----------------------------------------------------
                // UI RECT CLIPPING
                // ----------------------------------------------------

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

                alpha *=
                    clipFactor.x *
                    clipFactor.y;

                #endif

                // ----------------------------------------------------
                // ALPHA CLIP
                // ----------------------------------------------------

                #if defined(UNITY_UI_ALPHACLIP)

                clip(
                    alpha -
                    0.001
                );

                #endif

                // ----------------------------------------------------
                // PREMULTIPLIED ALPHA
                // ----------------------------------------------------

                baseColor *=
                    alpha;

                return half4(
                    baseColor,

                    alpha
                );
            }

            ENDHLSL
        }
    }
}