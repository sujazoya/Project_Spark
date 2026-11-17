Shader "ProjectSpark/Level95/PulseHologramURP"
{
    Properties
    {
        _MainTex ("Holographic Pattern", 2D) = "white" {}
        _Tint ("Cyan Tint (RGBA)", Color) = (0.04, 0.72, 1.0, 0.66)
        _ScrollSpeed ("Scroll Speed", Range(-6,6)) = 0.48
        _ScanDensity ("Scan Density", Range(1,250)) = 94
        _ScanStrength ("Scan Strength", Range(0,1)) = 0.16
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off Blend SrcAlpha One
        Pass
        {
            Name "UnlitHologram"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Tint;
                float _ScrollSpeed;
                float _ScanDensity;
                float _ScanStrength;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            Varyings Vert(Attributes i) {
                Varyings o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz);
                o.uv=i.uv*_MainTex_ST.xy + _MainTex_ST.zw; return o;
            }
            half4 Frag(Varyings i):SV_Target {
                float2 uv=i.uv; uv.y += _Time.y*_ScrollSpeed*0.07;
                half4 albedo=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv);
                float scan=0.5+0.5*sin(uv.y*_ScanDensity*6.283185);
                float intensity=1.0-_ScanStrength*(1.0-scan);
                return half4(albedo.rgb * _Tint.rgb * intensity,albedo.a*_Tint.a);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
