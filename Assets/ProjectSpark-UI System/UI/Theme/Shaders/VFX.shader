Shader "Project Spark/UI/VFX"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        _VFXColor ("VFX Color", Color) = (0,1,1,1)
        _GlowColor ("Glow Color", Color) = (0,1,1,1)
        _ScanColor ("Scan Color", Color) = (0,1,1,1)
        _SweepColor ("Sweep Color", Color) = (0,1,1,1)

        _Glow ("Glow", Range(0,5)) = 0
        _Scan ("Scan", Range(0,5)) = 0
        _Sweep ("Sweep", Range(0,5)) = 0
        _Flash ("Flash", Range(0,5)) = 0

         _ScanEnabled("Scan Enabled", Float) = 0

        _Glitch ("Glitch", Range(0,1)) = 0
        _Flicker ("Flicker", Range(0,1)) = 0

        _Dissolve ("Dissolve", Range(0,1)) = 0
        _Reveal ("Reveal", Range(0,1)) = 1
       

        _SweepPosition ("Sweep Position", Range(-1,2)) = 0.5

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)]
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

            #pragma vertex vert
            #pragma fragment frag

            #pragma target 3.0

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _ClipRect;

            CBUFFER_START(UnityPerMaterial)

                float4 _MainTex_ST;

                float4 _BaseColor;

                float4 _VFXColor;
                float4 _GlowColor;
                float4 _ScanColor;
                float4 _SweepColor;

                float _Glow;
                float _Scan;
                float _Sweep;
                float _Flash;

                float _ScanEnabled;

                float _Glitch;
                float _Flicker;

                float _Dissolve;
                float _Reveal;

                float _SweepPosition;

            CBUFFER_END


            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };


            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 worldPosition : TEXCOORD1;
            };


            float Hash21(float2 p)
            {
                float value;

                p = frac(p * 0.1031);

                p += dot(p, p.yx + 33.33);

                value = frac(
                    (p.x + p.y) * p.x
                );

                return value;
            }


            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInput;

                positionInput =
                    GetVertexPositionInputs(
                        input.positionOS.xyz
                    );

                output.positionCS =
                    positionInput.positionCS;

                output.uv =
                    input.uv *
                    _MainTex_ST.xy +
                    _MainTex_ST.zw;

                output.color =
                    input.color *
                    _BaseColor;

                output.worldPosition =
                    input.positionOS;

                return output;
            }


            half4 frag(Varyings input) : SV_Target
            {
                float2 uv;

                uv =
                    input.uv;


                // ========================================================
                // GLITCH
                // ========================================================

                float glitchNoise;

                glitchNoise =
                    Hash21(
                        float2(
                            floor(
                                uv.y * 100.0
                            ),
                            floor(
                                _Time.y * 12.0
                            )
                        )
                    );


                float glitchOffset;

                glitchOffset =
                    (
                        glitchNoise -
                        0.5
                    )
                    *
                    _Glitch
                    *
                    0.08;


                uv.x +=
                    glitchOffset;


                // ========================================================
                // TEXTURE
                // ========================================================

                half4 textureColor;

                textureColor =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        uv
                    );


                half4 baseColor;

                baseColor =
                    textureColor *
                    input.color;


                // ========================================================
                // GLOW
                // ========================================================

                float glowAmount;

                glowAmount =
                    saturate(
                        _Glow / 5.0
                    );


                half3 glowEffect;

                glowEffect =
                    _GlowColor.rgb *
                    glowAmount *
                    2.0;


                // ========================================================
                // SCAN
                // ========================================================

               float scanDistance =
    abs(
        uv.x -
        _SweepPosition
    );

float scanMask =
    1.0 -
    smoothstep(
        0.0,
        0.05,
        scanDistance
    );

                


                float scanAmount;

                scanAmount =
                    saturate(
                        _Scan / 5.0
                    );


                half3 scanEffect;

               scanEffect =
                _ScanColor.rgb *
                scanMask *
                scanAmount *
                _ScanEnabled *
                3.0;


                // ========================================================
                // SWEEP
                // ========================================================

                float sweepDistance;

                sweepDistance =
                    abs(
                        uv.x -
                        _SweepPosition
                    );


                float sweepMask;

                sweepMask =
                    1.0 -
                    smoothstep(
                        0.0,
                        0.15,
                        sweepDistance
                    );


                float sweepAmount;

                sweepAmount =
                    saturate(
                        _Sweep / 5.0
                    );


                half3 sweepEffect;

                sweepEffect =
                    _SweepColor.rgb *
                    sweepMask *
                    sweepAmount *
                    _ScanEnabled *
                    3.0;


                // ========================================================
                // FLASH
                // ========================================================

                float flashAmount;

                flashAmount =
                    saturate(
                        _Flash / 5.0
                    );


                half3 flashEffect;

                flashEffect =
                    _VFXColor.rgb *
                    flashAmount *
                    4.0;


                // ========================================================
                // FLICKER
                // ========================================================

                float flickerNoise;

                flickerNoise =
                    Hash21(
                        float2(
                            floor(
                                _Time.y *
                                20.0
                            ),
                            5.71
                        )
                    );


                float flickerFactor;

                flickerFactor =
                    lerp(
                        1.0,
                        flickerNoise,
                        _Flicker
                    );


                // ========================================================
                // FINAL COLOR
                // ========================================================

                half3 finalColor;

                finalColor =
                    baseColor.rgb;


                finalColor +=
                    glowEffect;


                finalColor +=
                    scanEffect;


                finalColor +=
                    sweepEffect;


                finalColor +=
                    flashEffect;


                finalColor *=
                    flickerFactor;


                // ========================================================
                // DISSOLVE
                // ========================================================

                float dissolveNoise;

                dissolveNoise =
                    Hash21(
                        floor(
                            uv *
                            80.0
                        )
                    );


                float dissolveMask;

                dissolveMask =
                    step(
                        _Dissolve,
                        dissolveNoise
                    );


                // ========================================================
                // REVEAL
                // ========================================================

                float revealMask;

                revealMask =
                    step(
                        uv.x,
                        _Reveal
                    );


                // ========================================================
                // ALPHA
                // ========================================================

                float finalAlpha;

                finalAlpha =
                    baseColor.a;


                finalAlpha *=
                    dissolveMask;


                finalAlpha *=
                    revealMask;


                // ========================================================
                // UI CLIPPING
                // ========================================================

                #ifdef UNITY_UI_CLIP_RECT

                float2 clipResult;

                clipResult =
                    step(
                        _ClipRect.xy,
                        input.worldPosition.xy
                    )
                    *
                    step(
                        input.worldPosition.xy,
                        _ClipRect.zw
                    );


                finalAlpha *=
                    clipResult.x *
                    clipResult.y;

                #endif


                // ========================================================
                // ALPHA CLIP
                // ========================================================

                #ifdef UNITY_UI_ALPHACLIP

                clip(
                    finalAlpha -
                    0.001
                );

                #endif


                // ========================================================
                // OUTPUT
                // ========================================================

                return half4(
                    finalColor,
                    finalAlpha
                );
            }

            ENDHLSL
        }
    }

    FallBack "UI/Default"
}