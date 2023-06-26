Shader "Hidden/disk"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Radius", Float) = 0.1
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
            StructuredBuffer<float4> positions;
            int particle_count;
            float _Radius;
            float near_plane;
            float far_plane;
            
            float dist_squared_between(float2 a, float2 b){
                const float2 ab = b-a;
                return dot(ab,ab);
            }

            bool is_within(float2 uv, float2 pos){
                return dist_squared_between(uv,pos) < _Radius * _Radius;
            }

            float3 get_particle(int index){
                const float4 clip_pos = mul(unity_MatrixVP, positions[index]);
                const float2 device_xy = clip_pos.xy/clip_pos.w;
                float2 uv = 0.5 + device_xy * 0.5;
                float depth = (clip_pos.w-near_plane)/far_plane; 
                return float3(uv,depth);
            }
            
            float get_depth(float2 uv){
                float depth = 2;
                for (int i=0; i < particle_count; i++){
                    const float3 particle = get_particle(i);
                    if (is_within(uv,particle.xy)){
                        if (particle.z < depth)
                            depth = particle.z;
                    }
                }
                return depth;
            }

            fixed4 frag (v2f i) : SV_Target{
                float depth = 1-get_depth(i.uv);
                fixed4 col;
                col = fixed4(depth,depth,depth,0); 
                return col;
            }
            ENDCG
        }
    }
}
