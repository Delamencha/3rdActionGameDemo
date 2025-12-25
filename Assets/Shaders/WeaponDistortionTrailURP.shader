Shader "Custom/WeaponDistortionTrailURP"
{
    Properties
    {
        [MainTexture]_DistortionTex ("Distortion (RG)", 2D) = "bump" {}
        _Strength ("Distortion Strength", Range(0, 0.1)) = 0.02
        _Alpha ("Alpha", Range(0, 1)) = 0.55
        _EdgePower ("Edge Power (higher = tighter edge)", Range(0.2, 8)) = 2.0
        _Scroll ("Distortion Scroll (UV units/sec)", Vector) = (1, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalRenderPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            TEXTURE2D(_DistortionTex);
            SAMPLER(sampler_DistortionTex);
            float4 _DistortionTex_ST;

            float _Strength;
            float _Alpha;
            float _EdgePower;
            float4 _Scroll;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
                float4 screenPos   : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = pos.positionCS;
                OUT.screenPos = ComputeScreenPos(pos.positionCS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _DistortionTex);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Screen UV for SceneColor sampling
                float2 screenUV = (IN.screenPos.xy / max(IN.screenPos.w, 1e-5));

                // Distortion UV scroll
                float2 duv = IN.uv + (_Scroll.xy * _Time.y);

                // Sample distortion: use RG as signed offset
                float2 d = SAMPLE_TEXTURE2D(_DistortionTex, sampler_DistortionTex, duv).rg;
                d = d * 2.0 - 1.0;

                // Edge mask: 0 at center (uv.y=0.5), 1 at edges (0 or 1)
                float edge = abs(IN.uv.y * 2.0 - 1.0);
                edge = pow(saturate(edge), _EdgePower);

                // Vertex alpha comes from WeaponRibbonTrail (segment age fade)
                float vFade = saturate(IN.color.a);

                float strength = _Strength * edge * vFade;
                float2 offset = d * strength;

                half3 scene = SampleSceneColor(screenUV + offset);
                half alpha = saturate(_Alpha * vFade);

                return half4(scene, alpha);
            }
            ENDHLSL
        }
    }
}


