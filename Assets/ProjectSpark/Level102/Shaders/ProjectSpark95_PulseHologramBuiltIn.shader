Shader "ProjectSpark/Level95/PulseHologramBuiltIn"
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
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off Blend SrcAlpha One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Tint;
            float _ScrollSpeed;
            float _ScanDensity;
            float _ScanStrength;
            struct appdata { float4 vertex: POSITION; float2 uv: TEXCOORD0; };
            struct v2f { float4 clip: SV_POSITION; float2 uv: TEXCOORD0; };
            v2f vert(appdata v) {
                v2f o; o.clip=UnityObjectToClipPos(v.vertex);
                o.uv=TRANSFORM_TEX(v.uv,_MainTex); return o;
            }
            fixed4 frag(v2f i):SV_Target {
                float2 uv=i.uv; uv.y += _Time.y*_ScrollSpeed*0.07;
                fixed4 t=tex2D(_MainTex,uv);
                float scan=0.5+0.5*sin(uv.y*_ScanDensity*6.283185);
                float intensity=1.0-_ScanStrength*(1.0-scan);
                fixed4 col = fixed4(t.rgb * _Tint.rgb * intensity,_Tint.a*t.a);
                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
