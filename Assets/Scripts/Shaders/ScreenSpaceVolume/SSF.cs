using UnityEngine;

namespace Shaders.ScreenSpaceVolume{
    public class SSF{

        private readonly ComputeShader shader;
        private static readonly int Positions = Shader.PropertyToID("positions");
        private const int Threads = 128;
        private bool didPrint = false;

        public SSF(){
            shader = Resources.Load("SSF") as ComputeShader;
        }

        public void CreatePointSprite(ComputeBuffer posBuffer, 
                                      ComputeBuffer ssPosBuffer, 
                                      int count,
                                      float radius,
                                      Camera cam, 
                                      RenderTexture texture){
            if (shader == null) return; 
            
            var kernel = shader.FindKernel("point_sprite");
            var groups = count / Threads;
            if (count % Threads != 0) groups++;
        
            var textureResolution = new Vector2(texture.width, texture.height);

            var pMatrix = cam.projectionMatrix;
            var vMatrix = cam.worldToCameraMatrix;
            shader.SetTexture(kernel, "render_texture", texture);
            shader.SetBuffer(kernel, "positions", posBuffer);
            shader.SetBuffer(kernel, "ss_positions", ssPosBuffer);
            shader.SetVector("texture_resolution", textureResolution);
            shader.SetMatrix("p_matrix", pMatrix);
            shader.SetMatrix("v_matrix", vMatrix);
            shader.SetFloat("radius", radius);
        
            shader.Dispatch(kernel, groups, 1, 1);
        }
    }
}
