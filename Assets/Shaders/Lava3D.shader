Shader "Ultimate 10+ Shaders/Lava3D_Fixed"
{
    Properties
    {
        [HDR] _Color ("Color Tint", Color) = (1,1,1,1)
        _MainTex ("Main Texture (Albedo)", 2D) = "white" {}
        _HeightMap ("Height Map", 2D) = "white" {}
        _FlowDirection ("Flow Direction", Vector) = (1, 0, 0, 0)
        _Speed ("Flow Speed", Float) = 0.25
        _Amplitude ("Height Amplitude", Float) = 1.0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull [_Cull]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _HeightMap;
            float4 _Color;
            float4 _FlowDirection;
            float _Speed;
            float _Amplitude;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                float2 flowUV = v.uv + _FlowDirection.xy * _Time.y * _Speed;
                float heightSample = tex2Dlod(_HeightMap, float4(flowUV, 0, 0)).r;
                v.vertex.y += heightSample * _Amplitude;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv + _FlowDirection.xy * _Time.y * _Speed;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }

            ENDCG
        }
    }

    FallBack "Diffuse"
}

