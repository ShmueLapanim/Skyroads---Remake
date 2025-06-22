Shader "URP/RefuelPlatform_LightningFullCycle"
{
    Properties
    {
        _MainColor ("Base Color", Color) = (0.3, 0.0, 0.5, 1)
        _GradientColor1 ("Gradient Color 1", Color) = (0.0, 1.0, 1.0, 1)
        _GradientYStart1 ("Gradient 1 Y Start", Float) = 0.5
        _GradientColor2 ("Gradient Color 2", Color) = (0.0, 0.8, 1.0, 1)
        _GradientYStart2 ("Gradient 2 Y Start", Float) = 0.3
        _GradientColor3 ("Gradient Color 3", Color) = (0.0, 0.6, 1.0, 1)
        _GradientYStart3 ("Gradient 3 Y Start", Float) = 0.1
        _GradientColor4 ("Gradient Color 4", Color) = (0.0, 0.4, 1.0, 1)
        _GradientYStart4 ("Gradient 4 Y Start", Float) = -0.1

        _CircleColor ("Circle Color", Color) = (1, 0, 0, 1)
        _CircleCount ("Circle Count", Float) = 10
        _CircleSize ("Circle Size", Float) = 0.2
        _CircleSpeed ("Circle Speed", Float) = 1.0
        _CircleLifetime ("Circle Lifetime", Float) = 2.0

        [NoScaleOffset]_LightningTex ("Lightning Texture", 2D) = "black" {}
        [HDR]_LightningColor ("Lightning Color", Color) = (1, 1, 1, 1)
        _LightningFillDuration ("Lightning Fill Duration", Float) = 1.0
        _LightningHoldDuration ("Lightning Hold Duration", Float) = 1.0
        _LightningFadeDuration ("Lightning Fade Duration", Float) = 1.0

        _DistortedTex ("Distorted Texture", 2D) = "white" {}
        _DistortTexColor ("Distorted Texture Color", Color) = (0, 0, 0, 1)
        _DistortSpeed ("Distortion Speed", Float) = 1.0
        _DistortIntensity ("Distortion Intensity", Float) = 0.05
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
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 objectPos   : TEXCOORD0;
                float3 worldPos    : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

            float4 _MainColor;
            float4 _GradientColor1, _GradientColor2, _GradientColor3, _GradientColor4;
            float _GradientYStart1, _GradientYStart2, _GradientYStart3, _GradientYStart4;

            float4 _CircleColor;
            float _CircleCount;
            float _CircleSize;
            float _CircleSpeed;
            float _CircleLifetime;

            TEXTURE2D(_LightningTex);
            SAMPLER(sampler_LightningTex);
            float4 _LightningColor;
            float _LightningFillDuration;
            float _LightningHoldDuration;
            float _LightningFadeDuration;

            TEXTURE2D(_DistortedTex);
            SAMPLER(sampler_DistortedTex);
            float4 _DistortTexColor;
            float _DistortSpeed;
            float _DistortIntensity;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.objectPos = v.positionOS.xyz;
                o.worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.worldNormal = TransformObjectToWorldNormal(v.normalOS);
                o.positionHCS = TransformWorldToHClip(o.worldPos);
                return o;
            }

            float4 ApplyGradient(float3 worldPos)
            {
                float4 color = _MainColor;

                float height1 = saturate(1.0 - (worldPos.y - _GradientYStart1));
                float height2 = saturate(1.0 - (worldPos.y - _GradientYStart2));
                float height3 = saturate(1.0 - (worldPos.y - _GradientYStart3));
                float height4 = saturate(1.0 - (worldPos.y - _GradientYStart4));

                color = lerp(color, _GradientColor1, height1);
                color = lerp(color, _GradientColor2, height2);
                color = lerp(color, _GradientColor3, height3);
                color = lerp(color, _GradientColor4, height4);

                return color;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float4 baseColor = ApplyGradient(i.worldPos);

                if (normal.y > 0.9)
                {
                    float2 uv = i.objectPos.xz * 0.5 + 0.5;
                    float circleSum = 0;

                    [unroll(20)]
                    for (int c = 0; c < 20; c++)
                    {
                        if (c >= (int)_CircleCount) break;

                        float seed = dot(float2(c, c * 1.37), float2(12.9898, 78.233));
                        float birthOffset = frac(sin(seed) * 43758.5453);
                        float t = fmod(_Time.y + birthOffset * _CircleLifetime, _CircleLifetime);
                        float lifeProgress = t / _CircleLifetime;

                        float scale = sin(lifeProgress * 3.14159);

                        float2 posOffset = float2(
                            frac(sin(seed + t * _CircleSpeed) * 12345.6789),
                            frac(cos(seed + t * _CircleSpeed) * 98765.4321)
                        );
                        posOffset = posOffset * 0.8 + 0.1;

                        float2 motion = float2(sin(seed + t), cos(seed + t)) * 0.1;
                        float2 pos = posOffset + motion * lifeProgress;

                        float2 deform = float2(1.0 + 0.2 * sin(t + seed), 1.0 + 0.2 * cos(t + seed));
                        float2 delta = (uv - pos) * deform;
                        float dist = length(delta);
                        float circle = smoothstep(_CircleSize * scale, _CircleSize * scale * 0.8, dist);

                        circleSum += circle;
                    }

                    circleSum = saturate(circleSum);
                    baseColor.rgb = lerp(baseColor.rgb, _CircleColor.rgb, circleSum);
                    baseColor.a = lerp(baseColor.a, _CircleColor.a, circleSum);

                    float cycleTime = _LightningFillDuration + _LightningHoldDuration + _LightningFadeDuration;
                    float t = fmod(_Time.y, cycleTime);

                    float2 lightningUV = uv;
                    float4 lightningTex = SAMPLE_TEXTURE2D(_LightningTex, sampler_LightningTex, lightningUV);
                    float lightningMask = 0.0;

                    if (t < _LightningFillDuration)
                    {
                        float phase = t / _LightningFillDuration;
                        float fillMask = smoothstep(0.0, 1.0, phase - lightningUV.y);
                        lightningMask = lightningTex.r * fillMask;
                    }
                    else if (t < _LightningFillDuration + _LightningHoldDuration)
                    {
                        lightningMask = lightningTex.r;
                    }
                    else
                    {
                        float fadeT = (t - _LightningFillDuration - _LightningHoldDuration) / _LightningFadeDuration;
                        float fadeOut = 1.0 - saturate(fadeT);
                        lightningMask = lightningTex.r * fadeOut;
                    }

                    if (lightningMask > 0.01)
                    {
                        baseColor.rgb = lerp(baseColor.rgb, _LightningColor.rgb, lightningMask);
                        baseColor.a = lerp(baseColor.a, _LightningColor.a, lightningMask);
                    }

                    // 📦 Distorted texture overlay
                    float2 distortUV = uv + sin(float2(uv.y, uv.x) * 20 + _Time.y * _DistortSpeed) * _DistortIntensity;
                    float4 distortTex = SAMPLE_TEXTURE2D(_DistortedTex, sampler_DistortedTex, distortUV);
                    baseColor.rgb += distortTex.rgb * _DistortTexColor.rgb * distortTex.a;
                }

                return baseColor;
            }
            ENDHLSL
        }
    }
}
