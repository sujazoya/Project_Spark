Shader "ProjectSpark/Resistor Cinematic Char URP"
{
    Properties
    {
        [MainTexture] _BaseMap("Resistor Body Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Resistor Body Tint", Color) = (0.9,0.8,0.54,1)
        _Heat("Heat", Range(0,1)) = 0
        _Char("Char", Range(0,1)) = 0
        _CharScale("Burn Pattern Scale", Range(1,60)) = 14
        _GlowStrength("Ember Glow", Range(0,12)) = 5
        _CharColor("Burned Char", Color) = (0.018,0.012,0.01,1)
        _EmberColor("Molten Orange", Color) = (1,0.14,0.01,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _CharColor;
                float4 _EmberColor;
                float _Heat;
                float _Char;
                float _CharScale;
                float _GlowStrength;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; float2 uv : TEXCOORD1; float3 positionWS : TEXCOORD2; };
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }
            float noise2(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash21(i), hash21(i + float2(1,0)), f.x),
                            lerp(hash21(i + float2(0,1)), hash21(i + 1), f.x), f.y);
            }
            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;
                float n = noise2(uv * _CharScale) * 0.65 + noise2(uv * (_CharScale * 2.7)) * 0.35;
                // Uneven burnt patches: darkening spreads progressively from hotspots.
                float burnMask = smoothstep(0.75 - saturate(_Char) * 0.95,
                                            0.98 - saturate(_Char) * 0.75, n);
                float3 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).rgb * _BaseColor.rgb;
                float3 albedo = lerp(baseTex, _CharColor.rgb, burnMask);
                // Thin glowing boundaries between normal paint and burned patches.
                float edge = smoothstep(0.05,0.19,burnMask) * (1 - smoothstep(0.72,0.94,burnMask));
                float ember = edge * saturate(_Heat) * _GlowStrength;
                float3 normal = normalize(i.normalWS);
                Light mainLight = GetMainLight();
                float lambert = saturate(dot(normal, mainLight.direction));
                float3 lit = albedo * (0.30 + lambert * 0.70) * mainLight.color;
                float3 emission = _EmberColor.rgb * (ember + pow(saturate(_Heat), 3) * 0.22);
                return half4(lit + emission, 1);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ColorMask 0
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };
            Varyings vert(Attributes v) { Varyings o; o.positionCS = TransformObjectToHClip(v.positionOS.xyz); return o; }
            half4 frag(Varyings i) : SV_Target { return 0; }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
