Shader "Project Spark/TMP/Spark VFX"
{
    Properties
    {
        // ============================================================
        // TMP FONT ATLAS
        // ============================================================

        [MainTexture]
        _MainTex ("Font Atlas", 2D) = "white" {}

        _FaceColor ("Face Color", Color) = (1,1,1,1)


        // ============================================================
        // PROJECT SPARK COLORS
        // ============================================================

        _VFXColor ("VFX Color", Color) = (1,1,1,1)

        _VFXGlowColor ("Glow Color", Color) = (1,1,1,1)

        _VFXScanColor ("Scan Color", Color) = (1,1,1,1)

        _VFXSweepColor ("Sweep Color", Color) = (1,1,1,1)


        // ============================================================
        // VFX VALUES
        // ============================================================

        _VFXGlow ("Glow", Range(0,5)) = 0

        _VFXScan ("Scan", Range(0,5)) = 0

        _VFXSweep ("Sweep", Range(0,5)) = 0

        _VFXSweepPosition ("Sweep Position", Range(-1,2)) = 0

        _VFXFlash ("Flash", Range(0,5)) = 0

        _VFXGlitch ("Glitch", Range(0,1)) = 0

        _VFXFlicker ("Flicker", Range(0,1)) = 0

        _VFXReveal ("Reveal", Range(0,1)) = 1

        _VFXDissolve ("Dissolve", Range(0,1)) = 0


        // ============================================================
        // ANIMATION SPEED
        // ============================================================

        _VFXScanSpeed ("Scan Speed", Float) = 1

        _VFXGlitchSpeed ("Glitch Speed", Float) = 10

        _VFXFlickerSpeed ("Flicker Speed", Float) = 8


        // ============================================================
        // TMP STYLE
        // ============================================================

        _FaceDilate ("Face Dilate", Range(-1,1)) = 0

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)

        _OutlineWidth ("Outline Width", Range(0,1)) = 0
    }


    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }


        Cull Off

        ZWrite Off

        Blend SrcAlpha OneMinusSrcAlpha


        Pass
        {
            Name "Spark TMP VFX"


            HLSLPROGRAM


            #pragma vertex vert

            #pragma fragment frag

            #pragma target 3.0


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            // ========================================================
            // VERTEX INPUT
            // ========================================================

            struct Attributes
            {
                float4 positionOS : POSITION;

                float4 color : COLOR;

                float2 uv : TEXCOORD0;
            };


            // ========================================================
            // VERTEX OUTPUT
            // ========================================================

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;

                float4 color : COLOR;

                float2 uv : TEXCOORD0;

                float2 positionText : TEXCOORD1;
            };


            // ========================================================
            // FONT ATLAS
            // ========================================================

            TEXTURE2D(_MainTex);

            SAMPLER(sampler_MainTex);


            // ========================================================
            // MATERIAL VALUES
            // ========================================================

            CBUFFER_START(UnityPerMaterial)

                float4 _FaceColor;

                float4 _VFXColor;

                float4 _VFXGlowColor;

                float4 _VFXScanColor;

                float4 _VFXSweepColor;


                float _VFXGlow;

                float _VFXScan;

                float _VFXSweep;

                float _VFXSweepPosition;

                float _VFXFlash;

                float _VFXGlitch;

                float _VFXFlicker;

                float _VFXReveal;

                float _VFXDissolve;


                float _VFXScanSpeed;

                float _VFXGlitchSpeed;

                float _VFXFlickerSpeed;


                float _FaceDilate;

                float4 _OutlineColor;

                float _OutlineWidth;

            CBUFFER_END


            // ========================================================
            // VERTEX
            // ========================================================

            Varyings vert(
                Attributes input
            )
            {
                Varyings output;


                // ----------------------------------------------------
                // POSITION
                // ----------------------------------------------------

                output.positionHCS =
                    TransformObjectToHClip(
                        input.positionOS.xyz
                    );


                // ----------------------------------------------------
                // TMP COLOR
                // ----------------------------------------------------

                output.color =
                    input.color *
                    _FaceColor;


                // ----------------------------------------------------
                // FONT ATLAS UV
                //
                // Used ONLY for SDF sampling.
                // ----------------------------------------------------

                output.uv =
                    input.uv;


                // ----------------------------------------------------
                // TEXT POSITION
                //
                // Used for VFX.
                //
                // This is deliberately separate from font atlas UV.
                // ----------------------------------------------------

                output.positionText =
                    input.positionOS.xy;


                return output;
            }


            // ========================================================
            // FRAGMENT
            // ========================================================

            half4 frag(
                Varyings input
            ) : SV_Target
            {
                // ====================================================
                // TMP FONT SAMPLE
                // ====================================================

                float sdf =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv
                    ).a;


                // ====================================================
                // SDF WIDTH
                // ====================================================

                float sdfWidth =
                    fwidth(
                        sdf
                    );


                sdfWidth =
                    max(
                        sdfWidth,
                        0.001
                    );


                // ====================================================
                // FACE
                // ====================================================

                float faceThreshold =
                    0.5;


                faceThreshold +=
                    _FaceDilate *
                    0.25;


                // ====================================================
                // MAIN TEXT ALPHA
                // ====================================================

                float alpha =
                    smoothstep(
                        faceThreshold -
                        sdfWidth,

                        faceThreshold +
                        sdfWidth,

                        sdf
                    );


                // ====================================================
                // TEXT COORDINATE
                //
                // The TMP mesh position is used for effects.
                // ====================================================

                float2 textPosition =
                    input.positionText;


                // ====================================================
                // STABLE LOCAL COORDINATE
                //
                // This keeps effects stable and avoids atlas UV.
                // ====================================================

                float textX =
                    frac(
                        textPosition.x *
                        0.01
                    );


                float textY =
                    frac(
                        textPosition.y *
                        0.01
                    );


                // ====================================================
                // BASE COLOR
                // ====================================================

                float3 finalColor =
                    input.color.rgb;


                // ====================================================
                // GLOW
                // ====================================================

                finalColor +=
                    _VFXGlowColor.rgb *
                    _VFXGlow *
                    0.5;


                // ====================================================
                // SCAN
                // ====================================================

                float scanTime =
                    frac(
                        _Time.y *
                        _VFXScanSpeed
                    );


                float scanDistance =
                    abs(
                        textY -
                        scanTime
                    );


                float scanMask =
                    1.0 -
                    smoothstep(
                        0.0,
                        0.15,
                        scanDistance
                    );


                finalColor +=
                    _VFXScanColor.rgb *
                    scanMask *
                    _VFXScan;


                // ====================================================
                // SWEEP
                // ====================================================

                float sweepDistance =
                    abs(
                        textX -
                        frac(
                            _VFXSweepPosition
                        )
                    );


                float sweepMask =
                    1.0 -
                    smoothstep(
                        0.0,
                        0.12,
                        sweepDistance
                    );


                finalColor +=
                    _VFXSweepColor.rgb *
                    sweepMask *
                    _VFXSweep;


                // ====================================================
                // FLASH
                // ====================================================

                finalColor +=
                    _VFXColor.rgb *
                    _VFXFlash;


                // ====================================================
                // FLICKER
                //
                // Simple deterministic animation.
                // ====================================================

                float flickerWave =
                    sin(
                        _Time.y *
                        _VFXFlickerSpeed *
                        6.2831853
                    );


                float flickerValue =
                    lerp(
                        1.0,
                        0.65 +
                        flickerWave *
                        0.35,
                        _VFXFlicker
                    );


                finalColor *=
                    flickerValue;


                // ====================================================
                // REVEAL
                //
                // Whole text-space effect.
                // ====================================================

                float revealMask =
                    step(
                        textX,
                        _VFXReveal
                    );


                alpha *=
                    revealMask;


                // ====================================================
                // DISSOLVE
                //
                // Stable procedural pattern.
                // ====================================================

                float dissolvePattern =
                    sin(
                        textPosition.x *
                        12.37
                    )
                    *
                    sin(
                        textPosition.y *
                        17.13
                    );


                dissolvePattern =
                    dissolvePattern *
                    0.5 +
                    0.5;


                float dissolveMask =
                    step(
                        _VFXDissolve,
                        dissolvePattern
                    );


                alpha *=
                    dissolveMask;


                // ====================================================
                // GLITCH
                // ====================================================

                float glitchWave =
                    sin(
                        (
                            textPosition.y *
                            4.0
                        )
                        +
                        (
                            _Time.y *
                            _VFXGlitchSpeed
                        )
                    );


                float glitchMask =
                    step(
                        0.5,
                        glitchWave *
                        0.5 +
                        0.5
                    );


                finalColor =
                    lerp(
                        finalColor,

                        _VFXColor.rgb,

                        glitchMask *
                        _VFXGlitch
                    );


                // ====================================================
                // OUTLINE
                // ====================================================

                float outlineThreshold =
                    faceThreshold -
                    _OutlineWidth;


                float outlineAlpha =
                    smoothstep(
                        outlineThreshold -
                        sdfWidth,

                        outlineThreshold +
                        sdfWidth,

                        sdf
                    );


                float outlineOnly =
                    saturate(
                        outlineAlpha -
                        alpha
                    );


                // ====================================================
                // COMBINE FACE + OUTLINE
                // ====================================================

                float3 colorWithOutline =
                    lerp(
                        _OutlineColor.rgb,

                        finalColor,

                        alpha
                    );


                float finalAlpha =
                    max(
                        alpha,

                        outlineOnly *
                        _OutlineColor.a
                    );


                // ====================================================
                // FINAL OUTPUT
                // ====================================================

                return half4(
                    colorWithOutline,

                    finalAlpha
                );
            }


            ENDHLSL
        }
    }


    FallBack Off
}