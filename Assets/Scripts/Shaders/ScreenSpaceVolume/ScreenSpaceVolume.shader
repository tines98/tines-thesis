Shader "Hidden/ScreenSpaceVolume"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
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

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthNormalsTexture);
            sampler2D _MainTex;
            // sampler2D _CameraDepthTexture;

            float diffuse(float3 normal, float3 ray_dir){
			    return max(0.0, dot(normal, ray_dir));
		    }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 depthNormalEncoded = tex2D(_CameraDepthNormalsTexture, i.uv);
                float4 main = tex2D(_MainTex, i.uv);
                float depth;    
                float3 normal;
                DecodeDepthNormal(depthNormalEncoded, depth, normal);
                float diff = diffuse(normal, float3(0,0,-1));
                return depth;//main * (1-diff);
            }
            ENDCG
        }
    }
}
