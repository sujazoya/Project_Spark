Shader "Custom/TMP Advanced Neon URP Standalone"
{
    Properties
    {
        // ============================================================
        // TMP FONT ATLAS
        // ============================================================

        [PerRendererData]
        _MainTex ("Font Atlas", 2D) = "white" {}

        _TextureWidth ("Texture Width", Float) = 512
        _TextureHeight ("Texture Height", Float) = 512

        _GradientScale ("SDF Gradient Scale", Float) = 5

        _FaceDilate ("Face Dilate", Range(-1,1)) = 0


        // ============================================================
        // FACE
        // ============================================================

        [HDR]
        _FaceColor ("Face Color", Color) = (1,1,1,1)


        // ============================================================
        // 3 COLOR WHOLE TEXT GRADIENT
        // ============================================================

        [HDR]
        _GradientColorA ("Gradient Color A", Color) = (0,1,1,1)

        [HDR]
        _GradientColorB ("Gradient Color B", Color) = (0,0.2,1,1)

        [HDR]
        _GradientColorC ("Gradient Color C", Color) = (1,0,1,1)


       [Enum(Horizontal,0,Vertical,1,Diagonal,2)]
_GradientDirection ("Gradient Direction", Float) = 0
        _GradientDirection
        (
            "Gradient Direction",
            Float
        ) = 0


        _GradientSpeed
        (
            "Gradient Animation Speed",
            Float
        ) = 0


        _GradientOffset
        (
            "Gradient Offset",
            Range(0,1)
        ) = 0


        // ============================================================
        // WHOLE TEXT BOUNDS
        // Updated by C#
        // ============================================================

        _TextMin
        (
            "Text Bounds Min",
            Vector
        ) = (0,0,0,0)


        _TextMax
        (
            "Text Bounds Max",
            Vector
        ) = (1,1,0,0)


        // ============================================================
        // OUTLINE
        // ============================================================

        [HDR]
        _OutlineColor
        (
            "Outline Color",
            Color
        ) = (0,0,0,1)


        _OutlineWidth
        (
            "Outline Width",
            Range(0,1)
        ) = 0.1


        _OutlineSoftness
        (
            "Outline Softness",
            Range(0,1)
        ) = 0


        // ============================================================
        // NEON EMISSION
        // ============================================================

        [HDR]
        _EmissionColor
        (
            "Emission Color",
            Color
        ) = (0,1,1,1)


        _EmissionIntensity
        (
            "Emission Intensity",
            Range(0,20)
        ) = 0


        // ============================================================
        // GLOW
        // ============================================================

        [HDR]
        _GlowColor
        (
            "Glow Color",
            Color
        ) = (0,1,1,1)


        _GlowStrength
        (
            "Glow Strength",
            Range(0,10)
        ) = 0


        _GlowSize
        (
            "Glow Size",
            Range(0,1)
        ) = 0.25


        _GlowSoftness
        (
            "Glow Softness",
            Range(0.001,1)
        ) = 0.25


        // ============================================================
        // TMP SCALE
        // ============================================================

        _ScaleX
        (
            "Scale X",
            Float
        ) = 1


        _ScaleY
        (
            "Scale Y",
            Float
        ) = 1


        _Sharpness
        (
            "Sharpness",
            Range(-1,1)
        ) = 0


        // ============================================================
        // UI MASK
        // ============================================================

        _ClipRect
        (
            "Clip Rect",
            Vector
        ) = (-32767,-32767,32767,32767)


        _MaskSoftnessX
        (
            "Mask Softness X",
            Float
        ) = 0


        _MaskSoftnessY
        (
            "Mask Softness Y",
            Float
        ) = 0


        // ============================================================
        // STENCIL
        // ============================================================

        _StencilComp
        (
            "Stencil Comparison",
            Float
        ) = 8


        _Stencil
        (
            "Stencil ID",
            Float
        ) = 0


        _StencilOp
        (
            "Stencil Operation",
            Float
        ) = 0


        _StencilWriteMask
        (
            "Stencil Write Mask",
            Float
        ) = 255


        _StencilReadMask
        (
            "Stencil Read Mask",
            Float
        ) = 255


        // ============================================================
        // RENDER
        // ============================================================

        _CullMode
        (
            "Cull Mode",
            Float
        ) = 0


        _ColorMask
        (
            "Color Mask",
            Float
        ) = 15
    }


    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
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


        Cull [_CullMode]

        ZWrite Off

        ZTest [unity_GUIZTestMode]

        Blend One OneMinusSrcAlpha

        ColorMask [_ColorMask]


        Pass
        {
            Name "TMP Advanced Neon"


            HLSLPROGRAM


            #pragma target 3.0


            #pragma vertex Vert

            #pragma fragment Frag


            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP


            // ========================================================
            // URP CORE
            // ========================================================

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            // ========================================================
            // PROPERTIES
            // ========================================================

            CBUFFER_START(UnityPerMaterial)

                float4 _FaceColor;

                float4 _GradientColorA;

                float4 _GradientColorB;

                float4 _GradientColorC;

                float _GradientDirection;

                float _GradientSpeed;

                float _GradientOffset;

                float4 _TextMin;

                float4 _TextMax;


                float4 _OutlineColor;

                float _OutlineWidth;

                float _OutlineSoftness;


                float4 _EmissionColor;

                float _EmissionIntensity;


                float4 _GlowColor;

                float _GlowStrength;

                float _GlowSize;

                float _GlowSoftness;


                float _GradientScale;

                float _TextureWidth;

                float _TextureHeight;

                float _FaceDilate;


                float _ScaleX;

                float _ScaleY;

                float _Sharpness;


                float4 _ClipRect;

                float _MaskSoftnessX;

                float _MaskSoftnessY;

            CBUFFER_END


            // ========================================================
            // TEXTURE
            // ========================================================

            TEXTURE2D(_MainTex);

            SAMPLER(sampler_MainTex);


            // ========================================================
            // TIME
            // ========================================================

           // float4 _Time;


            // ========================================================
            // VERTEX
            // ========================================================

            struct Attributes
            {
                float4 positionOS : POSITION;

                float3 normalOS : NORMAL;

                float4 color : COLOR;

                float4 texcoord0 : TEXCOORD0;

                float2 texcoord1 : TEXCOORD1;
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

                float4 mask : TEXCOORD2;

                float sdfScale : TEXCOORD3;
            };


            // ========================================================
            // WHOLE TEXT GRADIENT
            // ========================================================

            float GetGradientValue(
                float2 position
            )
            {
                float2 minPos =
                    _TextMin.xy;


                float2 maxPos =
                    _TextMax.xy;


                float2 size =
                    maxPos -
                    minPos;


                size =
                    max(
                        size,

                        float2(
                            0.0001,
                            0.0001
                        )
                    );


              float2 uv = (position - minPos) / max(size, float2(0.0001, 0.0001));


                uv = clamp(uv, 0.0, 1.0);


                // ----------------------------------------------------
                // HORIZONTAL
                // ----------------------------------------------------

                if (
                    _GradientDirection
                    < 0.5
                )
                {
                    return uv.x;
                }


                // ----------------------------------------------------
                // VERTICAL
                // ----------------------------------------------------

                if (
                    _GradientDirection
                    < 1.5
                )
                {
                    return uv.y;
                }


                // ----------------------------------------------------
                // DIAGONAL
                // ----------------------------------------------------

                return saturate(
                    (
                        uv.x +
                        uv.y
                    )
                    *
                    0.5
                );
            }


            // ========================================================
            // 3 COLOR GRADIENT
            // ========================================================

           float3 GetGradientColor(float t)
{
    t = saturate(t);

    float3 ab =
        lerp(
            _GradientColorA.rgb,
            _GradientColorB.rgb,
            smoothstep(0.0, 0.5, t)
        );

    float3 bc =
        lerp(
            _GradientColorB.rgb,
            _GradientColorC.rgb,
            smoothstep(0.5, 1.0, t)
        );

    return lerp(
        ab,
        bc,
        smoothstep(0.45, 0.55, t)
    );
}


            // ========================================================
            // VERTEX
            // ========================================================

            Varyings Vert(
                Attributes input
            )
            {
                Varyings output;


                // ----------------------------------------------------
                // POSITION
                // ----------------------------------------------------

                VertexPositionInputs pos =
                    GetVertexPositionInputs(
                        input.positionOS.xyz
                    );


                output.positionCS =
                    pos.positionCS;


                // ----------------------------------------------------
                // TMP COLOR
                // ----------------------------------------------------

                output.color =
                    input.color;


                // ----------------------------------------------------
                // FONT ATLAS UV
                //
                // TMP SDF font atlas is normally TEXCOORD0.
                // ----------------------------------------------------

                output.atlasUV =
                    input.texcoord0.xy;


                // ----------------------------------------------------
                // WHOLE TEXT POSITION
                //
                // This is NOT the atlas UV.
                //
                // This is the actual mesh position.
                // ----------------------------------------------------

                output.textPosition =
                    input.positionOS.xy;


                // ----------------------------------------------------
                // SCREEN PIXEL SCALE
                // ----------------------------------------------------

                float4 clipPosition =
                    output.positionCS;


                float2 pixelSize =
                    clipPosition.w;


                pixelSize /=
                    float2(
                        _ScaleX,
                        _ScaleY
                    )
                    *
                    abs(
                        mul(
                            (float2x2)
                            UNITY_MATRIX_P,

                            _ScreenParams.xy
                        )
                    );


                float scale =
                    rsqrt(
                        dot(
                            pixelSize,
                            pixelSize
                        )
                    );


                scale *=
                    abs(
                        input.texcoord0.w
                    )
                    *
                    _GradientScale
                    *
                    (
                        _Sharpness +
                        1
                    );


                output.sdfScale =
                    max(
                        scale,

                        0.0001
                    );


                // ----------------------------------------------------
                // UI CLIPPING
                // ----------------------------------------------------

                float2 clipPositionXY =
                    input.positionOS.xy;


                float2 maskPos =
                    clipPositionXY
                    -
                    _ClipRect.xy;


                float2 maskSize =
                    _ClipRect.zw
                    -
                    _ClipRect.xy;


                float2 normalizedMask =
                    maskPos /
                    max(
                        maskSize,

                        float2(
                            0.0001,
                            0.0001
                        )
                    );


                output.mask =
                    float4(
                        normalizedMask,

                        1,
                        1
                    );


                return output;
            }


            // ========================================================
            // FRAGMENT
            // ========================================================

            half4 Frag(
                Varyings input
            )
            :
            SV_Target
            {
                // ----------------------------------------------------
                // SDF SAMPLE
                // ----------------------------------------------------

                float sdf =
                    SAMPLE_TEXTURE2D(
                        _MainTex,

                        sampler_MainTex,

                        input.atlasUV
                    ).a;


                // ----------------------------------------------------
                // SDF SCALE
                // ----------------------------------------------------

                float scale =
                    input.sdfScale;


                // ----------------------------------------------------
                // FACE THRESHOLD
                // ----------------------------------------------------

                float faceThreshold =
                    0.5
                    -
                    _FaceDilate
                    *
                    0.5;


                // ----------------------------------------------------
                // FACE ALPHA
                // ----------------------------------------------------

                float faceAlpha =
                    smoothstep(
                        faceThreshold
                        -
                        0.02
                        /
                        scale,

                        faceThreshold
                        +
                        0.02
                        /
                        scale,

                        sdf
                    );


                // ----------------------------------------------------
                // OUTLINE
                // ----------------------------------------------------

                float outlineDistance =
                    _OutlineWidth
                    *
                    0.5;


                float outlineThreshold =
                    faceThreshold
                    -
                    outlineDistance;


                float outlineAlpha =
                    smoothstep(
                        outlineThreshold
                        -
                        _OutlineSoftness
                        /
                        scale,

                        outlineThreshold
                        +
                        _OutlineSoftness
                        /
                        scale,

                        sdf
                    );


                float outlineOnly =
                    saturate(
                        outlineAlpha
                        -
                        faceAlpha
                    );


                // ----------------------------------------------------
                // WHOLE TEXT GRADIENT POSITION
                // ----------------------------------------------------

                float gradientT =
                    GetGradientValue(
                        input.textPosition
                    );


                // ----------------------------------------------------
                // ANIMATION
                // ----------------------------------------------------

                gradientT +=
                    _GradientOffset;


                gradientT +=
                    _Time.y
                    *
                    _GradientSpeed;


                gradientT =
                    frac(
                        gradientT
                    );


                // ----------------------------------------------------
                // GRADIENT COLOR
                // ----------------------------------------------------

                float3 gradientColor =
                    GetGradientColor(
                        gradientT
                    );


                // ----------------------------------------------------
                // FACE COLOR
                // ----------------------------------------------------

                float3 faceRGB =
                    gradientColor
                    *
                    _FaceColor.rgb
                    *
                    input.color.rgb;


                // ----------------------------------------------------
                // OUTLINE COLOR
                // ----------------------------------------------------

                float3 outlineRGB =
                    _OutlineColor.rgb;


                // ----------------------------------------------------
                // COMBINE FACE + OUTLINE
                // ----------------------------------------------------

                float3 finalRGB =
                    lerp(
                        outlineRGB,

                        faceRGB,

                        faceAlpha
                    );


                float finalAlpha =
                    max(
                        faceAlpha,

                        outlineOnly
                    );


                // ----------------------------------------------------
                // NEON EMISSION
                //
                // Emission follows the animated gradient.
                // ----------------------------------------------------

                float3 emission =
                    gradientColor
                    *
                    _EmissionColor.rgb
                    *
                    _EmissionIntensity;


                finalRGB +=
                    emission
                    *
                    faceAlpha;


                // ----------------------------------------------------
                // GLOW
                //
                // Uses distance from SDF edge.
                // ----------------------------------------------------

                float glowDistance =
                    max(
                        0,

                        faceThreshold
                        -
                        sdf
                    );


                float glow =
                    1
                    -
                    smoothstep(
                        0,

                        max(
                            _GlowSize,

                            0.0001
                        ),

                        glowDistance
                    );


                glow *=
                    _GlowStrength;


                glow *=
                    _GlowColor.a;


                finalRGB +=
                    _GlowColor.rgb
                    *
                    glow;


                // ----------------------------------------------------
                // ALPHA GLOW
                // ----------------------------------------------------

                finalAlpha =
                    max(
                        finalAlpha,

                        saturate(
                            glow
                        )
                    );


                // ----------------------------------------------------
                // UI CLIPPING
                // ----------------------------------------------------

                #if defined(UNITY_UI_CLIP_RECT)

                float2 insideClip =
                    step(
                        _ClipRect.xy,

                        input.textPosition
                    )
                    *
                    step(
                        input.textPosition,

                        _ClipRect.zw
                    );


                finalAlpha *=
                    insideClip.x
                    *
                    insideClip.y;

                #endif


                // ----------------------------------------------------
                // ALPHA CLIP
                // ----------------------------------------------------

                #if defined(UNITY_UI_ALPHACLIP)

                clip(
                    finalAlpha
                    -
                    0.001
                );

                #endif


                // ----------------------------------------------------
                // PREMULTIPLIED ALPHA
                //
                // Blend is:
                // One OneMinusSrcAlpha
                // ----------------------------------------------------

                finalRGB *=
                    finalAlpha;


                return half4(
                    finalRGB,

                    finalAlpha
                );
            }


            ENDHLSL
        }
    }
}