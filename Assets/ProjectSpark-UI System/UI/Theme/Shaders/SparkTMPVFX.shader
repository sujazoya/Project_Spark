Shader "Project Spark/TMP/VFX"
{
    Properties
    {
        // ============================================================
        // TMP BASE
        // ============================================================

        _MainTex ("Font Atlas", 2D) = "white" {}

        _Color ("Text Color", Color) = (1,1,1,1)

        // ============================================================
        // VFX COLORS
        // ============================================================

        _VFXColor
        (
            "VFX Color",
            Color
        ) = (1,1,1,1)

        _VFXGlowColor
        (
            "Glow Color",
            Color
        ) = (1,1,1,1)

        _VFXScanColor
        (
            "Scan Color",
            Color
        ) = (1,1,1,1)

        _VFXSweepColor
        (
            "Sweep Color",
            Color
        ) = (1,1,1,1)

        // ============================================================
        // GLOW
        // ============================================================

        _VFXGlow
        (
            "Glow",
            Range(0,5)
        ) = 0

        // ============================================================
        // SCAN
        // ============================================================

        _VFXScan
        (
            "Scan",
            Range(0,5)
        ) = 0

        // ============================================================
        // SWEEP
        // ============================================================

        _VFXSweep
        (
            "Sweep",
            Range(0,5)
        ) = 0

        _VFXSweepPosition
        (
            "Sweep Position",
            Range(-1,2)
        ) = 0

        // ============================================================
        // FLASH
        // ============================================================

        _VFXFlash
        (
            "Flash",
            Range(0,5)
        ) = 0

        // ============================================================
        // GLITCH
        // ============================================================

        _VFXGlitch
        (
            "Glitch",
            Range(0,1)
        ) = 0

        // ============================================================
        // FLICKER
        // ============================================================

        _VFXFlicker
        (
            "Flicker",
            Range(0,1)
        ) = 0

        // ============================================================
        // REVEAL
        // ============================================================

        _VFXReveal
        (
            "Reveal",
            Range(0,1)
        ) = 1

        // ============================================================
        // DISSOLVE
        // ============================================================

        _VFXDissolve
        (
            "Dissolve",
            Range(0,1)
        ) = 0

        // ============================================================
        // EFFECT WIDTH
        // ============================================================

        _VFXScanWidth
        (
            "Scan Width",
            Range(0.001,1)
        ) = 0.15

        _VFXSweepWidth
        (
            "Sweep Width",
            Range(0.001,1)
        ) = 0.15

        _VFXDissolveSoftness
        (
            "Dissolve Softness",
            Range(0.001,1)
        ) = 0.15

        // ============================================================
        // GLITCH SETTINGS
        // ============================================================

        _VFXGlitchStrength
        (
            "Glitch Strength",
            Range(0,0.1)
        ) = 0.02

        _VFXGlitchSpeed
        (
            "Glitch Speed",
            Range(0,20)
        ) = 5

        // ============================================================
        // FLICKER SETTINGS
        // ============================================================

        _VFXFlickerSpeed
        (
            "Flicker Speed",
            Range(0,30)
        ) = 10
    }


    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }


        Cull Off

        ZWrite Off

        ZTest [unity_GUIZTestMode]

        Blend SrcAlpha OneMinusSrcAlpha


        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert

            #pragma fragment frag

            #pragma target 3.0


            #include "UnityCG.cginc"


            // ========================================================
            // PROPERTIES
            // ========================================================

            sampler2D _MainTex;

            float4 _Color;

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


            float _VFXScanWidth;

            float _VFXSweepWidth;

            float _VFXDissolveSoftness;


            float _VFXGlitchStrength;

            float _VFXGlitchSpeed;

            float _VFXFlickerSpeed;


            // ========================================================
            // VERTEX
            // ========================================================

            struct Attributes
            {
                float4 vertex : POSITION;

                float2 uv : TEXCOORD0;

                float4 color : COLOR;
            };


            // ========================================================
            // FRAGMENT DATA
            // ========================================================

            struct Varyings
            {
                float4 vertex : SV_POSITION;

                float2 uv : TEXCOORD0;

                float4 color : COLOR;
            };


            // ========================================================
            // SIMPLE HASH
            // ========================================================

            float SparkHash(
                float2 value
            )
            {
                float result =
                    sin(
                        dot(
                            value,
                            float2(
                                12.9898,
                                78.233
                            )
                        )
                    )
                    *
                    43758.5453;

                return frac(
                    result
                );
            }


            // ========================================================
            // VERTEX
            // ========================================================

            Varyings vert(
                Attributes input
            )
            {
                Varyings output;


                float2 uv =
                    input.uv;


                // ====================================================
                // GLITCH
                // ====================================================

                if (
                    _VFXGlitch >
                    0.001
                )
                {
                    float glitchTime =
                        _Time.y *
                        _VFXGlitchSpeed;


                    float noise =
                        SparkHash(
                            float2(
                                floor(
                                    glitchTime
                                ),
                                floor(
                                    uv.y *
                                    32.0
                                )
                            )
                        );


                    float glitchOffset =
                        (
                            noise -
                            0.5
                        )
                        *
                        _VFXGlitchStrength
                        *
                        _VFXGlitch;


                    uv.x +=
                        glitchOffset;
                }


                output.vertex =
                    UnityObjectToClipPos(
                        input.vertex
                    );


                output.uv =
                    uv;


                output.color =
                    input.color *
                    _Color;


                return output;
            }


            // ========================================================
            // FRAGMENT
            // ========================================================

            float4 frag(
                Varyings input
            ) : SV_Target
            {
                // ====================================================
                // FONT SAMPLE
                // ====================================================

                float4 font =
                    tex2D(
                        _MainTex,
                        input.uv
                    );


                float baseAlpha =
                    font.a *
                    input.color.a;


                // ====================================================
                // BASE COLOR
                // ====================================================

                float3 finalColor =
                    input.color.rgb;


                // ====================================================
                // VFX COLOR
                // ====================================================

                finalColor =
                    lerp(
                        finalColor,
                        _VFXColor.rgb,
                        saturate(
                            _VFXGlow *
                            0.25
                        )
                    );


                // ====================================================
                // GLOW
                // ====================================================

                if (
                    _VFXGlow >
                    0.001
                )
                {
                    float3 glowColor =
                        _VFXGlowColor.rgb *
                        _VFXGlow;


                    finalColor +=
                        glowColor;
                }


                // ====================================================
                // SWEEP
                //
                // UV X IS USED AS THE FULL TEXT EFFECT SPACE.
                //
                // The sweep moves from left to right across the
                // complete rendered text region.
                // ====================================================

                float sweepDistance =
                    abs(
                        input.uv.x -
                        _VFXSweepPosition
                    );


                float sweepMask =
                    1.0 -
                    smoothstep(
                        0.0,
                        _VFXSweepWidth,
                        sweepDistance
                    );


                sweepMask *=
                    saturate(
                        _VFXSweep
                    );


                finalColor =
                    lerp(
                        finalColor,
                        _VFXSweepColor.rgb,
                        sweepMask
                    );


                // ====================================================
                // SCAN
                //
                // Horizontal scan line across the entire text.
                // ====================================================

                float scanCenter =
                    frac(
                        _Time.y *
                        0.5
                    );


                float scanDistance =
                    abs(
                        input.uv.y -
                        scanCenter
                    );


                float scanMask =
                    1.0 -
                    smoothstep(
                        0.0,
                        _VFXScanWidth,
                        scanDistance
                    );


                scanMask *=
                    saturate(
                        _VFXScan
                    );


                finalColor =
                    lerp(
                        finalColor,
                        _VFXScanColor.rgb,
                        scanMask
                    );


                // ====================================================
                // FLASH
                // ====================================================

                if (
                    _VFXFlash >
                    0.001
                )
                {
                    finalColor +=
                        _VFXColor.rgb *
                        _VFXFlash;
                }


                // ====================================================
                // FLICKER
                // ====================================================

                float flickerNoise =
                    SparkHash(
                        float2(
                            floor(
                                _Time.y *
                                _VFXFlickerSpeed
                            ),
                            0.0
                        )
                    );


                float flickerValue =
                    lerp(
                        1.0,
                        flickerNoise,
                        saturate(
                            _VFXFlicker
                        )
                    );


                finalColor *=
                    flickerValue;


                // ====================================================
                // REVEAL
                //
                // LEFT -> RIGHT
                // ====================================================

                float revealMask =
                    step(
                        input.uv.x,
                        _VFXReveal
                    );


                // ====================================================
                // DISSOLVE
                //
                // Uses a stable pseudo-random value generated from
                // UV coordinates.
                // ====================================================

                float dissolveNoise =
                    SparkHash(
                        floor(
                            input.uv *
                            128.0
                        )
                    );


                float dissolveThreshold =
                    _VFXDissolve;


                float dissolveMask =
                    smoothstep(
                        dissolveThreshold -
                        _VFXDissolveSoftness,

                        dissolveThreshold +
                        _VFXDissolveSoftness,

                        dissolveNoise
                    );


                // ====================================================
                // FINAL ALPHA
                // ====================================================

                float finalAlpha =
                    baseAlpha;


                finalAlpha *=
                    revealMask;


                finalAlpha *=
                    dissolveMask;


                // ====================================================
                // OUTPUT
                // ====================================================

                return float4(
                    finalColor,
                    finalAlpha
                );
            }


            ENDHLSL
        }
    }


    FallBack Off
}