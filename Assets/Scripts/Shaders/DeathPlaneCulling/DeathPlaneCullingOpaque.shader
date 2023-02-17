Shader "Custom/DeathPlaneCullingOpaque"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _DeathPlanePosition ("death plane position", Vector) = (0,0,0,1)
        _DeathPlaneSize ("death plane size", Vector) = (1,1,1,0)
        _InteriorDarkening ("Interior Darkening", Range(0,1)) = 0.5
        _CutLineColor ("Cut Line Color", Color) = (1,0,0,0)
        _CutLineLength ("Cut Line Size", Range(0,1)) = 0.1
        _Alpha ("Alpha", Range(0,1)) = 1
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
        //First pass with front culling
        Cull Front
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        half _InteriorDarkening;
        fixed4 _Color;
        float4 _DeathPlanePosition;
        float4 _DeathPlaneSize;
        fixed4 _CutLineColor;
        half _CutLineLength;
        half _Alpha;
        

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            
            o.Albedo = c.rgb  * _InteriorDarkening;;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            if (IsInDeathBox(float4(IN.worldPos,1), _DeathPlanePosition, _DeathPlaneSize + + float4(0,_CutLineLength,0,0)))
                o.Alpha = 0;
            else if (IsInDeathBox(float4(IN.worldPos,1), _DeathPlanePosition, _DeathPlaneSize + float4(0,_CutLineLength*2,0,0))){
                o.Alpha = 1;
                o.Albedo = _CutLineColor * _InteriorDarkening;
            }
            else {
                o.Alpha = c.a;
            }
        }
        ENDCG
        
        // Second Pass with Front culling
        Cull Back
        
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float4 _DeathPlanePosition;
        float4 _DeathPlaneSize;
        fixed4 _CutLineColor;
        half _CutLineLength;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            if (IsInDeathBox(float4(IN.worldPos,1), _DeathPlanePosition, _DeathPlaneSize + + float4(0,_CutLineLength,0,0)))
                o.Alpha = 0;
            else if (IsInDeathBox(float4(IN.worldPos,1), _DeathPlanePosition, _DeathPlaneSize + float4(0,_CutLineLength*2,0,0))){
                o.Alpha = 1;
                o.Albedo = _CutLineColor;
            }
            else
            {
                o.Alpha = c.a;
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}
