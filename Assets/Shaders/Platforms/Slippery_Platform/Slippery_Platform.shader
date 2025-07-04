Shader "URP/SlipperyPlatform_CartoonIce"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _MainColor ("Main Color", Color) = (0.6, 0.9, 1.0, 1)
        _ShadowColor ("Shadow Color", Color) = (0.2, 0.4, 0.6, 1)
        _HighlightColor ("Highlight Color", Color) = (1, 1, 1, 1)
        _SpecularPower ("Specular Power", Float) = 32.0

        _RimColor ("Rim Color", Color) = (0.7, 0.9, 1.0, 1)
        _RimPower ("Rim Power", Float) = 3.0

        _GradientTop ("Gradient Top Color", Color) = (0.8, 1.0, 1.0, 1)
        _GradientBottom ("Gradient Bottom Color", Color) = (0.3, 0.5, 0.6, 1)
        _GradientMinY ("Gradient Min Y", Float) = 0.0
        _GradientMaxY ("Gradient Max Y", Float) = 1.0

        _MainTexTint ("Main Texture Tint", Color) = (1, 1, 1, 1)
        _OverlayTexTint ("Overlay Texture Tint", Color) = (1, 1, 1, 1)

        _GlowFlashColor ("Glow Flash Color", Color) = (2, 2, 2, 1)
        _GlowFlashDuration ("Glow Flash Duration", Float) = 3.0
        _GlowFlashSize ("Glow Flash Size", Float) = 0.15

        _ViewAngleGradientColor ("View Angle Gradient Color", Color) = (1, 1, 1, 1)
        _ViewAngleGradientStrength ("View Angle Gradient Strength", Float) = 0.3
        _NoiseStrength ("Noise Strength", Float) = 0.1
        _ShadowStrength ("Edge Shadow Strength", Float) = 0.3

        // 🔁 NEW Reflection Properties
        _ReflectionTex ("Reflection Cubemap", CUBE) = "" {}
        _ReflectionStrength ("Reflection Strength", Range(0, 1)) = 0.5
        _ReflectionTint ("Reflection Tint", Color) = (1, 1, 1, 1)
        _ReflectionSmoothness ("Reflection Sharpness", Range(0.1, 1)) = 0.7
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 objectNormal : TEXCOORD3;
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_OverlayTex); SAMPLER(sampler_OverlayTex);

            float4 _MainColor, _ShadowColor, _HighlightColor;
            float _SpecularPower;
            float4 _RimColor;
            float _RimPower;

            float4 _GradientTop, _GradientBottom;
            float _GradientMinY, _GradientMaxY;

            float4 _MainTexTint, _OverlayTexTint;
            float4 _GlowFlashColor;
            float _GlowFlashDuration, _GlowFlashSize;

            float4 _ViewAngleGradientColor;
            float _ViewAngleGradientStrength;
            float _NoiseStrength;
            float _ShadowStrength;

            // 🔁 NEW Reflection Uniforms
            TEXTURECUBE(_ReflectionTex);
            SAMPLER(sampler_ReflectionTex);
            float _ReflectionStrength;
            float4 _ReflectionTint;
            float _ReflectionSmoothness;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.worldNormal = TransformObjectToWorldNormal(v.normalOS);
                o.objectNormal = v.normalOS;
                o.uv = v.uv;
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float3 N = normalize(i.worldNormal);
                float3 L = normalize(_MainLightPosition.xyz);
                float NdotL = dot(N, L);

                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float rim = 1.0 - saturate(dot(N, viewDir));
                rim = pow(rim, _RimPower);

                float3 lightIntensity = NdotL > 0.5 ? _MainColor.rgb : _ShadowColor.rgb;
                lightIntensity = lerp(lightIntensity, _HighlightColor.rgb, pow(saturate(NdotL), _SpecularPower));

                float heightT = saturate((i.worldPos.y - _GradientMinY) / max(0.0001, (_GradientMaxY - _GradientMinY)));
                float3 gradientColor = lerp(_GradientBottom.rgb, _GradientTop.rgb, heightT);

                float4 texColor = float4(1, 1, 1, 1);
                float4 overlayColor = float4(0, 0, 0, 0);
                float3 finalColor;

                if (i.objectNormal.y > 0.9)
                {
                    texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _MainTexTint;
                    overlayColor = SAMPLE_TEXTURE2D(_OverlayTex, sampler_OverlayTex, i.uv) * _OverlayTexTint;

                    float2 noiseUV = i.uv;
                    float noise = frac(sin(dot(noiseUV, float2(12.9898, 78.233))) * 43758.5453);
                    texColor.rgb *= lerp(1.0 - _NoiseStrength, 1.0 + _NoiseStrength, noise);

                    float edgeFalloff = smoothstep(0.4, 0.0, abs(i.uv.x - 0.5) + abs(i.uv.y - 0.5));
                    texColor.rgb *= lerp(1.0, 1.0 - _ShadowStrength, edgeFalloff);
                }

                float viewDot = dot(normalize(i.worldNormal), viewDir);
                float viewAngleEffect = pow(1.0 - saturate(viewDot), 2.0);
                float3 viewGradient = _ViewAngleGradientColor.rgb * viewAngleEffect * _ViewAngleGradientStrength;

                finalColor = gradientColor * lightIntensity * texColor.rgb;
                finalColor += overlayColor.rgb * overlayColor.a;
                finalColor += rim * _RimColor.rgb * _RimColor.a;
                finalColor += viewGradient;

                // 🔁 ADD Reflection from Cubemap
                float3 reflectDir = reflect(-viewDir, N);
                float3 reflectionSample = SAMPLE_TEXTURECUBE(_ReflectionTex, sampler_ReflectionTex, reflectDir);
                reflectionSample *= _ReflectionTint.rgb;

                reflectionSample = lerp(finalColor, reflectionSample, _ReflectionSmoothness);
                finalColor = lerp(finalColor, reflectionSample, _ReflectionStrength);

                return float4(finalColor, 1);
            }
            ENDHLSL
        }
    }
}
