Shader "JinYanMaoYing/TaiHeReveal"
{
    Properties
    {
        _MainTex ("Scene", 2D) = "white" {}
        _RevealX ("Reveal X", Range(0,1)) = 0.5
        _RevealY ("Reveal Y", Range(0,1)) = 0.5
        _RevealRadius ("Reveal Radius", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _RevealX, _RevealY, _RevealRadius;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                float gray = dot(c.rgb, float3(0.299, 0.587, 0.114));
                float3 ink = lerp(float3(gray, gray, gray), float3(0.79, 0.76, 0.70), 0.12);
                float d = distance(i.uv, float2(_RevealX, _RevealY));
                float reveal = _RevealRadius <= 0.001 ? 0.0 :
                    smoothstep(d - 0.045, d + 0.045, _RevealRadius);
                c.rgb = lerp(ink, c.rgb, reveal);
                return c;
            }
            ENDCG
        }
    }
}
