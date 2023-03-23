Shader "Hidden/SplitCamera" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _PerspectiveTex ("Perspective Texture", 2D) = "white" {}
        _OrthographicTex ("Orthographic Texture", 2D) = "white" {}
        _SplitHeight ("Split Height", Range(0.0, 1.0)) = 0.5
        _BlurZone ("Blur Zone", Range(0,0.25)) = 0.1
    }
    SubShader {
        GrabPass { "BackGroundTexture" }
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 grabPos : TEXCOORD1;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            sampler2D _MainTex;
            sampler2D _PerspectiveTex;
            sampler2D _OrthographicTex;
            half _SplitHeight;
            half _BlurZone;
            sampler2D BackGroundTexture;

            fixed4 split_camera(float2 uv) {
                const float frag_height = uv.y - _SplitHeight;
                // Perspective Camera
                if (frag_height > 0){
                    return tex2D(_PerspectiveTex, uv);
                }
                // Blur
                if (abs(frag_height)<_BlurZone){
                    const half bottom = _SplitHeight-_BlurZone;
                    const half t = (uv.y - bottom) / _BlurZone*2.0;
                    return tex2D(_OrthographicTex,  uv) * (1-t)
                         + tex2D(_PerspectiveTex, uv) * t;
                }
                //Orthographic
                return tex2D(_OrthographicTex, uv);
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = split_camera(i.uv);
                const float4 background = tex2D(_MainTex, i .uv);
                
                return col * col.a
                     + background * (1-col.a);
            }
            ENDCG
        }
    }
}
