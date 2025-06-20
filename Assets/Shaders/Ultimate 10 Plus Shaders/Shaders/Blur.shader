/*
           ██████╗░██╗░░░░░██╗░░░██╗██████╗░  ░██████╗██╗░░██╗░█████╗░██████╗░███████╗██████╗░
           ██╔══██╗██║░░░░░██║░░░██║██╔══██╗  ██╔════╝██║░░██║██╔══██╗██╔══██╗██╔════╝██╔══██╗
           ██████╦╝██║░░░░░██║░░░██║██████╔╝  ╚█████╗░███████║███████║██║░░██║█████╗░░██████╔╝
           ██╔══██╗██║░░░░░██║░░░██║██╔══██╗  ░╚═══██╗██╔══██║██╔══██║██║░░██║██╔══╝░░██╔══██╗
           ██████╦╝███████╗╚██████╔╝██║░░██║  ██████╔╝██║░░██║██║░░██║██████╔╝███████╗██║░░██║
           ╚═════╝░╚══════╝░╚═════╝░╚═╝░░╚═╝  ╚═════╝░╚═╝░░╚═╝╚═╝░░╚═╝╚═════╝░╚══════╝╚═╝░░╚═╝
            
                █▀▀▄ █──█ 　 ▀▀█▀▀ █──█ █▀▀ 　 ░█▀▀▄ █▀▀ ▀█─█▀ █▀▀ █── █▀▀█ █▀▀█ █▀▀ █▀▀█ 
                █▀▀▄ █▄▄█ 　 ─░█── █▀▀█ █▀▀ 　 ░█─░█ █▀▀ ─█▄█─ █▀▀ █── █──█ █──█ █▀▀ █▄▄▀ 
                ▀▀▀─ ▄▄▄█ 　 ─░█── ▀──▀ ▀▀▀ 　 ░█▄▄▀ ▀▀▀ ──▀── ▀▀▀ ▀▀▀ ▀▀▀▀ █▀▀▀ ▀▀▀ ▀─▀▀
____________________________________________________________________________________________________________________________________________

        ▄▀█ █▀ █▀ █▀▀ ▀█▀ ▀   █░█ █░░ ▀█▀ █ █▀▄▀█ ▄▀█ ▀█▀ █▀▀   ▄█ █▀█ ▄█▄   █▀ █░█ ▄▀█ █▀▄ █▀▀ █▀█ █▀
        █▀█ ▄█ ▄█ ██▄ ░█░ ▄   █▄█ █▄▄ ░█░ █ █░▀░█ █▀█ ░█░ ██▄   ░█ █▄█ ░▀░   ▄█ █▀█ █▀█ █▄▀ ██▄ █▀▄ ▄█
____________________________________________________________________________________________________________________________________________
License:
    The license is ATTRIBUTION 3.0

    More license info here:
        https://creativecommons.org/licenses/by/3.0/
____________________________________________________________________________________________________________________________________________
This shader has NOT been tested on any other PC configuration except the following:
    CPU: Intel Core i5-6400
    GPU: NVidia GTX 750Ti
    RAM: 16GB
    Windows: 10 x64
    DirectX: 11
____________________________________________________________________________________________________________________________________________
*/

Shader "Ultimate 10+ URP/Blur"
{
    Properties
    {
        _BlurAmount ("Blur Amount", Range(0, 5)) = 1
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" }
        Pass
        {
            Name "URP_BlurPass"
            ZTest Always
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _BlurAmount;
            float4 _Color;

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 Frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float2 offset = _MainTex_TexelSize.xy * _BlurAmount;

                float4 col = float4(0,0,0,0);

                col += tex2D(_MainTex, uv + offset * float2(-1, -1));
                col += tex2D(_MainTex, uv + offset * float2( 0, -1));
                col += tex2D(_MainTex, uv + offset * float2( 1, -1));

                col += tex2D(_MainTex, uv + offset * float2(-1,  0));
                col += tex2D(_MainTex, uv);
                col += tex2D(_MainTex, uv + offset * float2( 1,  0));

                col += tex2D(_MainTex, uv + offset * float2(-1,  1));
                col += tex2D(_MainTex, uv + offset * float2( 0,  1));
                col += tex2D(_MainTex, uv + offset * float2( 1,  1));

                col /= 9.0;

                return col * _Color;
            }
            ENDHLSL
        }
    }
    FallBack Off
}

