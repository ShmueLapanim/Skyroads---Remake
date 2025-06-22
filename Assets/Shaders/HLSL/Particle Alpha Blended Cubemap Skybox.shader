Shader "URP/Particles/AlphaBlendedCubemapSkybox"
{
    Properties
    {
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Mask (A)", 2D) = "white" {}
        _CubeMap ("Cubemap", CUBE) = "_Skybox" {}
        _DLightPow ("Dir Light Power", Range(0,10)) = 0.5
        _Glow ("Intensity", Range(0,10)) = 0.0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "PreviewType" = "Plane" }
        LOD 200
        ZWrite Off
        ZTest Off
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldView   : TEXCOORD2;
                float4 color       : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURECUBE(_CubeMap);
            SAMPLER(sampler_CubeMap);

            float4 _MainTex_ST;
            float4 _TintColor;
            float _DLightPow;
            float _Glow;

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;

                o.worldNormal = normalize(TransformObjectToWorldNormal(v.normalOS));
                o.worldView = normalize(GetWorldSpaceViewDir(worldPos));
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 worldRefl = reflect(-i.worldView, normalize(i.worldNormal));

                float4 cubeCol = SAMPLE_TEXTURECUBE(_CubeMap, sampler_CubeMap, worldRefl);
                float4 tex     = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                Light mainLight = GetMainLight();
                float NdotL = max(dot(normalize(i.worldNormal), -mainLight.direction), 0.0);
                float3 lighting = cubeCol.rgb * tex.rgb;

                lighting += mainLight.color.rgb * NdotL * _DLightPow;
                lighting += _Glow;

                float alpha = (cubeCol.r + cubeCol.g + cubeCol.b) / 3.0;
                alpha *= cubeCol.a * tex.a * i.color.a * _TintColor.a;

                return float4(lighting * i.color.rgb * _TintColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
