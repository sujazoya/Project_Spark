Shader "ProjectSpark/WirePulseOverlayURP"
{
    Properties
    {
        [HDR] _GlowColor ("Pulse Color", Color) = (0.3, 1.2, 2.5, 1)
        _Intensity ("Intensity", Float) = 0
        _FlowSpeed ("Flow Speed", Float) = 1
        _PulseCount ("Pulses Per Wire", Range(1, 20)) = 5
        _PulseSharpness ("Pulse Sharpness", Range(1, 32)) = 14
        _Polarity ("Polarity", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Name "ElectricalPulse"
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
                float _PulseCount;
                float _PulseSharpness;
                float _Polarity;
            CBUFFER_END
            Varyings Vert(Attributes v)
            {
                Varyings o;
                // Lift the overlay slightly above the cable surface to avoid z-fighting.
                float3 raised = v.positionOS.xyz + v.normalOS * 0.0015;
                o.positionCS = TransformObjectToHClip(raised);
                o.uv = v.uv;
                return o;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                float phase = (i.uv.x * _PulseCount - _Time.y * _FlowSpeed) * 6.2831853;
                float pulse = pow(saturate(0.5 + 0.5 * cos(phase)), max(1.0, _PulseSharpness));
                float strength = saturate(_Intensity) * pulse;
                return half4(_GlowColor.rgb * max(0, _Intensity), strength * _GlowColor.a);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
