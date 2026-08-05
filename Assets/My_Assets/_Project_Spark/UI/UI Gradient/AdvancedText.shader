Shader "AdvancedText/OriginalText"
{
    Properties
    {
        _MainTex("Atlas",2D)="white"{}

        _FaceColor("Face Color",Color)=(1,1,1,1)

        _GradientTop("Gradient Top",Color)=(1,1,1,1)
        _GradientBottom("Gradient Bottom",Color)=(0,0.5,1,1)
        _GradientStrength("Gradient Strength",Range(0,1))=1

        _OutlineColor("Outline Color",Color)=(0,0,0,1)
        _OutlineSize("Outline Size",Range(0,0.1))=0.01

        _GlowColor("Glow Color",Color)=(0,1,1,1)
        _GlowPower("Glow",Range(0,10))=2

        _EmissionColor("Emission",Color)=(0,1,1,1)
        _EmissionIntensity("Emission Intensity",Range(0,20))=2
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _FaceColor;

            float4 _GradientTop;
            float4 _GradientBottom;
            float _GradientStrength;

            float4 _OutlineColor;
            float _OutlineSize;

            float4 _GlowColor;
            float _GlowPower;

            float4 _EmissionColor;
            float _EmissionIntensity;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;

                return OUT;
            }

            float SampleAlpha(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv).a;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float alpha = SampleAlpha(IN.uv);

                float2 o=float2(_OutlineSize,0);

                float outline=0;

                outline=max(outline,SampleAlpha(IN.uv+o));
                outline=max(outline,SampleAlpha(IN.uv-o));
                outline=max(outline,SampleAlpha(IN.uv+o.yx));
                outline=max(outline,SampleAlpha(IN.uv-o.yx));

                float edge=saturate(outline-alpha);

                float4 gradient=lerp(
                    _GradientBottom,
                    _GradientTop,
                    IN.uv.y);

                float4 face=_FaceColor;

                face*=lerp(1,gradient,_GradientStrength);

                float3 color=face.rgb*alpha;

                color=lerp(color,_OutlineColor.rgb,edge);

                color+=_GlowColor.rgb*edge*_GlowPower;

                color+=_EmissionColor.rgb*
                       alpha*
                       _EmissionIntensity;

                return float4(color,max(alpha,edge));
            }

            ENDHLSL
        }
    }
}