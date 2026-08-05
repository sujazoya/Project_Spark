Shader "Custom/TMP Advanced Neon URP"
{
    Properties
    {
        // =========================================================
        // TMP FONT ATLAS
        // =========================================================

        [PerRendererData]
        _MainTex ("Font Atlas", 2D) = "white" {}

        // =========================================================
        // FACE
        // =========================================================

        [HDR]
        _FaceColor ("Face Color", Color) = (1,1,1,1)

        // =========================================================
        // 3 COLOR GRADIENT
        // =========================================================

        [HDR]
        _GradientColorA ("Gradient Color A", Color) = (0,1,1,1)

        [HDR]
        _GradientColorB ("Gradient Color B", Color) = (0,0.2,1,1)

        [HDR]
        _GradientColorC ("Gradient Color C", Color) = (1,0,1,1)

        [Enum(Vertical,0, Horizontal,1,Diagonal,2)]
        _GradientDirection ("Gradient Direction", Float) = 0

        _GradientSpeed ("Gradient Speed", Float) = 1

        _GradientOffset ("Gradient Offset", Range(0,1)) = 0

        // =========================================================
        // OUTLINE
        // =========================================================

        [HDR]
        _OutlineColor ("Outline Color", Color) = (0,1,1,1)

        _OutlineWidth ("Outline Width", Range(0,0.5)) = 0.05

        // =========================================================
        // EMISSION
        // =========================================================

        [HDR]
        _EmissionColor ("Emission Color", Color) = (0,1,1,1)

        _EmissionIntensity ("Emission Intensity", Float) = 2

        // =========================================================
        // GLOW
        // =========================================================

        [HDR]
        _GlowColor ("Glow Color", Color) = (0,1,1,1)

        _GlowStrength ("Glow Strength", Float) = 1

        _GlowWidth ("Glow Width", Range(0,0.5)) = 0.1

        // =========================================================
        // TMP SDF SETTINGS
        // =========================================================

        _FaceDilate ("Face Dilate", Range(-1,1)) = 0

        _Softness ("Text Softness", Range(0,1)) = 0.05

        // =========================================================
        // TMP SCALE
        // =========================================================

        _ScaleRatioA ("Scale Ratio A", Float) = 1

        _ScaleRatioB ("Scale Ratio B", Float) = 1

        _ScaleRatioC ("Scale Ratio C", Float) = 1

        // =========================================================
        // CLIPPING
        // =========================================================

        _ClipRect ("Clip Rect", Vector) = (-32767,-32767,32767,32767)

        _MaskSoftnessX ("Mask Softness X", Float) = 0

        _MaskSoftnessY ("Mask Softness Y", Float) = 0

        _VertexOffsetX ("Vertex Offset X", Float) = 0

        _VertexOffsetY ("Vertex Offset Y", Float) = 0

        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.001
         _TextMin ("Text Min", Vector) = (0,0,0,0)

         _TextMax ("Text Max", Vector) = (1,1,0,0)
    }
    


    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }


        // =========================================================
        // TRANSPARENCY
        // =========================================================

        Blend SrcAlpha OneMinusSrcAlpha

        ZWrite Off

        Cull Off

        ZTest [unity_GUIZTestMode]


        Pass
        {
            Name "TMP Advanced Neon"

            HLSLPROGRAM

            #pragma vertex Vert

            #pragma fragment Frag

            #pragma target 3.0


            // =====================================================
            // URP CORE
            // =====================================================

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            // =====================================================
            // STRUCTURES
            // =====================================================

            struct Attributes
            {
                float4 positionOS : POSITION;

                float3 normalOS : NORMAL;

                float4 tangentOS : TANGENT;

                float2 uv : TEXCOORD0;

                float2 uv2 : TEXCOORD1;

                float4 color : COLOR;
            };


           struct Varyings
        {
            float4 positionHCS : SV_POSITION;

            float2 uv : TEXCOORD0;

            float2 uv2 : TEXCOORD1;

            float4 color : COLOR;

            float4 screenPosition : TEXCOORD2;

            float3 positionOS : TEXCOORD3;
        };

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

                float _GlowWidth;

                float _FaceDilate;

                float _Softness;

                float _ScaleRatioA;

                float _ScaleRatioB;

                float _ScaleRatioC;

                float4 _ClipRect;

                float _MaskSoftnessX;

                float _MaskSoftnessY;

                float _VertexOffsetX;

                float _VertexOffsetY;

                float _Cutoff;

            CBUFFER_END


            // =====================================================
            // VERTEX
            // =====================================================

            Varyings Vert(
                Attributes IN
            )
            {
                Varyings OUT;


                // -------------------------------------------------
                // Object position
                // -------------------------------------------------

                float3 positionOS =
                    IN.positionOS.xyz;


                // -------------------------------------------------
                // Unity transform
                // -------------------------------------------------

                OUT.positionHCS =
                    TransformObjectToHClip(
                        positionOS
                    );

                    OUT.positionOS =
                    positionOS;


                // -------------------------------------------------
                // TMP Atlas UV
                // -------------------------------------------------

                OUT.uv =
                    TRANSFORM_TEX(
                        IN.uv,
                        _MainTex
                    );


                // -------------------------------------------------
                // Secondary UV
                // -------------------------------------------------

                OUT.uv2 =
                    IN.uv2;


                // -------------------------------------------------
                // TMP Vertex Color
                // -------------------------------------------------

                OUT.color =
                    IN.color;


                // -------------------------------------------------
                // Screen Position
                // -------------------------------------------------

                OUT.screenPosition =
                    ComputeScreenPos(
                        OUT.positionHCS
                    );


                return OUT;
            }


            // =====================================================
            // 3 COLOR GRADIENT
            // =====================================================

            float3 ThreeColorGradient(
                float t
            )
            {
                // -------------------------------------------------
                // Loop gradient
                // -------------------------------------------------

                t =
                    frac(t);


                // -------------------------------------------------
                // First half
                // A → B
                // -------------------------------------------------

                if (t < 0.5)
                {
                    float localT =
                        t * 2.0;


                    return lerp(
                        _GradientColorA.rgb,

                        _GradientColorB.rgb,

                        localT
                    );
                }


                // -------------------------------------------------
                // Second half
                // B → C
                // -------------------------------------------------

                float localT2 =
                    (t - 0.5) * 2.0;


                return lerp(
                    _GradientColorB.rgb,

                    _GradientColorC.rgb,

                    localT2
                );
            }
           

            // =====================================================
            // GRADIENT POSITION
            // =====================================================

            float GradientPosition(
                float2 uv
            )
            {
                float position =
                    0.0;


                // -------------------------------------------------
                // HORIZONTAL
                // -------------------------------------------------

                if (
                    _GradientDirection < 0.5
                )
                {
                    position =
                        uv.x;
                }


                // -------------------------------------------------
                // VERTICAL
                // -------------------------------------------------

                else if (
                    _GradientDirection < 1.5
                )
                {
                    position =
                        uv.y;
                }


                // -------------------------------------------------
                // DIAGONAL
                // -------------------------------------------------

                else
                {
                    position =
                        (uv.x + uv.y)
                        *
                        0.5;
                }


                // -------------------------------------------------
                // ALWAYS ANIMATED
                // -------------------------------------------------

                position +=
                    _Time.y
                    *
                    _GradientSpeed;


                // -------------------------------------------------
                // MANUAL OFFSET
                // -------------------------------------------------

                position +=
                    _GradientOffset;


                // -------------------------------------------------
                // LOOP
                // -------------------------------------------------

                return frac(
                    position
                );
            }


            // =====================================================
            // SDF FACE
            // =====================================================

            float FaceMask(
                float sdf
            )
            {
                // -------------------------------------------------
                // TMP SDF center
                // -------------------------------------------------

                float threshold =
                    0.5;


                // -------------------------------------------------
                // Face dilate
                // -------------------------------------------------

                threshold -=
                    _FaceDilate
                    *
                    0.1;


                // -------------------------------------------------
                // Anti-aliasing
                // -------------------------------------------------

                float aa =
                    max(
                        fwidth(sdf),
                        0.0001
                    );


                // -------------------------------------------------
                // Softness
                // -------------------------------------------------

                aa +=
                    _Softness
                    *
                    0.05;


                return smoothstep(
                    threshold - aa,

                    threshold + aa,

                    sdf
                );
            }


            // =====================================================
            // OUTLINE MASK
            // =====================================================

            float OutlineMask(
                float sdf
            )
            {
                // -------------------------------------------------
                // Outline expands outward
                // -------------------------------------------------

                float threshold =
                    0.5
                    -
                    _OutlineWidth;


                float aa =
                    max(
                        fwidth(sdf),
                        0.0001
                    );


                return smoothstep(
                    threshold - aa,

                    threshold + aa,

                    sdf
                );
            }


            // =====================================================
            // GLOW MASK
            // =====================================================

            float GlowMask(
                float sdf
            )
            {
                // -------------------------------------------------
                // Outline threshold
                // -------------------------------------------------

                float outlineThreshold =
                    0.5
                    -
                    _OutlineWidth;


                // -------------------------------------------------
                // Glow extends further outward
                // -------------------------------------------------

                float glowThreshold =
                    outlineThreshold
                    -
                    _GlowWidth;


                float aa =
                    max(
                        fwidth(sdf),
                        0.0001
                    );


                float glow =
                    smoothstep(
                        glowThreshold - aa,

                        glowThreshold + aa,

                        sdf
                    );


                float outline =
                    smoothstep(
                        outlineThreshold - aa,

                        outlineThreshold + aa,

                        sdf
                    );


                // -------------------------------------------------
                // Remove outline from glow
                // -------------------------------------------------

                glow -=
                    outline;


                return saturate(
                    glow
                );
            }


            // =====================================================
            // FRAGMENT
            // =====================================================

            half4 Frag(
                Varyings IN
            )
                : SV_Target
            {
                // =================================================
                // SAMPLE TMP SDF
                // =================================================

                float sdf =
                    SAMPLE_TEXTURE2D(
                        _MainTex,

                        sampler_MainTex,

                        IN.uv
                    ).a;


                // =================================================
                // FACE MASK
                // =================================================

                float face =
                    FaceMask(
                        sdf
                    );


                // =================================================
                // OUTLINE
                // =================================================

                float outline =
                    OutlineMask(
                        sdf
                    );


                // -------------------------------------------------
                // Only outside face
                // -------------------------------------------------

                float pureOutline =
                    saturate(
                        outline
                        -
                        face
                    );


                // =================================================
                // GLOW
                // =================================================

                float glow =
                    GlowMask(
                        sdf
                    );


                // =================================================
                // GRADIENT POSITION
                // =================================================

                float gradientT =
                    GradientPosition(
                        IN.uv
                    );


                // =================================================
                // 3 COLOR GRADIENT
                // =================================================

                float3 gradient =
                    ThreeColorGradient(
                        gradientT
                    );


                // =================================================
                // FACE COLOR
                // =================================================

                float3 faceRGB =
                    gradient
                    *
                    _FaceColor.rgb;


                // =================================================
                // FACE RESULT
                // =================================================

                float3 faceResult =
                    faceRGB
                    *
                    face;


                // =================================================
                // OUTLINE RESULT
                // =================================================

                float3 outlineResult =
                    _OutlineColor.rgb
                    *
                    pureOutline;


                // =================================================
                // GLOW RESULT
                // =================================================

                float3 glowResult =
                    _GlowColor.rgb
                    *
                    _GlowStrength
                    *
                    glow;


                // =================================================
                // BASE
                // =================================================

                float3 baseColor =
                    faceResult
                    +
                    outlineResult
                    +
                    glowResult;


                // =================================================
                // FACE EMISSION
                // =================================================

                float3 faceEmission =
                    faceRGB
                    *
                    _EmissionColor.rgb
                    *
                    _EmissionIntensity
                    *
                    face;


                // =================================================
                // OUTLINE EMISSION
                // =================================================

                float3 outlineEmission =
                    _OutlineColor.rgb
                    *
                    _EmissionColor.rgb
                    *
                    _EmissionIntensity
                    *
                    pureOutline;


                // =================================================
                // TOTAL EMISSION
                // =================================================

                float3 totalEmission =
                    faceEmission
                    +
                    outlineEmission;


                // =================================================
                // FINAL RGB
                // =================================================

                float3 finalRGB =
                    baseColor
                    +
                    totalEmission;


                // =================================================
                // ALPHA
                // =================================================

                float alpha =
                    saturate(
                        face
                        +
                        pureOutline
                        +
                        glow
                    );


                // =================================================
                // TMP VERTEX ALPHA
                // =================================================

                alpha *=
                    IN.color.a;


                // =================================================
                // TMP VERTEX COLOR
                // =================================================

                finalRGB *=
                    IN.color.rgb;


                // =================================================
                // ALPHA CUTOUT
                // =================================================

                clip(
                    alpha
                    -
                    _Cutoff
                );


                // =================================================
                // RETURN
                // =================================================

                return half4(
                    finalRGB,

                    alpha
                );
            }

            ENDHLSL
        }
    }


    // =============================================================
    // FALLBACK
    // =============================================================

    FallBack Off
}