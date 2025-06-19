Shader "Ultimate 10+ Shaders/Force Field Fixed"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (1,1,1,1)
        _FresnelPower ("Fresnel Power", Range(0, 10)) = 3
        _ScrollDirection ("Scroll Direction", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "IgnoreProjector"="True" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Cull Back
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _Color;
            float _FresnelPower;
            float4 _ScrollDirection;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float rim : TEXCOORD1;
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;

                // Position
                o.pos = UnityObjectToClipPos(v.vertex);

                // UV + Scroll
                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                uv += _ScrollDirection.xy * _Time.y;
                o.uv = uv;

                // Fresnel calculation with fallback minimum
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 viewDir = normalize(UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex).xyz));
                float rawRim = 1.0 - saturate(dot(viewDir, worldNormal));
                o.rim = saturate(pow(rawRim, _FresnelPower) + 0.1); // prevents full disappearance

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv) * _Color;
                tex *= i.rim;
                tex.a = i.rim;
                return tex;
            }

            ENDCG
        }
    }

    FallBack "Transparent/Diffuse"
}
