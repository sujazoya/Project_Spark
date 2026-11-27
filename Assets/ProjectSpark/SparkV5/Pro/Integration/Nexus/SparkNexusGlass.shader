Shader "Project Spark/NEXUS/Cyan Glass"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _GridStrength ("Grid Strength", Range(0,0.15)) = 0.045
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct Attributes { float4 position : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Interpolators { float4 position : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; float2 local : TEXCOORD1; };
            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            half _GridStrength;

            Interpolators vert(Attributes input)
            {
                Interpolators output;
                output.position = UnityObjectToClipPos(input.position);
                output.local = input.position.xy;
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            fixed4 frag(Interpolators input) : SV_Target
            {
                fixed4 color = (tex2D(_MainTex, input.uv) + _TextureSampleAdd) * input.color;
                float2 cell = min(input.uv, 1.0 - input.uv);
                half edge = 1.0 - smoothstep(0.0, 0.035, min(cell.x, cell.y));
                float2 grid = abs(frac(input.local / 32.0 + 0.5) - 0.5);
                half gridLine = 1.0 - smoothstep(0.0, 0.07, min(grid.x, grid.y));
                color.rgb += half3(0.08, 0.38, 0.43) * edge * edge * 0.34;
                color.rgb += half3(0.2, 0.78, 0.86) * gridLine * _GridStrength;
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(input.local, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif
                return color;
            }
            ENDCG
        }
    }
}
