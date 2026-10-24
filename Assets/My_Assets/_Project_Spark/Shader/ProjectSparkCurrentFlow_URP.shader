Shader "ProjectSpark/CurrentFlowURP"
{
    Properties
    {
        _BaseColor ("Wire Base Color", Color) = (0.02, 0.06, 0.09, 1)
        [HDR] _EnergyColor ("Flow Glow Color", Color) = (0.05, 1.2, 2.5, 1)
        _StripeFrequency ("Stripes Along U", Range(1, 100)) = 18
        _StripeSharpness ("Stripe Sharpness", Range(1, 16)) = 7
        _CurrentAmps ("Signed Current Amps", Float) = 0
        _CurrentFlowSpeed ("Flow Speed", Float) = 0
        _CurrentGlow ("Glow", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "CurrentFlow"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Back
            ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _EnergyColor;
                float _StripeFrequency;
                float _StripeSharpness;
                float _CurrentAmps;
                float _CurrentFlowSpeed;
                float _CurrentGlow;
            CBUFFER_END
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float phase = (input.uv.x * _StripeFrequency - _Time.y * _CurrentFlowSpeed) * 6.2831853;
                float wave = saturate(0.5 + 0.5 * cos(phase));
                float stripe = pow(wave, max(_StripeSharpness, 1.0));
                half3 color = _BaseColor.rgb + _EnergyColor.rgb * (stripe * max(_CurrentGlow, 0.0));
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
