Shader "Project Spark/TMP/PVFX"
{
    Properties
    {
        // ============================================================
        // TMP SDF
        // ============================================================

        [PerRendererData]
        _MainTex ("Font Atlas", 2D) = "white" {}

        _FaceColor ("Face Color", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)

        _OutlineWidth ("Outline Width", Range(0,1)) = 0

        _FaceDilate ("Face Dilate", Range(-1,1)) = 0

        _OutlineSoftness ("Outline Softness", Range(0,1)) = 0

        _UnderlayColor ("Underlay Color", Color) = (0,0,0,0)

        _UnderlayOffsetX ("Underlay X", Range(-1,1)) = 0

        _UnderlayOffsetY ("Underlay Y", Range(-1,1)) = 0

        _UnderlayDilate ("Underlay Dilate", Range(-1,1)) = 0

        _UnderlaySoftness ("Underlay Softness", Range(0,1)) = 0


        // ============================================================
        // PROJECT SPARK THEME
        // ============================================================

        _VFXColor ("VFX Color", Color) = (1,1,1,1)

        _VFXGlowColor ("VFX Glow Color", Color) = (1,1,1,1)

        _VFXScanColor ("VFX Scan Color", Color) = (1,1,1,1)

        _VFXSweepColor ("VFX Sweep Color", Color) = (1,1,1,1)


        // ============================================================
        // VFX INTENSITY
        // ============================================================

        _VFXGlow ("Glow", Range(0,5)) = 0

        _VFXScan ("Scan", Range(0,5)) = 0

        _VFXSweep ("Sweep", Range(0,5)) = 0

        _VFXFlash ("Flash", Range(0,5)) = 0

        _VFXGlitch ("Glitch", Range(0,1)) = 0

        _VFXFlicker ("Flicker", Range(0,1)) = 0

        _VFXReveal ("Reveal", Range(0,1)) = 0

        _VFXDissolve ("Dissolve", Range(0,1)) = 0


        // ============================================================
        // ANIMATION
        // ============================================================

        _VFXScanSpeed ("Scan Speed", Float) = 1

        _VFXSweepPosition ("Sweep Position", Range(-1,2)) = 0

        _VFXSweepWidth ("Sweep Width", Range(0.001,1)) = 0.2

        _VFXGlitchSpeed ("Glitch Speed", Float) = 10

        _VFXFlickerSpeed ("Flicker Speed", Float) = 8

        _VFXDissolveEdge ("Dissolve Edge", Range(0.001,1)) = 0.05


        // ============================================================
        // UI MASKING
        // ============================================================

        _StencilComp ("Stencil Comparison", Float) = 8

        _Stencil ("Stencil ID", Float) = 0

        _StencilOp ("Stencil Operation", Float) = 0

        _StencilWriteMask ("Stencil Write Mask", Float) = 255

        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

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


        // ============================================================
        // RENDER
        // ============================================================

        Cull Off

        ZWrite Off

        ZTest [unity_GUIZTestMode]

        Blend SrcAlpha OneMinusSrcAlpha

        ColorMask [_ColorMask]


        Pass
        {
            Name "Project Spark TMP PVFX"


            HLSLPROGRAM


            // ========================================================
            // ENTRY POINT
            // ========================================================

            #pragma vertex Vert

            #pragma fragment Frag

            #pragma target 3.0


            // ========================================================
            // UI CLIPPING
            // ========================================================

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP


            // ========================================================
            // URP CORE
            // ========================================================

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


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
            // FRAGMENT INPUT
            // ========================================================

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;

                float4 color : COLOR;

                float2 uv : TEXCOORD0;

                float4 positionOS : TEXCOORD1;
            };


            // ========================================================
            // FONT ATLAS
            // ========================================================

            TEXTURE2D(_MainTex);

            SAMPLER(sampler_MainTex);


            // ========================================================
            // MATERIAL CONSTANTS
            // ========================================================

            CBUFFER_START(UnityPerMaterial)


                float4 _FaceColor;

                float4 _OutlineColor;

                float _OutlineWidth;

                float _FaceDilate;

                float _OutlineSoftness;


                float4 _UnderlayColor;

                float _UnderlayOffsetX;

                float _UnderlayOffsetY;

                float _UnderlayDilate;

                float _UnderlaySoftness;


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

                float _VFXReveal;

                float _VFXDissolve;


                float _VFXScanSpeed;

                float _VFXSweepPosition;

                float _VFXSweepWidth;

                float _VFXGlitchSpeed;

                float _VFXFlickerSpeed;

                float _VFXDissolveEdge;


                float4 _ClipRect;


            CBUFFER_END


            // ========================================================
            // HASH
            // ========================================================

            float SparkHash(float2 p)
            {
                p =
                    frac(
                        p *
                        float2(
                            123.34,
                            456.21
                        )
                    );


                p +=
                    dot(
                        p,
                        p +
                        45.32
                    );


                return
                    frac(
                        p.x *
                        p.y
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


                float3 position =
                    input.positionOS.xyz;


                // ----------------------------------------------------
                // GLITCH
                // ----------------------------------------------------

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
                            input.uv.y,
                            glitchFrame
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


                // ----------------------------------------------------
                // TRANSFORM
                // ----------------------------------------------------

                output.positionHCS =
                    TransformObjectToHClip(
                        position
                    );


                // ----------------------------------------------------
                // DATA
                // ----------------------------------------------------

                output.color =
                    input.color *
                    _FaceColor;


                output.uv =
                    input.uv;


                output.positionOS =
                    input.positionOS;


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


                // ====================================================
                // FONT SAMPLE
                // ====================================================

                float sdf =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv
                    ).a;


                // ====================================================
                // SDF PARAMETERS
                // ====================================================

                float faceThreshold =
                    0.5 -
                    _FaceDilate *
                    0.1;


                float outlineThreshold =
                    faceThreshold -
                    _OutlineWidth *
                    0.1;


                // ====================================================
                // FACE
                // ====================================================

                float faceAlpha =
                    smoothstep(
                        faceThreshold -
                        _OutlineSoftness *
                        0.05,

                        faceThreshold +
                        _OutlineSoftness *
                        0.05,

                        sdf
                    );


                // ====================================================
                // OUTLINE
                // ====================================================

                float outlineAlpha =
                    smoothstep(
                        outlineThreshold -
                        _OutlineSoftness *
                        0.05,

                        outlineThreshold +
                        _OutlineSoftness *
                        0.05,

                        sdf
                    );


                float outlineOnly =
                    saturate(
                        outlineAlpha -
                        faceAlpha
                    );


                // ====================================================
                // BASE COLOR
                // ====================================================

                float3 faceColor =
                    _FaceColor.rgb;


                float3 finalColor =
                    faceColor *
                    faceAlpha;


                // ====================================================
                // OUTLINE COLOR
                // ====================================================

                finalColor +=
                    _OutlineColor.rgb *
                    outlineOnly;


                float finalAlpha =
                    max(
                        faceAlpha,
                        outlineOnly
                    );


                // ====================================================
                // UNDERLAY
                // ====================================================

                float2 underlayUV =
                    input.uv;


                underlayUV.x +=
                    _UnderlayOffsetX *
                    0.01;


                underlayUV.y +=
                    _UnderlayOffsetY *
                    0.01;


                float underlaySDF =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        underlayUV
                    ).a;


                float underlayThreshold =
                    0.5 -
                    _UnderlayDilate *
                    0.1;


                float underlayAlpha =
                    smoothstep(
                        underlayThreshold -
                        _UnderlaySoftness *
                        0.05,

                        underlayThreshold +
                        _UnderlaySoftness *
                        0.05,

                        underlaySDF
                    );


                finalColor +=
                    _UnderlayColor.rgb *
                    underlayAlpha *
                    _UnderlayColor.a;


                finalAlpha =
                    max(
                        finalAlpha,
                        underlayAlpha *
                        _UnderlayColor.a
                    );


                // ====================================================
                // REVEAL
                // ====================================================

                float revealMask =
                    step(
                        input.uv.x,
                        _VFXReveal
                    );


                float revealEnabled =
                    step(
                        0.001,
                        _VFXReveal
                    );


                float finalReveal =
                    lerp(
                        1.0,
                        revealMask,
                        revealEnabled
                    );


                finalAlpha *=
                    finalReveal;


                // ====================================================
                // SCAN
                // ====================================================

                float scanPosition =
                    frac(
                        _Time.y *
                        max(
                            _VFXScanSpeed,
                            0.001
                        )
                    );


                float scanDistance =
                    abs(
                        input.uv.y -
                        scanPosition
                    );


                float scanMask =
                    1.0 -
                    smoothstep(
                        0.0,
                        0.05,
                        scanDistance
                    );


                finalColor +=
                    _VFXScanColor.rgb *
                    scanMask *
                    _VFXScan *
                    finalAlpha;


                // ====================================================
                // SWEEP
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
                        max(
                            _VFXSweepWidth,
                            0.001
                        ),
                        sweepDistance
                    );


                finalColor +=
                    _VFXSweepColor.rgb *
                    sweepMask *
                    _VFXSweep *
                    finalAlpha;


                // ====================================================
                // GLOW
                // ====================================================

                finalColor +=
                    _VFXGlowColor.rgb *
                    _VFXGlow *
                    finalAlpha;


                // ====================================================
                // FLASH
                // ====================================================

                finalColor +=
                    _VFXColor.rgb *
                    _VFXFlash *
                    finalAlpha;


                // ====================================================
                // FLICKER
                // ====================================================

                float flickerFrame =
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
                            flickerFrame,
                            1.0
                        )
                    );


                float flicker =
                    lerp(
                        1.0,
                        flickerNoise,
                        _VFXFlicker
                    );


                finalAlpha *=
                    flicker;


                // ====================================================
                // DISSOLVE
                // ====================================================

                float dissolveNoise =
                    SparkHash(
                        input.uv *
                        100.0
                    );


                float dissolve =
                    smoothstep(
                        _VFXDissolve,
                        _VFXDissolve +
                        max(
                            _VFXDissolveEdge,
                            0.001
                        ),
                        dissolveNoise
                    );


                finalAlpha *=
                    dissolve;


                // ====================================================
                // GLITCH COLOR
                // ====================================================

                float glitchLine =
                    floor(
                        input.uv.y *
                        50.0
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


                finalColor =
                    lerp(
                        finalColor,
                        _VFXColor.rgb *
                        max(
                            length(
                                finalColor
                            ),
                            1.0
                        ),
                        glitchMask *
                        _VFXGlitch
                    );


                // ====================================================
                // UI CLIP RECT
                // ====================================================

                #ifdef UNITY_UI_CLIP_RECT

                    float2 clipMin =
                        _ClipRect.xy;


                    float2 clipMax =
                        _ClipRect.zw;


                    float2 position =
                        input.positionOS.xy;


                    float2 insideMin =
                        step(
                            clipMin,
                            position
                        );


                    float2 insideMax =
                        step(
                            position,
                            clipMax
                        );


                    float clipMask =
                        insideMin.x *
                        insideMin.y *
                        insideMax.x *
                        insideMax.y;


                    finalAlpha *=
                        clipMask;

                #endif


                // ====================================================
                // ALPHA CLIP
                // ====================================================

                #ifdef UNITY_UI_ALPHACLIP

                    clip(
                        finalAlpha -
                        0.001
                    );

                #endif


                // ====================================================
                // FINAL
                // ====================================================

                return
                    half4(
                        finalColor,
                        finalAlpha
                    );
            }


            ENDHLSL
        }
    }


    FallBack Off
}