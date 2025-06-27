Shader "URP/BoostPlatform_GradientStarsArrows"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.2, 0.8, 0.2, 1)
        _TopGradientColor ("Top Gradient Color", Color) = (0.4, 1, 0.4, 1)
        _BottomGradientColor ("Bottom Gradient Color", Color) = (0.0, 0.3, 0.0, 1)
        _GradientStartY ("Gradient Start Y", Float) = 0.0
        _GradientEndY ("Gradient End Y", Float) = 1.0

        [HDR]_StarColor ("Star Color", Color) = (1, 1, 1, 1)
        _StarSize ("Star Size", Float) = 0.02
        _StarTwinkleSpeed ("Star Twinkle Speed", Float) = 5.0
        _StarDensity ("Star Density", Float) = 50.0
        [NoScaleOffset]_StarTex ("Star Texture", 2D) = "white" {}

        [NoScaleOffset]_ArrowTex ("Arrow Texture", 2D) = "white" {}
        _ArrowColor ("Arrow Color", Color) = (0.7, 1.0, 0.7, 1)
        [HDR]_ArrowGlowColor ("Arrow Glow Color", Color) = (1.0, 1.0, 1.0, 1)
        _ArrowGlowSpeed ("Arrow Glow Speed", Float) = 1.0
        _ArrowGlowDelay ("Arrow Glow Delay", Float) = 0.3
        _ArrowPaddingX ("Arrow Padding X", Float) = 0.1
        _ArrowPaddingY ("Arrow Padding Y", Float) = 0.1
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
                float3 worldNormal : TEXCOORD1;
            };

            float4 _BaseColor;
            float4 _TopGradientColor;
            float4 _BottomGradientColor;
            float _GradientStartY;
            float _GradientEndY;

            float4 _StarColor;
            float _StarSize;
            float _StarTwinkleSpeed;
            float _StarDensity;
            TEXTURE2D(_StarTex);
            SAMPLER(sampler_StarTex);

            TEXTURE2D(_ArrowTex);
            SAMPLER(sampler_ArrowTex);
            float4 _ArrowColor;
            float4 _ArrowGlowColor;
            float _ArrowGlowSpeed;
            float _ArrowGlowDelay;
            float _ArrowPaddingX;
            float _ArrowPaddingY;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.objectPos = v.positionOS.xyz;
                o.worldNormal = TransformObjectToWorldNormal(v.normalOS);
                o.positionHCS = TransformWorldToHClip(TransformObjectToWorld(v.positionOS.xyz));
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);

                float direction = sign(_GradientEndY - _GradientStartY);
                float gradientT = saturate((i.objectPos.y - _GradientStartY) / max(0.0001, abs(_GradientEndY - _GradientStartY)));
                if (direction < 0) gradientT = 1.0 - gradientT;

                float4 color = lerp(_BottomGradientColor, _TopGradientColor, gradientT);

                if (normal.y > 0.9)
                {
                    float2 uv = i.objectPos.xz * 0.5 + 0.5;

                    // Padded UV
                    float2 paddedUV = float2(
                        lerp(_ArrowPaddingX, 1 - _ArrowPaddingX, uv.x),
                        lerp(_ArrowPaddingY, 1 - _ArrowPaddingY, uv.y)
                    );

                    // Stars
                    float starSum = 0;
                    for (int j = 0; j < 100; j++)
                    {
                        if (j >= _StarDensity) break;
                        float2 pos = float2(frac(sin(j * 12.9898) * 43758.5453), frac(cos(j * 78.233) * 12345.678));
                        float d = length(paddedUV - pos);
                        float twinkle = 0.5 + 0.5 * sin(_Time.y * _StarTwinkleSpeed + j);
                        float star = smoothstep(_StarSize * twinkle, 0.0, d);

                        float2 texUV = paddedUV - pos + 0.5;
                        float starTex = SAMPLE_TEXTURE2D(_StarTex, sampler_StarTex, texUV).r;
                        starSum += star * starTex;
                    }
                    color.rgb += _StarColor.rgb * starSum;

                    // Arrows: 3 fixed positions
                    for (int idx = 0; idx < 3; idx++)
                    {
                        float fy = (float)idx / 2.0;
                        float2 arrowUV = float2(paddedUV.x, paddedUV.y * 3.0 - fy);

                        float glowT = fmod(_Time.y, 3 * _ArrowGlowDelay);
                        float active = saturate(1.0 - abs(glowT - idx * _ArrowGlowDelay) / (_ArrowGlowDelay * 0.5));
                        float glowPhase = smoothstep(0.0, 1.0, active);

                        float4 tex = SAMPLE_TEXTURE2D(_ArrowTex, sampler_ArrowTex, arrowUV);
                        float4 arrow = lerp(_ArrowColor, _ArrowGlowColor, glowPhase);
                        color.rgb = lerp(color.rgb, arrow.rgb, tex.a);
                    }
                }

                return color;
            }
            ENDHLSL
        }
    }
}
