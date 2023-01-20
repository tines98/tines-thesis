// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DeathPlaneCulling"
{
    Properties 
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color (RGBA)", Color) = (1, 1, 1, 1) // add _Color property
        deathPlanePosition ("death plane position", Vector) = (0,0,0,1)
        deathPlaneSize ("death plane size", Vector) = (1,1,1,0)
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
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull front 
        LOD 100

        Pass 
        {
            Cull Off 
            CGPROGRAM

            #pragma vertex vert alpha
            #pragma fragment frag alpha

            #include "UnityCG.cginc"

            struct appdata_t 
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f 
            {
                float4 vertex  : SV_POSITION;
                half2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 deathPlanePosition;
            float4 deathPlaneSize;

            v2f vert (appdata_t v)
            {
                v2f o;
                float4 wp = mul(unity_ObjectToWorld,v.vertex);
                
                o.vertex     = UnityObjectToClipPos(v.vertex);
                v.texcoord.x = 1 - v.texcoord.x;
                o.texcoord   = TRANSFORM_TEX(v.texcoord, _MainTex);
                if (IsInDeathBox(wp,deathPlanePosition,deathPlaneSize))
                {
                    o.texcoord = half2(-1,-1);
                }
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = _Color;
                if (i.texcoord.x < 0)
                {
                    color.a = 0;
                }
                fixed4 col = tex2D(_MainTex, i.texcoord) * color; // multiply by _Color
                return col;
            }

            ENDCG
        }
    }
}
