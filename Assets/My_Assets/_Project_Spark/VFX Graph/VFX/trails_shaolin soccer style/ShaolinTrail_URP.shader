Shader "VFX/ShaolinTrail_URP"
{
    Properties
    {
        _FlameTex ("Flame Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "gray" {}

        _DistortStrength ("Distortion Strength", Float) = 0.1
        _EmissionBoost ("Emission Boost", Float) = 4.0
        _Speed ("Speed", Float) = 1.0

        _ScrollSpeed ("Scroll Speed", Float) = -2.5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _FlameTex;
            sampler2D _NoiseTex;

            float _DistortStrength;
            float _EmissionBoost;
            float _Speed;
            float _ScrollSpeed;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // -----------------------------
                // 1. UV FLOW SCROLL
                // -----------------------------
                float2 uv = IN.uv;
                uv.y += _Time.y * _ScrollSpeed * (_Speed * 0.15);

                // -----------------------------
                // 2. NOISE DISTORTION WARP
                // -----------------------------
                float noise = tex2D(_NoiseTex, uv * 2).r;
                noise = (noise - 0.5);

                uv.x += noise * _DistortStrength * _Speed;

                // -----------------------------
                // 3. SAMPLE FLAME TEXTURE
                // -----------------------------
                half4 flame = tex2D(_FlameTex, uv);

                // -----------------------------
                // 4. ALPHA SHAPING
                // -----------------------------
                float alpha =
                    smoothstep(0.1, 0.8, flame.a) *
                    saturate(_Speed / 10);

                // -----------------------------
                // 5. WHITE HOT CORE BOOST
                // -----------------------------
                float core = pow(flame.a, 3);

                // -----------------------------
                // 6. EMISSION HDR OUTPUT
                // -----------------------------
                half3 emission =
                    flame.rgb *
                    _EmissionBoost *
                    _Speed * 2;

                emission += core * 4;

                return half4(emission, alpha);
            }

            ENDHLSL
        }
    }
}
