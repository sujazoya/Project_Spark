Shader "ProjectSpark/RoomStudio/HologramURP"
{
    Properties
    {
        _BaseMap ("Hologram Base", 2D) = "white" {}
        _Tint ("Hologram Tint", Color) = (0.17, 0.85, 1, 0.53)
        _EmissionColor ("Emission Color", Color) = (0.2, 1.4, 2.7, 1)
        _EmissionStrength ("Emission Strength", Range(0,10)) = 2
        _ScanSpeed ("Moving Scan Speed", Range(0,5)) = 0.8
        _ScanStrength ("Scan Contrast", Range(0,1)) = 0.22
        _FresnelStrength ("Fresnel Glow", Range(0,5)) = 1.35
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST, _Tint, _EmissionColor;
                float _EmissionStrength, _ScanSpeed, _ScanStrength, _FresnelStrength;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 positionWS : TEXCOORD1; float3 normalWS : TEXCOORD2; };
            Varyings vert(Attributes input)
            {
                Varyings o;
                VertexPositionInputs pos=GetVertexPositionInputs(input.positionOS.xyz);
                o.positionCS=pos.positionCS;
                o.positionWS=pos.positionWS;
                o.normalWS=TransformObjectToWorldNormal(input.normalOS);
                o.uv=TRANSFORM_TEX(input.uv,_BaseMap);
                return o;
            }
            half4 frag(Varyings i) : SV_Target
            {
                half4 baseSample=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);
                float3 view=normalize(GetWorldSpaceViewDir(i.positionWS));
                float edge=pow(1-saturate(dot(normalize(i.normalWS),view)),2);
                float scan=.5+.5*sin((i.positionWS.y+_Time.y*_ScanSpeed)*31.4);
                float gain=1+edge*_FresnelStrength+scan*_ScanStrength;
                half3 rgb=baseSample.rgb*_Tint.rgb*gain+_EmissionColor.rgb*_EmissionStrength*.22;
                half alpha=saturate(_Tint.a*baseSample.a*(.74+edge*.35));
                return half4(rgb,alpha);
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Unlit"
}
