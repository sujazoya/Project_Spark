Shader "Custom/TMP_Master_AAA"
{
    Properties
    {
        // TMP SDF texture
        [PerRendererData] _MainTex ("Font Atlas", 2D) = "white" {}

        // Base
        _FaceColor ("Face Color", Color) = (1,1,1,1)

        // Outline
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0,1)) = 0.15
        _OutlineSoftness ("Outline Softness", Range(0,1)) = 0.02

        // Gradient
        [HDR]_GradientTop ("Gradient Top", Color) = (0,1,1,1)
        [HDR]_GradientBottom ("Gradient Bottom", Color) = (0,0.2,1,1)
        _GradientStrength ("Gradient Strength", Range(0,1)) = 1

        // Emission
        [HDR]_EmissionColor ("Emission Color", Color) = (0,1,1,1)
        _EmissionStrength ("Emission Strength", Range(0,10)) = 2

        // Glow
        [HDR]_GlowColor ("Glow Color", Color) = (0,1,1,1)
        _GlowPower ("Glow Power", Range(0,10)) = 3
        _GlowSize ("Glow Size", Range(0,1)) = 0.3

        // SDF settings
        _FaceDilate ("Face Dilate", Range(-1,1)) = 0
        _ScaleRatioA ("Scale Ratio A", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "TMP_MASTER"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
                float2 localPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float4 _FaceColor;
            float4 _OutlineColor;
            float _OutlineWidth;
            float _OutlineSoftness;

            float4 _GradientTop;
            float4 _GradientBottom;
            float _GradientStrength;

            float4 _EmissionColor;
            float _EmissionStrength;

            float4 _GlowColor;
            float _GlowPower;
            float _GlowSize;

            float _FaceDilate;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color;
                o.localPos = v.vertex.xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample SDF
                float sdf = tex2D(_MainTex, i.uv).a;

                // Screen-space smoothing
                float width = fwidth(sdf);

                // Face
                float face = smoothstep(0.5 - width + _FaceDilate,
                                        0.5 + width + _FaceDilate,
                                        sdf);

                // Outline
                float outline = smoothstep(0.5 - _OutlineWidth - _OutlineSoftness - width,
                                           0.5 - _OutlineWidth + width,
                                           sdf);

                // Base color
                float4 col = lerp(_OutlineColor, _FaceColor, face);

                // Vertical gradient
                float gradientT = saturate(i.uv.y);
                float4 gradientCol = lerp(_GradientBottom, _GradientTop, gradientT);
                col.rgb = lerp(col.rgb, col.rgb * gradientCol.rgb, _GradientStrength);

                // Glow / emission around edges
                float glowMask = 1.0 - smoothstep(0.5, 0.5 + _GlowSize, sdf);
                float3 glow = _GlowColor.rgb * glowMask * _GlowPower;

                // Emission
                float3 emission = _EmissionColor.rgb * _EmissionStrength * face;

                // Final
                col.rgb += glow + emission;
                col.a *= max(face, outline);

                // Vertex color support (TMP tint)
                col *= i.color;

                return col;
            }
            ENDHLSL
        }
    }

    FallBack "TextMeshPro/Distance Field"
}