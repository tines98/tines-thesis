
Shader "PBDFluid/Particle" 
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
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
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
		#pragma exclude_renderers d3d11 gles
		// Physically based Standard lighting model
		#pragma surface surf Standard addshadow fullforwardshadows
		#pragma multi_compile_instancing
		#pragma instancing_options procedural:setup

		sampler2D _MainTex;
		float4 color;
		float diameter;
		float4x4 boundaryMatrices[8];
		int useMatrix;
		float4 deathPlanePosition;
		float4 deathPlaneSize;

		struct Input 
		{
			float2 uv_MainTex;
			float4 color;
		};

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			StructuredBuffer<float4> positions;
			StructuredBuffer<int> particles2Boundary;
		#endif

		
		
		void setup()
		{
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				float4 pos;
				if (useMatrix > 0){
					int matrixIndex = particles2Boundary[unity_InstanceID];
					matrix <float, 4, 4> TSR = boundaryMatrices[matrixIndex];
					pos = mul(TSR, positions[unity_InstanceID]);
				} else {
					pos = positions[unity_InstanceID];
				}
				float d = diameter;
				if (IsInDeathBox(pos,deathPlanePosition,deathPlaneSize)){
					pos = float4(999,999,999,0);
					d = 0;
					return;
				}

				unity_ObjectToWorld._11_21_31_41 = float4(d, 0, 0, 0);
				unity_ObjectToWorld._12_22_32_42 = float4(0, d, 0, 0);
				unity_ObjectToWorld._13_23_33_43 = float4(0, 0, d, 0);
				unity_ObjectToWorld._14_24_34_44 = float4(pos.x,pos.y,pos.z, 1);

				unity_WorldToObject = unity_ObjectToWorld;
				unity_WorldToObject._14_24_34 *= -1;
				unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
			#endif
		}

		half _Glossiness;
		half _Metallic;

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			o.Albedo = color.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 1;
		}
		ENDCG
	}
}