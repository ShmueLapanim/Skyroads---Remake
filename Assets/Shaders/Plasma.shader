Shader "Ultimate 10+ Shaders/Plasma_Fixed"
{
    Properties
    {
        [HDR] _Color ("Color Tint", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Normal ("Normal Map", 2D) = "bump" {}
        _NoiseTex ("Noise", 2D) = "white" {}
        _MovementDirection ("Movement Direction", Vector) = (0, -1, 0, 1)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        Cull [_Cull]
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _Normal;
            sampler2D _NoiseTex;
            float4 _Color;
            float4 _MovementDirection;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float2 uv3 : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float2 offset = _MovementDirection.xy * _Time.y;
                o.uv = v.uv + offset;
                o.uv2 = v.uv + offset * 0.5; // For Noise
                o.uv3 = v.uv + offset * 0.5; // For Normal
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 noise = tex2D(_NoiseTex, i.uv2);
                fixed4 albedo = tex2D(_MainTex, i.uv) * _Color * noise.r;

                // Normal map usage (optional)
                tex2D(_Normal, i.uv3); // Sampled but not affecting lighting directly

                albedo.a = noise.r; // Alpha from noise

                return albedo;
            }

            ENDCG
        }
    }

    FallBack "Transparent/Diffuse"
}

