Shader "Project Spark/UI/VFX"
{
    Properties
    {
        [PerRendererData]
        _MainTex ("Sprite Texture", 2D) = "white" {}

        _Color ("Base Color", Color) = (1,1,1,1)

        _VFXColor ("VFX Color", Color) = (1,1,1,1)
        _VFXGlowColor ("Glow Color", Color) = (1,1,1,1)
        _VFXScanColor ("Scan Color", Color) = (1,1,1,1)
        _VFXSweepColor ("Sweep Color", Color) = (1,1,1,1)

        _VFXGlow ("Glow", Range(0,5)) = 0
        _VFXScan ("Scan", Range(0,5)) = 0
        _VFXSweep ("Sweep", Range(0,5)) = 0
        _VFXFlash ("Flash", Range(0,5)) = 0

        _VFXGlitch ("Glitch", Range(0,1)) = 0
        _VFXFlicker ("Flicker", Range(0,1)) = 0
        _VFXDissolve ("Dissolve", Range(0,1)) = 0
        _VFXReveal ("Reveal", Range(0,1)) = 0

        _VFXSweepPosition ("Sweep Position", Range(-1,2)) = 0

        _VFXScanSpeed ("Scan Speed", Float) = 1
        _VFXGlitchSpeed ("Glitch Speed", Float) = 10
        _VFXFlickerSpeed ("Flicker Speed", Float) = 8

        _GlowWidth ("Glow Width", Range(0,1)) = 0.25
        _ScanWidth ("Scan Width", Range(0.001,1)) = 0.05
        _SweepWidth ("Sweep Width", Range(0.01,1)) = 0.2

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)]
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Blend SrcAlpha OneMinusSrcAlpha

        ColorMask [_ColorMask]

        Pass
        {
            Name "Project Spark UI VFX"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #pragma target 3.0

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)

                float4 _Color;

                float4 _VFXColor;
                float4 _VFXGlowColor;
                float4 _VFXScanColor;
                float4 _VFXSweepColor;

                float _VFXGlow;
                float _VFXScan;
                float _VFXSweep;
                float _VFXFlash;

                float _VFXGlitch;
                float _VFXFlicker;
                float _VFXDissolve;
                float _VFXReveal;

                float _VFXSweepPosition;

                float _VFXScanSpeed;
                float _VFXGlitchSpeed;
                float _VFXFlickerSpeed;

                float _GlowWidth;
                float _ScanWidth;
                float _SweepWidth;

            CBUFFER_END


            // --------------------------------------------------
            // SIMPLE HASH
            // --------------------------------------------------

            float SparkHash(float2 p)
            {
                p = frac(
                    p *
                    float2(123.34, 456.21)
                );

                p += dot(
                    p,
                    p + 45.32
                );

                return frac(
                    p.x * p.y
                );
            }


            // --------------------------------------------------
            // VERTEX
            // --------------------------------------------------

            Varyings Vert(
                Attributes input
            )
            {
                Varyings output;

                float3 position =
                    input.positionOS.xyz;


                // Subtle glitch movement.

                float glitchTime =
                    floor(
                        _Time.y *
                        max(
                            _VFXGlitchSpeed,
                            0.001
                        )
                    );


                float glitchNoise =
                    SparkHash(
                        float2(
                            input.uv.y,
                            glitchTime
                        )
                    );


                float glitchOffset =
                    (
                        glitchNoise -
                        0.5
                    )
                    *
                    _VFXGlitch
                    *
                    0.01;


                position.x +=
                    glitchOffset;


                output.positionHCS =
                    TransformObjectToHClip(
                        position
                    );


                output.color =
                    input.color *
                    _Color;


                output.uv =
                    input.uv;


                output.worldPosition =
                    input.positionOS;


                return output;
            }


            // --------------------------------------------------
            // FRAGMENT
            // --------------------------------------------------

            half4 Frag(
                Varyings input
            ) :
                SV_Target
            {
                float2 uv =
                    input.uv;


                // --------------------------------------------------
                // BASE
                // --------------------------------------------------

                half4 textureSample =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        uv
                    );


                float alpha =
                    textureSample.a *
                    input.color.a;


                float3 baseColor =
                    textureSample.rgb *
                    input.color.rgb;


                // --------------------------------------------------
                // REVEAL
                // --------------------------------------------------

                if (
                    _VFXReveal > 0.001
                )
                {
                    float revealMask =
                        step(
                            uv.x,
                            _VFXReveal
                        );

                    alpha *=
                        revealMask;
                }


                // --------------------------------------------------
                // GLOW
                // --------------------------------------------------

                baseColor +=
                    _VFXGlowColor.rgb *
                    _VFXGlow *
                    alpha;


                // --------------------------------------------------
                // SCAN
                // --------------------------------------------------

                float scanPosition =
                    frac(
                        _Time.y *
                        _VFXScanSpeed
                    );


                float scanDistance =
                    abs(
                        uv.y -
                        scanPosition
                    );


                float scanMask =
                    1.0 -
                    smoothstep(
                        0.0,
                        max(
                            _ScanWidth,
                            0.001
                        ),
                        scanDistance
                    );


                baseColor +=
                    _VFXScanColor.rgb *
                    scanMask *
                    _VFXScan;


                // --------------------------------------------------
                // ENERGY SWEEP
                // --------------------------------------------------

                float sweepDistance =
                    abs(
                        uv.x -
                        _VFXSweepPosition
                    );


                float sweepMask =
                    1.0 -
                    smoothstep(
                        0.0,
                        max(
                            _SweepWidth,
                            0.001
                        ),
                        sweepDistance
                    );


                baseColor +=
                    _VFXSweepColor.rgb *
                    sweepMask *
                    _VFXSweep;


                // --------------------------------------------------
                // FLASH
                // --------------------------------------------------

                baseColor +=
                    _VFXColor.rgb *
                    _VFXFlash;


                // --------------------------------------------------
                // FLICKER
                // --------------------------------------------------

                float flickerTime =
                    floor(
                        _Time.y *
                        max(
                            _VFXFlickerSpeed,
                            0.001
                        )
                    );


                float flickerNoise =
                    SparkHash(
                        float2(
                            flickerTime,
                            1.0
                        )
                    );


                float flicker =
                    lerp(
                        1.0,
                        flickerNoise,
                        _VFXFlicker
                    );


                alpha *=
                    flicker;


                // --------------------------------------------------
                // DISSOLVE
                // --------------------------------------------------

                float dissolveNoise =
                    SparkHash(
                        uv *
                        100.0
                    );


                float dissolveMask =
                    step(
                        _VFXDissolve,
                        dissolveNoise
                    );


                alpha *=
                    dissolveMask;


                // --------------------------------------------------
                // GLITCH COLOR
                // --------------------------------------------------

                float glitchLine =
                    floor(
                        uv.y *
                        40.0
                    );


                float glitchFrame =
                    floor(
                        _Time.y *
                        max(
                            _VFXGlitchSpeed,
                            0.001
                        )
                    );


                float glitchNoise =
                    SparkHash(
                        float2(
                            glitchLine,
                            glitchFrame
                        )
                    );


                float glitchMask =
                    step(
                        1.0 -
                        _VFXGlitch,
                        glitchNoise
                    );


                baseColor =
                    lerp(
                        baseColor,
                        _VFXColor.rgb,
                        glitchMask *
                        _VFXGlitch
                    );


                // --------------------------------------------------
                // FINAL
                // --------------------------------------------------

                half4 result =
                    half4(
                        baseColor,
                        alpha
                    );


                #ifdef UNITY_UI_ALPHACLIP

                clip(
                    result.a -
                    0.001
                );

                #endif


                return result;
            }

            ENDHLSL
        }
    }

    FallBack Off
}