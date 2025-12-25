Shader "Custom/WeaponTrailEdgeGlowURP"
{
    Properties
    {
        [HDR]_Color ("Glow Color", Color) = (1.0, 0.5, 0.15, 1)
        _Intensity ("Intensity", Range(0, 8)) = 2
        _Alpha ("Alpha", Range(0, 1)) = 0.8

        _TipEdgeWidth ("Tip Edge Width (UV)", Range(0.001, 0.5)) = 0.12
        _TipEdgePower ("Tip Edge Power", Range(0.2, 10)) = 3.0

        _LengthFadePower ("Length Fade Power", Range(0.2, 6)) = 1.0
        _HeadBoost ("Head Boost", Range(0, 2)) = 0.4

        _NoiseTex ("Noise (R)", 2D) = "white" {}
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.25
        _NoiseScroll ("Noise Scroll (UV/sec)", Vector) = (1, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalRenderPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            // Additive glow on top of distortion
            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);
            float4 _NoiseTex_ST;

            float4 _Color;
            float _Intensity;
            float _Alpha;
            float _TipEdgeWidth;
            float _TipEdgePower;
            float _LengthFadePower;
            float _HeadBoost;
            float _NoiseStrength;
            float4 _NoiseScroll;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0; // x=length, y=across (0 base, 1 tip)
                float4 color      : COLOR;     // alpha = segment fade from WeaponRibbonTrail
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = pos.positionCS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _NoiseTex);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Tip-only edge mask:
                // uv.y is across the ribbon (0 = TrailBase edge, 1 = TrailTip edge).
                // We only want glow near the TrailTip side (uv.y close to 1).
                float w = max(1e-5, _TipEdgeWidth);
                float tip01 = saturate((IN.uv.y - (1.0 - w)) / w); // 0..1 in the last 'w' band
                float edge = pow(tip01, _TipEdgePower);

                // Length fade (optional): keep head a bit stronger (uv.x close to 1)
                float len = saturate(IN.uv.x);
                float lengthFade = pow(len, _LengthFadePower);
                float headBoost = lerp(1.0, 1.0 + _HeadBoost, lengthFade);

                // Noise modulation
                float2 nuv = IN.uv + (_NoiseScroll.xy * _Time.y);
                float n = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, nuv).r;
                float noiseMul = lerp(1.0 - _NoiseStrength, 1.0, n);

                float vFade = saturate(IN.color.a);
                float a = saturate(_Alpha * vFade * edge);

                half3 rgb = (half3)_Color.rgb * (half)(_Intensity * headBoost * noiseMul);
                return half4(rgb, (half)a);
            }
            ENDHLSL
        }
    }
}


