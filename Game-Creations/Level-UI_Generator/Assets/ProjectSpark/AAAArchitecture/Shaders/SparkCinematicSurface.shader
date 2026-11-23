Shader "ProjectSpark/UI/Cinematic Surface"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Accent ("Metal edge color", Color) = (0.24,0.8,1,1)
        _Role ("Role: 0 Plate, 1 Frame, 2 Backdrop, 3 Hero", Float) = 0
        _Motion ("Animation", Range(0,1)) = 1
        _Glow ("Luminous edge", Range(0,2)) = 0.65
        _Metal ("Bevel sheen", Range(0,1)) = 0.5
        _Grid ("Technical overlay", Range(0,1)) = 0.3
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
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            Name "CinematicUI"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; float2 localUV:TEXCOORD1; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float2 p:TEXCOORD1; half4 mask:TEXCOORD2; UNITY_VERTEX_OUTPUT_STEREO };
            sampler2D _MainTex;
            float4 _MainTex_ST, _ClipRect;
            fixed4 _Color, _TextureSampleAdd;
            float4 _Accent;
            float _Role,_Motion,_Glow,_Metal,_Grid,_UIMaskSoftnessX,_UIMaskSoftnessY;
            // Shared unscaled clock, set by active Canvas only. No material per card.
            float _SparkCinematicClock;
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos=UnityObjectToClipPos(v.vertex);
                o.uv=TRANSFORM_TEX(v.uv,_MainTex);
                o.p=v.localUV;
                o.color=v.color*_Color;
                float2 pixelSize=o.pos.w / max(abs(mul((float2x2)UNITY_MATRIX_P,_ScreenParams.xy)),float2(1e-6,1e-6));
                float4 clipRect=clamp(_ClipRect,-2e10,2e10);
                o.mask=half4(v.vertex.xy*2-clipRect.xy-clipRect.zw,0.25/(0.25*half2(_UIMaskSoftnessX,_UIMaskSoftnessY)+abs(pixelSize)));
                return o;
            }
            float GridLine(float x,float frequency)
            {
                float q=x*frequency;
                float dist=abs(frac(q+0.5)-0.5);
                return 1-smoothstep(0.015,0.015+max(fwidth(q),0.015),dist);
            }
            fixed4 frag(v2f i):SV_Target
            {
                float4 tex=tex2D(_MainTex,i.uv)+_TextureSampleAdd;
                float4 c=tex*i.color;
                float2 p=saturate(i.p);
                float t=_SparkCinematicClock;
                float phase=frac(t*0.18);
                float sweep=1-smoothstep(.025,.15,abs(frac(p.x+p.y*.55-phase+.5)-.5));
                float flicker=.9+.1*sin(t*2.1);
                if (_Role<0.5)
                {
                    // A restrained highlight across the baked metal, additive on dark surfaces.
                    float bevel=pow(saturate(1-abs(p.y-.88)*5),3);
                    float shoulder=pow(saturate(1-abs(p.y-.12)*7),3);
                    float hairline=.5+.5*sin(p.y*800.0);
                    c.rgb+=_Accent.rgb*_Metal*(bevel*.085+shoulder*.035+sweep*.12*_Motion+hairline*.004)*i.color.rgb;
                }
                else if (_Role<1.5)
                {
                    // The original nine-sliced frame sprite supplies the actual shape/mask.
                    // Vertex tint carries selection, status and theme colors.
                    float corner=pow(saturate(1-min(p.x,1-p.x)*5),3)*pow(saturate(1-min(p.y,1-p.y)*5),3);
                    float narrow=1-smoothstep(.014,.055,abs(frac(p.x+p.y*.55-phase+.5)-.5));
                    float pulse=_Motion*(sweep*.6+narrow*.5+corner*flicker*.32);
                    c.rgb=saturate(c.rgb*(.85+_Glow*(.32+pulse)));
                    c.rgb=lerp(c.rgb,float3(1,1,1),saturate((_Glow*.09+pulse*.40)*tex.a));
                    c.a*=saturate(.87+_Glow*.15+pulse*.15);
                }
                else
                {
                    float grid=max(GridLine(p.x,24),GridLine(p.y,16));
                    float vignette=saturate(1-dot(p-.5,p-.5)*1.6);
                    float diagonal=pow(saturate(1-abs(p.x+p.y*.65-.92)*2.5),3);
                    c.rgb+=_Accent.rgb*(grid*.035+diagonal*.055)*_Grid*vignette*i.color.rgb;
                    if (_Role<2.5) c.rgb+=_Accent.rgb*(sweep*.013*_Motion)*vignette;
                }
                #ifdef UNITY_UI_CLIP_RECT
                    half2 m=saturate((_ClipRect.zw-_ClipRect.xy-abs(i.mask.xy))*i.mask.zw);
                    c.a*=m.x*m.y;
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                    clip(c.a-.001);
                #endif
                c.rgb*=c.a;
                return c;
            }
            ENDCG
        }
    }
    Fallback "UI/Default"
}
