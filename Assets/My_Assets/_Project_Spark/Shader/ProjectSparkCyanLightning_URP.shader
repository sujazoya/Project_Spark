Shader "ProjectSpark/CyanLightningURP"
{
    Properties
    {
        [HDR] _GlowColor ("Cyan Lightning Color", Color) = (0.02, 1.1, 2.8, 1)
        _Intensity ("Runtime Intensity", Float) = 0
        _FlowSpeed ("Flow Speed", Float) = 1.2
        _Strands ("Helix Strands", Range(1, 4)) = 2
        _Twists ("Turns Along Cable", Range(1, 24)) = 5
        _Width ("Arc Width", Range(0.005, 0.15)) = 0.018
        _Jaggedness ("Jaggedness", Range(0, 0.2)) = 0.075
        _SparkDensity ("Traveling Spark Count", Range(1, 30)) = 9
        _Flicker ("Flicker", Range(0, 1)) = 0.3
        _SurfaceOffset ("Overlay Lift (m)", Range(0.0001, 0.008)) = 0.002
        _GapStrength ("Broken Arc Gaps", Range(0, 1)) = 0.65
        _BranchStrength ("Side Arc Strength", Range(0, 1)) = 0.45
        [HideInInspector] _Polarity ("Polarity", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+20" "RenderType"="Transparent" }
        Pass
        {
            Name "CyanLightning"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha One
            ZWrite Off
            ZTest LEqual
            Cull Back
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            CBUFFER_START(UnityPerMaterial)
                half4 _GlowColor;
                float _Intensity;
                float _FlowSpeed;
                float _Strands;
                float _Twists;
                float _Width;
                float _Jaggedness;
                float _SparkDensity;
                float _Flicker;
                float _SurfaceOffset;
                float _GapStrength;
                float _BranchStrength;
                float _Polarity;
            CBUFFER_END

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float Noise21(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = Hash21(cell);
                float b = Hash21(cell + float2(1, 0));
                float c = Hash21(cell + float2(0, 1));
                float d = Hash21(cell + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz + v.normalOS * _SurfaceOffset);
                o.uv = v.uv;
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float x = i.uv.x;
                float around = i.uv.y;
                float time = _Time.y * _FlowSpeed;
                float strands = max(1.0, _Strands);

                // Multi-frequency distortion breaks the candy-cane regularity.
                float jag = (Noise21(float2(x * 53.0 - time * 2.1, 1.3)) - 0.5) * 1.6;
                jag += (Noise21(float2(x * 147.0 + time * 3.7, 8.1)) - 0.5) * 0.65;
                float phase = frac(around * strands - x * _Twists + time * 0.35 + jag * _Jaggedness * strands);
                float distanceToArc = min(phase, 1.0 - phase) / strands;

                float core = 1.0 - smoothstep(_Width * 0.25, _Width, distanceToArc);
                float halo = (1.0 - smoothstep(_Width, _Width * 3.0, distanceToArc)) * 0.22;

                // Segmented gaps with slow evolution. Never entirely erase every strand.
                float segmentNoise = Noise21(float2(x * 31.0 - time * 0.7, floor(around * strands + 0.1) * 4.0));
                float gaps = lerp(1.0, smoothstep(0.22, 0.68, segmentNoise), _GapStrength);

                // Small forked arcs alongside the main strand, only in selected patches.
                float branchOffset = sin(x * 88.0 + time * 2.4) * _Width * 3.0;
                float branchDistance = abs(distanceToArc - abs(branchOffset));
                float branch = (1.0 - smoothstep(_Width * 0.15, _Width * 0.7, branchDistance));
                float branchMask = smoothstep(0.62, 0.82, Noise21(float2(x * 17.0 + time * 0.35, 5.6)));
                branch *= branchMask * _BranchStrength * 0.45;

                // Sparse bright packets traveling along UV.x.
                float packetPhase = frac(x * _SparkDensity - time * 1.1);
                float packet = pow(saturate(1.0 - abs(packetPhase - 0.5) * 2.0), 22.0);
                float flickerNoise = Noise21(float2(x * 75.0, time * 8.0));
                float flicker = lerp(1.0, 0.45 + 0.55 * flickerNoise, _Flicker);

                float coverage = saturate((core * gaps + halo * gaps + branch) * flicker);
                float alpha = saturate(coverage * (0.7 + packet * 0.75) * max(0.0, _Intensity));
                float brightness = max(0.0, _Intensity) * (0.65 + packet * 1.3);
                return half4(_GlowColor.rgb * brightness, alpha * _GlowColor.a);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
