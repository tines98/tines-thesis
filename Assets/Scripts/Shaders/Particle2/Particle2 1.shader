Shader "Custom/InstancedDepthWriteShaderWithNormals" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _DepthNormal ("Depth Normal", Range(0,1)) = 1
    }
    SubShader {
        Tags {"Queue"="Geometry" "RenderType"="Opaque"}
        ZWrite On
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #include "UnityCG.cginc"
            #pragma target 4.5

            struct instanceData {
                float4x4 unity_ObjectToWorld;
            };

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float4 vertex : SV_POSITION;
                float depth : SV_Depth;
            };

            #if SHADER_TARGET >= 45
				StructuredBuffer<float4> positions;
			#endif

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _DepthNormal;

            v2f vert (appdata v, uint id : SV_InstanceID) {
  
                v2f o;
                // Transform the vertex position to clip space
                o.vertex = positions[id] + v.vertex;
                // o.vertex = mul(unity_ObjectToWorld[id], v.vertex + positions[id]);
                o.vertex = UnityObjectToClipPos(o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // Calculate the world normal
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                // Calculate the screen position and depth
                o.screenPos = ComputeGrabScreenPos(o.vertex);
                o.depth = _DepthNormal;
                // Output the per-instance data
                UNITY_SETUP_INSTANCE_ID(id);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                UNITY_TRANSFER_DEPTH(i.screenPos.z / i.screenPos.w);
                return tex2D(_MainTex, i.uv);
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}