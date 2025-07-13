Shader "URP/HighlightSweepOverlay_XY_FullControl"
{
    Properties
    {
        [HDR] _GlowColor ("Glow Color (HDR)", Color) = (10, 5, 1, 1)

        _GlowSizeX ("Glow Size X", Range(0,1)) = 0.2
        _GlowWidthX ("Glow Edge Softness X", Range(0.001, 0.5)) = 0.05
        _GlowSpeedX ("Glow Speed X", Float) = 1.0
        _AlphaX ("Alpha X", Range(0,1)) = 1.0

        _GlowSizeY ("Glow Size Y", Range(0,1)) = 0.2
        _GlowWidthY ("Glow Edge Softness Y", Range(0.001, 0.5)) = 0.05
        _GlowSpeedY ("Glow Speed Y", Float) = 1.0
        _AlphaY ("Alpha Y", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+10" }
        ZWrite Off
        Blend SrcAlpha One
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Properties
            float4 _GlowColor;

            float _GlowSizeX;
            float _GlowWidthX;
            float _GlowSpeedX;
            float _AlphaX;

            float _GlowSizeY;
            float _GlowWidthY;
            float _GlowSpeedY;
            float _AlphaY;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.uv = IN.uv;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float time = _Time.y;

                // -------------------------
                // Glow X
                float centerX = frac(time * _GlowSpeedX); // בין 0 ל-1
                float minX = centerX - _GlowSizeX * 0.5;
                float maxX = centerX + _GlowSizeX * 0.5;
                float glowX = smoothstep(minX - _GlowWidthX, minX, IN.uv.x) * 
                              (1.0 - smoothstep(maxX, maxX + _GlowWidthX, IN.uv.x)) * _AlphaX;

                // -------------------------
                // Glow Y
                float centerY = frac(time * _GlowSpeedY);
                float minY = centerY - _GlowSizeY * 0.5;
                float maxY = centerY + _GlowSizeY * 0.5;
                float glowY = smoothstep(minY - _GlowWidthY, minY, IN.uv.y) * 
                              (1.0 - smoothstep(maxY, maxY + _GlowWidthY, IN.uv.y)) * _AlphaY;

                float totalGlow = glowX + glowY;

                return float4(_GlowColor.rgb, totalGlow);
            }
            ENDHLSL
        }
    }
}
