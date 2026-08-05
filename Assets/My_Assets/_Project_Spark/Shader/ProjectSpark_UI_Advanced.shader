Shader "ProjectSpark/UI/AdvancedHologram"
{
    Properties
    {
        [PerRendererData] _MainTex ("UI Texture", 2D) = "white" {}

        _Color ("Base Color", Color) = (1,1,1,1)

        _Tiling ("Texture Tiling", Vector) = (1,1,0,0)
        _Offset ("Texture Offset", Vector) = (0,0,0,0)

        _ScrollSpeed ("UV Scroll Speed", Vector) = (0,0,0,0)

        _GlowColor ("Glow Color", Color) = (0,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0,10)) = 2

        _ScanlineDensity ("Scanline Density", Range(1,500)) = 100
        _ScanlineSpeed ("Scanline Speed", Range(-10,10)) = 1
        _ScanlineStrength ("Scanline Strength", Range(0,1)) = 0.2

        _PulseSpeed ("Pulse Speed", Range(0,10)) = 1
        _PulseStrength ("Pulse Strength", Range(0,1)) = 0.2

        _GlitchAmount ("Glitch Amount", Range(0,0.1)) = 0
        _GlitchSpeed ("Glitch Speed", Range(0,20)) = 5

        _EdgeGlow ("Edge Glow", Range(0,5)) = 1

        _Alpha ("Alpha", Range(0,1)) = 1

        [Toggle] _UseGlitch ("Enable Glitch", Float) = 0
        [Toggle] _UseScanlines ("Enable Scanlines", Float) = 1
        [Toggle] _UsePulse ("Enable Pulse", Float) = 1
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

        Pass
        {
            Name "ProjectSparkUI"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
                float2 screenUV : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_ST;

            CBUFFER_START(UnityPerMaterial)

            float4 _Color;

            float4 _Tiling;
            float4 _Offset;

            float4 _ScrollSpeed;

            float4 _GlowColor;
            float _GlowIntensity;

            float _ScanlineDensity;
            float _ScanlineSpeed;
            float _ScanlineStrength;

            float _PulseSpeed;
            float _PulseStrength;

            float _GlitchAmount;
            float _GlitchSpeed;

            float _EdgeGlow;

            float _Alpha;

            float _UseGlitch;
            float _UseScanlines;
            float _UsePulse;

            CBUFFER_END

            #ifdef UNITY_UI_CLIP_RECT
            float4 _ClipRect;
            #endif

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS =
                    TransformObjectToHClip(IN.positionOS.xyz);

                OUT.color = IN.color * _Color;

                OUT.uv =
                    IN.uv * _Tiling.xy
                    + _Offset.xy;

                float2 screenPos =
                    OUT.positionHCS.xy /
                    OUT.positionHCS.w;

                OUT.screenUV =
                    screenPos * 0.5 + 0.5;

                return OUT;
            }

            float RandomNoise(float2 p)
            {
                return frac(
                    sin(
                        dot(
                            p,
                            float2(12.9898,78.233)
                        )
                    )
                    * 43758.5453
                );
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float time = _Time.y;

                // --------------------------------
                // UV SCROLL
                // --------------------------------

                float2 uv =
                    IN.uv
                    + _ScrollSpeed.xy * time;

                // --------------------------------
                // GLITCH DISTORTION
                // --------------------------------

                float glitchNoise =
                    RandomNoise(
                        float2(
                            floor(uv.y * 50),
                            floor(time * _GlitchSpeed)
                        )
                    );

                float glitchOffset =
                    (glitchNoise - 0.5)
                    * _GlitchAmount
                    * _UseGlitch;

                uv.x += glitchOffset;

                // --------------------------------
                // SAMPLE TEXTURE
                // --------------------------------

                half4 tex =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        uv
                    );

                // --------------------------------
                // SCANLINES
                // --------------------------------

                float scan =
                    sin(
                        uv.y
                        * _ScanlineDensity
                        + time
                        * _ScanlineSpeed
                    );

                scan =
                    scan * 0.5 + 0.5;

                float scanEffect =
                    lerp(
                        1.0,
                        scan,
                        _ScanlineStrength
                        * _UseScanlines
                    );

                // --------------------------------
                // PULSE
                // --------------------------------

                float pulse =
                    sin(
                        time
                        * _PulseSpeed
                    )
                    * 0.5
                    + 0.5;

                float pulseEffect =
                    lerp(
                        1.0,
                        lerp(
                            1.0,
                            pulse,
                            _PulseStrength
                        ),
                        _UsePulse
                    );

                // --------------------------------
                // EDGE GLOW
                // --------------------------------

                float2 centeredUV =
                    IN.uv * 2.0 - 1.0;

                float edge =
                    max(
                        abs(centeredUV.x),
                        abs(centeredUV.y)
                    );

                float edgeGlow =
                    pow(
                        saturate(edge),
                        4.0
                    )
                    * _EdgeGlow;

                // --------------------------------
                // HOLOGRAPHIC COLOR
                // --------------------------------

                float3 baseColor =
                    tex.rgb
                    * IN.color.rgb;

                float3 glow =
                    _GlowColor.rgb
                    * _GlowIntensity;

                baseColor =
                    baseColor
                    * scanEffect
                    * pulseEffect;

                baseColor +=
                    glow
                    * edgeGlow
                    * tex.a;

                // --------------------------------
                // FINAL ALPHA
                // --------------------------------

                float alpha =
                    tex.a
                    * IN.color.a
                    * _Alpha;

                #ifdef UNITY_UI_CLIP_RECT

                float2 inside =
                    step(
                        _ClipRect.xy,
                        IN.positionHCS.xy
                    )
                    *
                    step(
                        IN.positionHCS.xy,
                        _ClipRect.zw
                    );

                alpha *=
                    inside.x
                    * inside.y;

                #endif

                #ifdef UNITY_UI_ALPHACLIP

                clip(alpha - 0.001);

                #endif

                return half4(
                    baseColor,
                    alpha
                );
            }

            ENDHLSL
        }
    }
}