Shader "ProjectSpark/TerminalEnergyURP"
{
    Properties
    {
        [MainColor] _BaseColor ("Base Tint", Color) = (0.025, 0.09, 0.14, 1)
        _PositiveColor ("Positive Glow", Color) = (1, 0.12, 0.055, 1)
        _NegativeColor ("Negative Glow", Color) = (0.04, 0.4, 1, 1)
        _NeutralColor ("Neutral Glow", Color) = (0.04, 0.8, 0.95, 1)
        _WarningColor ("Short Circuit Glow", Color) = (1, 0.02, 0.01, 1)
        _Intensity ("Intensity", Range(0, 12)) = 0
        _Voltage ("Signed Voltage", Float) = 0
        _Polarity ("Polarity Override (-1/0/1)", Range(-1, 1)) = 0
        _ShortCircuit ("Short Circuit", Range(0, 1)) = 0
        _VoltageForFullGlow ("Voltage For Full Glow", Float) = 5
        _RimPower ("Rim Tightness", Range(0.5, 12)) = 3
        _RimStrength ("Rim Strength", Range(0, 5)) = 1.4
        _CoreStrength ("Core Strength", Range(0, 5)) = 0.6
        _FlowScale ("Energy Flow Scale", Range(0, 40)) = 12
        _FlowSpeed ("Energy Flow Speed", Range(-20, 20)) = 2.5
        _FlowStrength ("Flow Strength", Range(0, 2)) = 0.3
        _FlickerSpeed ("Flicker Speed", Range(0, 50)) = 13
        _FlickerAmount ("Flicker Amount", Range(0, 1)) = 0.08
        _PulseSpeed ("Pulse Speed", Range(0, 30)) = 3
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.12
        _WarningFlashSpeed ("Warning Flash Speed", Range(0, 50)) = 14
        _Opacity ("Opacity", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "TerminalEnergy"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Back
            ZWrite On
            ZTest LEqual
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _PositiveColor;
                half4 _NegativeColor;
                half4 _NeutralColor;
                half4 _WarningColor;
                float _Intensity;
                float _Voltage;
                float _Polarity;
                float _ShortCircuit;
                float _VoltageForFullGlow;
                float _RimPower;
                float _RimStrength;
                float _CoreStrength;
                float _FlowScale;
                float _FlowSpeed;
                float _FlowStrength;
                float _FlickerSpeed;
                float _FlickerAmount;
                float _PulseSpeed;
                float _PulseAmount;
                float _WarningFlashSpeed;
                float _Opacity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings o;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs norm = GetVertexNormalInputs(input.normalOS);
                o.positionCS = pos.positionCS;
                o.positionWS = pos.positionWS;
                o.normalWS = norm.normalWS;
                o.uv = input.uv;
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float t = _Time.y;
                float3 N = normalize(input.normalWS);
                float3 V = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                float ndv = saturate(dot(N, V));
                float rim = pow(1.0 - ndv, max(_RimPower, 0.5));

                float polarity = abs(_Polarity) > 0.1 ? _Polarity :
                                 (_Voltage > 0.001 ? 1.0 : (_Voltage < -0.001 ? -1.0 : 0.0));
                half3 glowColor = polarity > 0.1 ? _PositiveColor.rgb :
                                  (polarity < -0.1 ? _NegativeColor.rgb : _NeutralColor.rgb);

                float flowWave = 0.5 + 0.5 * sin(input.uv.y * _FlowScale - t * _FlowSpeed + input.uv.x * 4.0);
                float flow = lerp(1.0, flowWave, saturate(_FlowStrength));
                float flicker = 1.0 - _FlickerAmount *
                    (0.5 + 0.5 * sin(t * _FlickerSpeed + sin(t * 7.31) * 2.7));
                float pulse = 1.0 + _PulseAmount * sin(t * _PulseSpeed * 6.2831853);
                float warningFlash = 0.5 + 0.5 * sin(t * _WarningFlashSpeed * 6.2831853);
                glowColor = lerp(glowColor, _WarningColor.rgb, saturate(_ShortCircuit));

                float energy = max(_Intensity, 0.0);
                float voltageBoost = saturate(abs(_Voltage) / max(_VoltageForFullGlow, 0.001));
                float shape = _CoreStrength + rim * _RimStrength;
                float animation = flow * flicker * pulse;
                float warningMultiplier = lerp(1.0, 0.35 + warningFlash * 1.65, saturate(_ShortCircuit));
                half3 emission = glowColor * energy * shape * animation * warningMultiplier *
                                 (1.0 + voltageBoost * 0.25);
                half3 finalColor = _BaseColor.rgb + emission;
                return half4(finalColor, _Opacity);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
