Shader "ProjectSpark/PCB/AAA Hologram UI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ScanColor ("Scanline Color", Color) = (0.08,0.65,0.85,1)
        _ScanlineStrength ("Scanline Strength", Range(0,0.5)) = 0.09
        _ScanlineDensity ("Density", Float) = 90
        _ScanlineSpeed ("Speed", Float) = 0.2
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
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
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
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct a2v { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            struct v2f { float4 position:SV_POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; float4 worldPosition:TEXCOORD1; };
            sampler2D _MainTex;
            fixed4 _Color, _ScanColor;
            float4 _ClipRect;
            float _ScanlineStrength, _ScanlineDensity, _ScanlineSpeed;
            v2f vert(a2v input)
            {
                v2f o;
                o.worldPosition=input.vertex;
                o.position=UnityObjectToClipPos(input.vertex);
                o.color=input.color*_Color;
                o.uv=input.uv;
                return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv)*i.color;
                float row = sin(i.uv.y * _ScanlineDensity * 6.283185 + _Time.y * _ScanlineSpeed * 6.283185);
                c.rgb += _ScanColor.rgb * max(0,row) * _ScanlineStrength * c.a;
                #ifdef UNITY_UI_CLIP_RECT
                    c.a*=UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                    clip(c.a-0.001);
                #endif
                return c;
            }
            ENDCG
        }
    }
}
