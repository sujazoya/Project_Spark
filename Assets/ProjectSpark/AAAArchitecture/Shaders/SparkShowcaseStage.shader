Shader "ProjectSpark/Preview/Showcase Stage"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Off ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION; half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; half4 color:COLOR; };
            Varyings Vert(Attributes a) { Varyings o; o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.color=a.color;return o; }
            half4 Frag(Varyings i):SV_Target { return i.color; }
            ENDHLSL
        }
    }
    Fallback Off
}
