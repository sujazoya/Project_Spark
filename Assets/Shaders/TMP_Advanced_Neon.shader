Shader "Custom/TMP Advanced Neon URP"
{
    Properties
    {
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

        // 0 = Horizontal
        // 1 = Vertical
        // 2 = Diagonal

        [Enum(Horizontal,0,Vertical,1,Diagonal,2)]
        _GradientDirection ("Gradient Direction", Float) = 0

        _GradientSpeed ("Gradient Animation Speed", Float) = 1

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
        // SDF
        // =========================================================

        _FaceDilate ("Face Dilate", Range(-1,1)) = 0

        _Softness ("Text Softness", Range(0,1)) = 0.05

        // =========================================================
        // ALPHA
        // =========================================================

        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.001
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha

        ZWrite Off

        Cull Off

        Pass
        {
            Name "TMP Advanced Neon"

            HLSLPROGRAM

            #pragma vertex vert

            #pragma fragment frag

            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // =====================================================
            // STRUCTURES
            // =====================================================

            struct Attributes
            {
                float4 positionOS : POSITION;

                float2 uv : TEXCOORD0;

                float4 color : COLOR;
            };


            struct Varyings
            {
                float4 positionHCS : SV_POSITION;

                float2 uv : TEXCOORD0;

                float4 color : COLOR;
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

                float _Cutoff;

            CBUFFER_END


            // =====================================================
            // VERTEX
            // =====================================================

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS =
                    TransformObjectToHClip(
                        IN.positionOS.xyz
                    );

                OUT.uv =
                    TRANSFORM_TEX(
                        IN.uv,
                        _MainTex
                    );

                OUT.color =
                    IN.color;

                return OUT;
            }


            // =====================================================
            // 3 COLOR GRADIENT
            // =====================================================

            float3 GetThreeColorGradient(float t)
            {
                t = frac(t);

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

            float GetGradientPosition(float2 uv)
            {
                float position = 0.0;

                // Horizontal
                if (_GradientDirection < 0.5)
                {
                    position = uv.x;
                }

                // Vertical
                else if (_GradientDirection < 1.5)
                {
                    position = uv.y;
                }

                // Diagonal
                else
                {
                    position =
                        (uv.x + uv.y) * 0.5;
                }

                // Continuous animation
                position +=
                    _Time.y *
                    _GradientSpeed;

                // User offset
                position +=
                    _GradientOffset;

                return frac(position);
            }


            // =====================================================
            // FRAGMENT
            // =====================================================

            half4 frag(Varyings IN) : SV_Target
            {
                // =================================================
                // SAMPLE SDF
                // =================================================

                float sdf =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        IN.uv
                    ).a;


                // =================================================
                // SCREEN SPACE SDF AA
                // =================================================

                float sdfAA =
                    max(
                        fwidth(sdf),
                        0.0001
                    );


                // =================================================
                // FACE
                // =================================================

                float faceThreshold =
                    0.5 -
                    (_FaceDilate * 0.1);


                float faceSoftness =
                    sdfAA +
                    (_Softness * 0.05);


                float face =
                    smoothstep(
                        faceThreshold -
                        faceSoftness,

                        faceThreshold +
                        faceSoftness,

                        sdf
                    );


                // =================================================
                // OUTLINE
                // =================================================

                float outlineThreshold =
                    0.5 -
                    _OutlineWidth;


                float outline =
                    smoothstep(
                        outlineThreshold -
                        sdfAA,

                        outlineThreshold +
                        sdfAA,

                        sdf
                    );


                // Only outline outside face

                float pureOutline =
                    saturate(
                        outline -
                        face
                    );


                // =================================================
                // GLOW
                // =================================================

                float glowThreshold =
                    outlineThreshold -
                    _GlowWidth;


                float glow =
                    smoothstep(
                        glowThreshold -
                        sdfAA,

                        glowThreshold +
                        sdfAA,

                        sdf
                    );


                // Remove outline and face

                glow =
                    saturate(
                        glow -
                        outline
                    );


                // =================================================
                // GRADIENT
                // =================================================

                float gradientPosition =
                    GetGradientPosition(
                        IN.uv
                    );


                float3 gradient =
                    GetThreeColorGradient(
                        gradientPosition
                    );


                // =================================================
                // FACE COLOR
                // =================================================

                float3 faceRGB =
                    gradient *
                    _FaceColor.rgb;


                // =================================================
                // OUTLINE
                // =================================================

                float3 outlineRGB =
                    _OutlineColor.rgb;


                // =================================================
                // BASE COLOR
                // =================================================

                float3 baseColor =
                    (
                        faceRGB *
                        face
                    )
                    +
                    (
                        outlineRGB *
                        pureOutline
                    );


                // =================================================
                // EMISSION
                // =================================================

                float3 faceEmission =
                    faceRGB *
                    _EmissionColor.rgb *
                    _EmissionIntensity *
                    face;


                float3 outlineEmission =
                    outlineRGB *
                    _EmissionColor.rgb *
                    _EmissionIntensity *
                    pureOutline;


                // =================================================
                // GLOW
                // =================================================

                float3 glowRGB =
                    _GlowColor.rgb *
                    _GlowStrength *
                    glow;


                // =================================================
                // FINAL
                // =================================================

                float3 finalRGB =
                    baseColor
                    +
                    faceEmission
                    +
                    outlineEmission
                    +
                    glowRGB;


                // =================================================
                // ALPHA
                // =================================================

                float alpha =
                    saturate(
                        face +
                        pureOutline +
                        glow
                    );


                // TMP vertex alpha

                alpha *=
                    IN.color.a;


                // =================================================
                // CUTOUT
                // =================================================

                clip(
                    alpha -
                    _Cutoff
                );


                return half4(
                    finalRGB,
                    alpha
                );
            }

            ENDHLSL
        }
    }

    FallBack Off
}