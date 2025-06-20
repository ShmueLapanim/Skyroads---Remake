/*

███████╗██████╗░░██████╗░███████╗  ██████╗░███████╗████████╗███████╗░█████╗░████████╗██╗░█████╗░███╗░░██╗
██╔════╝██╔══██╗██╔════╝░██╔════╝  ██╔══██╗██╔════╝╚══██╔══╝██╔════╝██╔══██╗╚══██╔══╝██║██╔══██╗████╗░██║
█████╗░░██║░░██║██║░░██╗░█████╗░░  ██║░░██║█████╗░░░░░██║░░░█████╗░░██║░░╚═╝░░░██║░░░██║██║░░██║██╔██╗██║
██╔══╝░░██║░░██║██║░░╚██╗██╔══╝░░  ██║░░██║██╔══╝░░░░░██║░░░██╔══╝░░██║░░██╗░░░██║░░░██║██║░░██║██║╚████║
███████╗██████╔╝╚██████╔╝███████╗  ██████╔╝███████╗░░░██║░░░███████╗╚█████╔╝░░░██║░░░██║╚█████╔╝██║░╚███║
╚══════╝╚═════╝░░╚═════╝░╚══════╝  ╚═════╝░╚══════╝░░░╚═╝░░░╚══════╝░╚════╝░░░░╚═╝░░░╚═╝░╚════╝░╚═╝░░╚══╝

                            ░██████╗██╗░░██╗░█████╗░██████╗░███████╗██████╗░
                            ██╔════╝██║░░██║██╔══██╗██╔══██╗██╔════╝██╔══██╗
                            ╚█████╗░███████║███████║██║░░██║█████╗░░██████╔╝
                            ░╚═══██╗██╔══██║██╔══██║██║░░██║██╔══╝░░██╔══██╗
                            ██████╔╝██║░░██║██║░░██║██████╔╝███████╗██║░░██║
                            ╚═════╝░╚═╝░░╚═╝╚═╝░░╚═╝╚═════╝░╚══════╝╚═╝░░╚═╝
            
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
Shader "Ultimate 10+ URP/EdgeDetection_Sobel"
{
    Properties
    {
        _Color ("Edge Color", Color) = (1,1,1,1)
        _MainTex ("Screen Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" }
        Pass
        {
            Name "EdgeDetection"
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
            float4 _Color;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float2 texel = _MainTex_TexelSize.xy;

                // Sobel sampling
                float3 gx = 0;
                float3 gy = 0;

                gx += -1 * tex2D(_MainTex, uv + texel * float2(-1, -1)).rgb;
                gx += -2 * tex2D(_MainTex, uv + texel * float2(-1,  0)).rgb;
                gx += -1 * tex2D(_MainTex, uv + texel * float2(-1,  1)).rgb;
                gx +=  1 * tex2D(_MainTex, uv + texel * float2( 1, -1)).rgb;
                gx +=  2 * tex2D(_MainTex, uv + texel * float2( 1,  0)).rgb;
                gx +=  1 * tex2D(_MainTex, uv + texel * float2( 1,  1)).rgb;

                gy += -1 * tex2D(_MainTex, uv + texel * float2(-1, -1)).rgb;
                gy += -2 * tex2D(_MainTex, uv + texel * float2( 0, -1)).rgb;
                gy += -1 * tex2D(_MainTex, uv + texel * float2( 1, -1)).rgb;
                gy +=  1 * tex2D(_MainTex, uv + texel * float2(-1,  1)).rgb;
                gy +=  2 * tex2D(_MainTex, uv + texel * float2( 0,  1)).rgb;
                gy +=  1 * tex2D(_MainTex, uv + texel * float2( 1,  1)).rgb;

                float edge = length(gx + gy);
                return float4(_Color.rgb * edge, edge);
            }

            ENDHLSL
        }
    }
    FallBack Off
}
