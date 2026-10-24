Shader "ProjectSpark/CyanHelixURP"
{
    Properties
    {
        [HDR] _GlowColor ("Cyan Lightning Color", Color) = (0.05, 1.6, 3.5, 1)
        _Intensity ("Runtime Intensity", Float) = 0
        _FlowSpeed ("Flow Speed", Float) = 1
        _Strands ("Helix Strands", Range(1, 4)) = 2
        _Twists ("Turns Along Cable", Range(1, 24)) = 7
        _Width ("Arc Width", Range(0.005, 0.15)) = 0.035
        _Jaggedness ("Jaggedness", Range(0, 0.2)) = 0.045
        _SparkDensity ("Traveling Spark Count", Range(1, 30)) = 12
        _Flicker ("Flicker", Range(0, 1)) = 0.18
        _SurfaceOffset ("Overlay Lift (m)", Range(0.0001, 0.008)) = 0.002
        [HideInInspector] _Polarity ("Polarity", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+20" "RenderType"="Transparent" }
        Pass
        {
            Name "CyanHelicalLightning"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha One
            ZWrite Off
            ZTest LEqual
            Cull Back
            HLSLPROGRAM
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
                float _Polarity;
            CBUFFER_END

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            Varyings Vert(Attributes v)
            {
                Varyings o;
                float3 raised = v.positionOS.xyz + v.normalOS * _SurfaceOffset;
                o.positionCS = TransformObjectToHClip(raised);
                o.uv = v.uv;
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float x = i.uv.x;
                float around = i.uv.y;
                float t = _Time.y * _FlowSpeed;
                // UV.x follows cable length; UV.y wraps around its circumference.
                float jitter = (sin(x * 97.0 - t * 9.0) * 0.5
                              + sin(x * 211.0 + t * 6.0) * 0.25
                              + sin(x * 431.0 - t * 13.0) * 0.125) * _Jaggedness;
                float phase = frac(around * _Strands - x * _Twists + t * 0.38 + jitter);
                float d = min(phase, 1.0 - phase) / max(_Strands, 1.0);
                float arcLine = 1.0 - smoothstep(_Width * 0.3, _Width, d);
                float halo = (1.0 - smoothstep(_Width, _Width * 3.0, d)) * 0.18;

                float sparkPhase = frac(x * _SparkDensity - t * 1.3);
                float spark = pow(saturate(1.0 - abs(sparkPhase - 0.5) * 2.0), 18.0);
                float flicker = 1.0 - _Flicker * (0.5 + 0.5 * sin(t * 19.0 + x * 87.0));
                float alpha = saturate((arcLine + halo) * (0.65 + spark * 0.65) * flicker * _Intensity);
                float brightness = max(0.0, _Intensity) * (0.8 + spark * 1.6);
                return half4(_GlowColor.rgb * brightness, alpha * _GlowColor.a);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
