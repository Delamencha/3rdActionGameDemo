Shader "Custom/HitFlashFresnel_URP"
{
    Properties
    {
        _FlashColor ("Flash Color", Color) = (1,1,1,1)
        _FlashIntensity ("Flash Intensity", Float) = 5
        _FresnelPower ("Fresnel Power", Float) = 5
        _HitFlash ("Hit Flash", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }

        Pass
        {
            Name "HitFlash"
            Tags { "LightMode"="UniversalForward" }

            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Back

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
                float3 normalWS    : TEXCOORD0;
                float3 viewDirWS   : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _FlashColor;
                float  _FlashIntensity;
                float  _FresnelPower;
                float  _HitFlash;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetWorldSpaceViewDir(positionWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(IN.viewDirWS);

                float ndv = saturate(dot(N, V));
                float fresnel = pow(1.0 - ndv, max(0.0001, _FresnelPower));

                float strength = saturate(_HitFlash) * fresnel * max(0.0, _FlashIntensity);
                float3 col = _FlashColor.rgb * strength;

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}


