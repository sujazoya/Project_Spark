Shader "ProjectSpark/RoomStudio/HologramBuiltin"
{
    Properties
    {
        _MainTex ("Hologram Base", 2D) = "white" {}
        _Tint ("Hologram Tint", Color) = (0.17, 0.85, 1, 0.53)
        _EmissionColor ("Emission Color", Color) = (0.2, 1.4, 2.7, 1)
        _EmissionStrength ("Emission Strength", Range(0,10)) = 2
        _ScanSpeed ("Moving Scan Speed", Range(0,5)) = 0.8
        _ScanStrength ("Scan Contrast", Range(0,1)) = 0.22
        _FresnelStrength ("Fresnel Glow", Range(0,5)) = 1.35
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _MainTex_ST;
            float4 _Tint, _EmissionColor;
            float _EmissionStrength, _ScanSpeed, _ScanStrength, _FresnelStrength;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct v2f { float4 position : SV_POSITION; float2 uv : TEXCOORD0; float3 wn : TEXCOORD1; float3 toView : TEXCOORD2; float3 world : TEXCOORD3; };
            v2f vert(appdata v)
            {
                v2f o;
                o.position=UnityObjectToClipPos(v.vertex);
                o.uv=TRANSFORM_TEX(v.uv,_MainTex);
                o.wn=UnityObjectToWorldNormal(v.normal);
                o.world=mul(unity_ObjectToWorld,v.vertex).xyz;
                o.toView=_WorldSpaceCameraPos-o.world;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 baseSample=tex2D(_MainTex,i.uv);
                float fresnel=pow(1-saturate(dot(normalize(i.wn),normalize(i.toView))),2);
                float scan=0.5+0.5*sin((i.world.y+_Time.y*_ScanSpeed)*31.4);
                float intensity=1+fresnel*_FresnelStrength+scan*_ScanStrength;
                fixed3 rgb=baseSample.rgb*_Tint.rgb*intensity+_EmissionColor.rgb*_EmissionStrength*.22;
                fixed alpha=saturate(_Tint.a*baseSample.a*(0.74+fresnel*.35));
                return fixed4(rgb,alpha);
            }
            ENDCG
        }
    }
    Fallback "Unlit/Transparent"
}
