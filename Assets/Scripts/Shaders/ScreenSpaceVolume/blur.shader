Shader "Hidden/blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurDistance ("Blur Distance", Float) = 0.1
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
            float _BlurDistance;
            
            float4 sample_depth(float2 uv){
                return tex2D(_MainTex, uv);
            }

            float4 box_blur(float2 uv, float dist){
                float3 a = float3(dist,0,-dist);
                const float2 ct = uv + a.yx;
                const float2 rt = uv + a.xx;
                const float2 rc = uv + a.xy;
                const float2 rb = uv + a.xz;
                const float2 cb = uv + a.yz;
                const float2 lb = uv + a.zz;
                const float2 lc = uv + a.zy;
                const float2 lt = uv + a.zx;

                const float4 sum = sample_depth(uv)
                                + sample_depth(ct)
                                + sample_depth(rt)
                                + sample_depth(rc)
                                + sample_depth(rb)
                                + sample_depth(cb)
                                + sample_depth(lb)
                                + sample_depth(lc)
                                + sample_depth(lt);
                
                return sum / 9.0;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = box_blur(i.uv, _BlurDistance);
                return col;
            }
            ENDCG
        }
    }
}
