Shader "ProjectSpark/ResistorBurnURP"
{
    Properties
    {
        _BaseColor("Ceramic Base Color", Color) = (0.65,0.47,0.30,1)
        _BurnColor("Char Color", Color) = (0.06,0.045,0.035,1)
        _GlowColor("Heat Glow", Color) = (1,0.13,0.015,1)
        _Heat("Heat", Range(0,1)) = 0
        _Char("Char", Range(0,1)) = 0
        _EmissionStrength("Emission Strength", Range(0,8)) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; float3 normalWS : TEXCOORD1; };
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _BurnColor;
                half4 _GlowColor;
                float _Heat;
                float _Char;
                float _EmissionStrength;
            CBUFFER_END
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }
            half4 frag(Varyings input) : SV_Target
            {
                float noise = frac(sin(dot(input.uv * 97.13, float2(12.9898,78.233))) * 43758.5453);
                float charMask = saturate(_Char * (0.78 + noise * 0.45));
                half3 albedo = lerp(_BaseColor.rgb, _BurnColor.rgb, charMask);
                float pulse = 0.86 + 0.14 * sin(_Time.y * 18.0);
                float hotCore = pow(saturate(_Heat), 3.0);
                half3 emission = _GlowColor.rgb * hotCore * _EmissionStrength * pulse * (1.0 - _Char * 0.8);
                float ndotl = saturate(dot(normalize(input.normalWS), normalize(float3(0.4,0.8,0.5))));
                return half4(albedo * (0.3 + ndotl * 0.7) + emission, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
