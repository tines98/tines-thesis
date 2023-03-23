Shader "Hidden/DepthOnly"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off 
        ZWrite Off 
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthNormalsTexture);

            fixed4 frag (v2f i) : SV_Target
            {
                float4 depthNormalEncoded = tex2D(_CameraDepthNormalsTexture, i.uv);
                // float4 main = tex2D(_MainTex, i.uv);
                float depth = 0;    
                float3 normal = 0;
                DecodeDepthNormal(depthNormalEncoded, depth, normal);
                return depth;
            }
            ENDCG
        }
    }
}
