Shader "Unlit/RenderParticleDepth"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ParticleSize ("Particle Size", Float) = 0.1
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2g
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
            struct g2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 vpos : TEXCOORD1;
                float size : TEXCOORD2;
            };

            sampler2D _MainTex;
            StructuredBuffer<float4> particles;
            float4 _MainTex_ST;
            float _ParticleSize;

            // --------------------------------------------------------------------
            // Vertex Shader
            // --------------------------------------------------------------------
            v2g vert(uint id : SV_VertexID)
            {
              v2g o = (v2g) 0;
              float4 fp = particles[id];
              o.vertex = fp;
              return o;
            }

            // --------------------------------------------------------------------
            // Geometry Shader
            // --------------------------------------------------------------------
            // Position of each vertex of the point sprite
            static const float3 g_positions[4] =
            {
              float3(-1, 1, 0),
              float3( 1, 1, 0),
              float3(-1,-1, 0),
              float3( 1,-1, 0),
            };
            // UV coordinates of each vertex
            static const float2 g_texcoords[4] =
            {
              float2(0, 1),
              float2(1, 1),
              float2(0, 0),
              float2(1, 0),
            };

            [maxvertexcount(4)]
            void geom(point v2g In[1], inout TriangleStream<g2f> SpriteStream)
            {
              g2f o = (g2f) 0;
              // Position of the center vertex of the point sprite
              float3 vertpos = In[0].vertex.xyz;
              // 4 point sprites
              [unroll]
              for (int i = 0; i < 4; i++)
              {
                // Find and substitute the position of the point sprite in the clip coordinate system
                float3 pos = g_positions[i] * _ParticleSize;
                pos = mul(unity_CameraToWorld, pos) + vertpos;
                o.vertex = UnityObjectToClipPos(float4(pos, 1.0));
                // Substitute the UV coordinates of the point sprite vertices
                o.uv       = g_texcoords[i];
                // Find and substitute the position of the point sprite in the viewpoint coordinate system
                o.vpos = UnityObjectToViewPos(float4(pos, 1.0)).xyz * float3(1, 1, 1);
                // Substitute the size of the point sprite
                o.size     = _ParticleSize;

                SpriteStream.Append(o);
              }
              SpriteStream.RestartStrip();
            }

            // --------------------------------------------------------------------
            // Fragment Shader
            // --------------------------------------------------------------------
            struct fragmentOut
            {
              float  depthBuffer  : SV_Target0;
              float  depthStencil : SV_Depth;
            };

            fragmentOut frag(g2f i)
            {
              // Calculate normal
              float3 N = (float3) 0;
              N.xy = i.uv.xy * 2.0 - 1.0;
              float radius_sq = dot(N.xy, N.xy);
              if (radius_sq > 1.0) discard;
              N.z = sqrt(1.0 - radius_sq);

              // Pixel position in clip space
              float4 pixelPos     = float4(i.vpos.xyz + N * i.size, 1.0);
              float4 clipSpacePos = mul(UNITY_MATRIX_P, pixelPos);
              // depth
              float depth = clipSpacePos.z / clipSpacePos.w; // normalization

              fragmentOut o = (fragmentOut) 0;
              o.depthBuffer  = depth;
              o.depthStencil = depth;

              return o;
            }
            ENDCG
        }
    }
}
