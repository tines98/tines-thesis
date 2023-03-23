// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Particle2"{
    Properties{
        _MainTex ("Texture", 2D) = "white" {}
    	_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
    	_Depth ("Depth", Range(0,1)) = 0.5
    	_DepthNormal ("Depth", Range(0,1)) = 1
    	_Color ("Color", Color) = (1,1,1)
    }
    CGINCLUDE
	bool IsInDeathBox(float4 pos, float4 deathPlanePosition, float4 deathPlaneSize){
		float4 comparePos = pos - deathPlanePosition;
		float4 halfDeathPlaneSize = deathPlaneSize/2.0f;
		return comparePos.x > -halfDeathPlaneSize.x &&
			   comparePos.y >  0 &&
			   comparePos.z > -halfDeathPlaneSize.z &&
			   comparePos.x <  halfDeathPlaneSize.x &&
			   comparePos.y <  deathPlaneSize.y &&
			   comparePos.z <  halfDeathPlaneSize.z;
	}
	ENDCG
    SubShader{
        Tags { "RenderType"="Opaque" "Queue" = "Geometry"}
        LOD 200
    	ZWrite On

        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma target 4.5
            #include "UnityCG.cginc"
			#include "UnityInstancing.cginc"

            #if SHADER_TARGET >= 45
				StructuredBuffer<float4> positions;
				StructuredBuffer<int> particles2Boundary;
			#endif

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f{
            	float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            	float4 screenPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            	float depth : SV_Depth;
            };

            struct instancedData{
	            float4x4 unity_ObjectToWorld;
	            float4x4 unity_WorldToObject;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
			float diameter;
			float4x4 boundaryMatrices[8];
			int useMatrix;
			float4 deathPlanePosition;
			float4 deathPlaneSize;
            half _Glossiness;
			half _Metallic;
			half _Depth;
			half _DepthNormal;

            v2f vert(appdata v, uint instanceID : SV_InstanceID){
	            float4 particle_center = positions[instanceID];
				float d = diameter;
				
    //             unity_ObjectToWorld._11_21_31_41 = float4(d, 0, 0, 0);
				// unity_ObjectToWorld._12_22_32_42 = float4(0, d, 0, 0);
				// unity_ObjectToWorld._13_23_33_43 = float4(0, 0, d, 0);
				// unity_ObjectToWorld._14_24_34_44 = float4(particle_center.xyz, 1);
				//
				// unity_WorldToObject = unity_ObjectToWorld;
				// unity_WorldToObject._14_24_34 *= -1;
				// unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
            	
            	v2f o;
            	o.vertex = v.vertex * d + particle_center;
            	o.vertex = UnityObjectToClipPos(o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeGrabScreenPos(o.vertex);
            	o.depth = o.screenPos.z / o.screenPos.w;
            	// Calculate the world normal
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                // Calculate the screen position and depth
            	
                return o;
            }

            fixed4 frag(v2f i): SV_Target{
                // if (_DepthNormal > 0) {
                //     float3 normal = normalize(i.worldNormal);
                //     float2 depthNormal = (normal.xy * 0.5 + 0.5) * normal.z;
                //     float depthNormalValue = depthNormal.x + depthNormal.y * 256.0;
                //     depthValue = lerp(depthValue, depthNormalValue, _DepthNormal);
                // }
            	UNITY_TRANSFER_DEPTH(i);
            	return tex2D(_MainTex, i.uv) * _Color * _Depth;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
