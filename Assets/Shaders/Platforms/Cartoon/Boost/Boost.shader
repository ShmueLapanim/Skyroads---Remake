Shader "URP/CartoonBoostPlatform_FINALv12"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0, 1, 0, 1)

        _ArrowTex ("Arrow Texture", 2D) = "white" {}
        _ArrowColor ("Arrow Color", Color) = (1, 1, 1, 1)
        _ArrowScrollSpeed ("Arrow Scroll Speed", Float) = 1.0
        _ArrowGlow ("Arrow Glow Power", Float) = 1.5
        _ArrowCount ("Arrow Count", Float) = 3
        _ArrowSpacingX ("Arrow Spacing X", Float) = 1.0
        _ArrowSpacingY ("Arrow Spacing Y", Float) = 1.0
        _ArrowArea ("Arrow Area (0=centered)", Range(0,1)) = 0.2
        _ArrowSoftX ("Arrow Softness X", Float) = 0.05
        _ArrowSoftY ("Arrow Softness Y", Float) = 0.05

        _StarTex ("Star Texture", 2D) = "white" {}
        _StarColor ("Star Color", Color) = (1, 1, 1, 1)
        _StarCount ("Number of Stars", Float) = 10
        _StarSizeMin ("Min Star Size", Float) = 0.05
        _StarSizeMax ("Max Star Size", Float) = 0.15
        _StarLifeMin ("Min Star Life", Float) = 0.5
        _StarLifeMax ("Max Star Life", Float) = 2.0
        _StarRotMin ("Min Rotation Speed", Float) = 30
        _StarRotMax ("Max Rotation Speed", Float) = 120
        _StarGlow ("Star Glow Power", Float) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define MAX_STAR_COUNT 30

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;

            float4 _ArrowColor;
            float _ArrowScrollSpeed;
            float _ArrowGlow;
            float _ArrowCount;
            float _ArrowSpacingX;
            float _ArrowSpacingY;
            float _ArrowArea;
            float _ArrowSoftX;
            float _ArrowSoftY;

            float4 _StarColor;
            float _StarCount;
            float _StarSizeMin;
            float _StarSizeMax;
            float _StarLifeMin;
            float _StarLifeMax;
            float _StarRotMin;
            float _StarRotMax;
            float _StarGlow;
            CBUFFER_END

            TEXTURE2D(_ArrowTex);
            SAMPLER(sampler_ArrowTex);
            TEXTURE2D(_StarTex);
            SAMPLER(sampler_StarTex);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                if (IN.normalWS.y < 0.9)
                    return _BaseColor;

                // ======== STAR LAYER =========
                float4 starLayer = float4(0, 0, 0, 0);
                for (int i = 0; i < MAX_STAR_COUNT; i++)
                {
                    if (i >= (int)_StarCount) break;
                    float seed = i * 37.412;
                    float2 randPos = float2(
                        frac(sin(seed * 12.9898) * 43758.5453),
                        frac(cos(seed * 78.233) * 12345.6789)
                    );
                    float randLife = lerp(_StarLifeMin, _StarLifeMax, frac(sin(seed * 0.33) * 17.123));
                    float randSize = lerp(_StarSizeMin, _StarSizeMax, frac(sin(seed * 1.77) * 97.231));
                    float randRotSpeed = lerp(_StarRotMin, _StarRotMax, frac(sin(seed * 2.33) * 53.891));
                    float offsetTime = frac((_Time.y + seed * 0.13) / randLife);
                    float scale = sin(offsetTime * 3.14159);
                    float angle = offsetTime * randRotSpeed * 3.14159 / 180.0;
                    float2 localUV = (uv - randPos) / randSize;
                    localUV = float2(
                        localUV.x * cos(angle) - localUV.y * sin(angle),
                        localUV.x * sin(angle) + localUV.y * cos(angle)
                    );
                    localUV /= scale + 0.0001;
                    if (abs(localUV.x) <= 1 && abs(localUV.y) <= 1)
                    {
                        float4 starSample = SAMPLE_TEXTURE2D(_StarTex, sampler_StarTex, 0.5 + localUV);
                        float3 starRGB = starSample.rgb * starSample.a;
                        float4 glow = float4(starRGB, 1) * _StarColor * _StarGlow * scale;
                        starLayer += glow;
                    }
                }

                // ========== ARROW LAYER ==========
                float2 scrollOffset = float2(0, _Time.y * _ArrowScrollSpeed);
                float2 tiling = float2(_ArrowCount / _ArrowSpacingX, _ArrowCount / _ArrowSpacingY);

                float arrowUVx = (uv.x - 0.5) / (1.0 - _ArrowArea) + 0.5;
                float2 arrowUV = float2(arrowUVx, uv.y);

                float2 tiledUV = frac(arrowUV * tiling + scrollOffset);
                float4 arrowSample = SAMPLE_TEXTURE2D(_ArrowTex, sampler_ArrowTex, tiledUV);
                float arrowAlpha = arrowSample.a;

                // ריכוך מהקצוות פנימה
                float maskX = smoothstep(0.0, _ArrowSoftX, abs(uv.x - 0.5));
                float maskY = smoothstep(0.0, _ArrowSoftY, abs(tiledUV.y - 0.5));
                float mask = 1.0 - maskX * maskY;
                arrowAlpha *= mask;

                float4 arrowLayer = _ArrowColor * arrowAlpha * _ArrowGlow;

                // suppress stars under arrows
                starLayer *= (1.0 - saturate(arrowAlpha * 1.5));

                float4 finalColor = _BaseColor + starLayer + arrowLayer;
                return finalColor;
            }
            ENDHLSL
        }
    }
}
