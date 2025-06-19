Shader "Unlit/Platform_SideTop_Gradient"
{
    Properties
    {
        _TopBottomColorStart ("Top/Bottom Color Start", Color) = (1,1,1,1)
        _TopBottomColorEnd ("Top/Bottom Color End", Color) = (0.5,0.5,0.5,1)
        _SideColorStart ("Side Color Start", Color) = (0,0,0,1)
        _SideColorEnd ("Side Color End", Color) = (0.2,0.2,0.2,1)

        _TopTexture ("Top Arrow Texture", 2D) = "white" {}
        _TextureTint ("Texture Tint", Color) = (1,1,1,1)
        _BumpMap ("Top Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Float) = 1

        _Threshold ("Top Face Threshold", Float) = 0.999
        _GradientMinY ("Gradient Min Y", Float) = 0
        _GradientMaxY ("Gradient Max Y", Float) = 1

        _Animate ("Enable Animation", Float) = 1
        _CycleDuration ("Cycle Duration (sec)", Float) = 1

        _ObjectScale ("Object Scale (XYZ)", Vector) = (1,1,1,1)

        _TopZOffset ("Top Texture Z Offset", Range(0, 0.1)) = 0.01
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Offset -1, -1
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _TopBottomColorStart;
            fixed4 _TopBottomColorEnd;
            fixed4 _SideColorStart;
            fixed4 _SideColorEnd;

            sampler2D _TopTexture;
            float4 _TopTexture_ST;
            fixed4 _TextureTint;

            sampler2D _BumpMap;
            float4 _BumpMap_ST;
            float _BumpScale;

            float _Threshold;
            float _GradientMinY;
            float _GradientMaxY;

            float _Animate;
            float _CycleDuration;

            float4 _ObjectScale;
            float _TopZOffset;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 localPos : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;

                float3 normalDir = UnityObjectToWorldNormal(v.normal);
                float upDot = dot(normalize(normalDir), float3(0, 1, 0));
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

                // רק לפאה העליונה נזיז כלפי מעלה
                if (upDot >= 0.999)
                {
                    worldPos.xyz += float3(0, _TopZOffset, 0);
                }

                o.pos = UnityWorldToClipPos(worldPos);
                o.worldPos = worldPos.xyz;
                o.worldNormal = normalDir;
                o.localPos = v.vertex.xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float yNormalized = saturate((i.worldPos.y - _GradientMinY) / (_GradientMaxY - _GradientMinY));
                float3 normalDir = normalize(i.worldNormal);
                float upDot = dot(normalDir, float3(0,1,0));

                // ----- TOP FACE ONLY -----
                if (upDot >= 0.999)
                {
                    fixed4 gradColor = lerp(_TopBottomColorStart, _TopBottomColorEnd, yNormalized);

                    float2 uv = i.localPos.xz;
                    float2 size = max(_ObjectScale.xz, float2(0.001, 0.001));
                    float2 uvNorm = uv / size + 0.5;

                    // animate texture
                    if (_Animate > 0.5)
                    {
                        uvNorm.y = frac(uvNorm.y + (_Time.y / _CycleDuration));
                    }

                    float2 texUV = saturate(uvNorm) * _TopTexture_ST.xy + _TopTexture_ST.zw;
                    fixed4 tex = tex2D(_TopTexture, texUV) * _TextureTint;

                    if (tex.a < 0.01)
                        tex.rgb = gradColor.rgb;

                    float2 bumpUV = saturate(uvNorm) * _BumpMap_ST.xy + _BumpMap_ST.zw;
                    float3 bumpNormal = UnpackNormal(tex2D(_BumpMap, bumpUV));
                    bumpNormal.xy *= _BumpScale;
                    bumpNormal.z = sqrt(1.0 - saturate(dot(bumpNormal.xy, bumpNormal.xy)));
                    bumpNormal = normalize(bumpNormal);

                    float3 lightDir = normalize(float3(0.3, 1, 0.5));
                    float light = saturate(dot(bumpNormal, lightDir));

                    fixed4 arrowColor = lerp(gradColor, tex, tex.a) * light;
                    return arrowColor;
                }

                // ----- BOTTOM FACE -----
                else if (abs(upDot) > _Threshold)
                {
                    return lerp(_TopBottomColorStart, _TopBottomColorEnd, yNormalized);
                }

                // ----- SIDES -----
                else
                {
                    return lerp(_SideColorStart, _SideColorEnd, yNormalized);
                }
            }
            ENDCG
        }
    }

    FallBack "Unlit/Transparent"
}
