Shader "Hidden/SplitCamera"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PerspectiveTex ("Perspective Texture", 2D) = "white" {}
        _OrthographicTex ("Orthographic Texture", 2D) = "white" {}
        _SplitHeight ("Split Height", Range(0.0, 1.0)) = 0.5
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
            sampler2D _PerspectiveTex;
            sampler2D _OrthographicTex;
            float _SplitHeight;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col;
                // fixed4 background = tex2D(_MainTex, i.uv);
                if (i.uv.y > _SplitHeight){
                    col = tex2D(_PerspectiveTex, i.uv);
                }
                else{
                    col = tex2D(_OrthographicTex, i.uv);
                }

                return col;// * col.a + background * (1-col.a);
            }
            ENDCG
        }
    }
}
