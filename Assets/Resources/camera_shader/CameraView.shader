Shader "Custom/CameraView"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _QuantizeSteps ("Color Quantization Steps", Float) = 8
        _NoiseIntensity ("Noise Intensity", Range(0,1)) = 0.05
        _ScanlineIntensity ("Scanline Intensity", Range(0,1)) = 0.2
        _RGBOffset ("RGB Offset Amount", Float) = 0.002
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            // Blend SrcAlpha OneMinusSrcAlpha
            // Blend One One
            Blend DstColor Zero
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _QuantizeSteps;
            float _NoiseIntensity;
            float _ScanlineIntensity;
            float _RGBOffset;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // === RGB 偏移 ===
                float2 rUV = uv + float2(_RGBOffset * 2.0, 0);
                float2 gUV = uv;
                float2 bUV = uv - float2(_RGBOffset * 2.0, 0);

                float3 col;
                col.r = tex2D(_MainTex, rUV).r;
                col.g = tex2D(_MainTex, gUV).g;
                col.b = tex2D(_MainTex, bUV).b;

                // === 颜色量化 ===
                col = floor(col * _QuantizeSteps) / _QuantizeSteps;

                // === 加噪声 ===
                float noise = frac(sin(dot(uv * _Time.y, float2(12.9898,78.233))) * 43758.5453);
                col += (noise - 0.5) * _NoiseIntensity * 2.0;

                // === 扫描线 ===
                float scan = sin(uv.y * 100 + _Time.y * 20) * 0.5 + 0.5;
                col *= 1.0 - scan * _ScanlineIntensity;

                return fixed4(col, 0.5);
            }
            ENDCG
        }
    }
}
