using UnityEngine;

namespace Shaders.Split_Camera{
    public class SplitCamera : MonoBehaviour{
        [SerializeField] private RenderTexture perspectiveTexture;
        [SerializeField] private RenderTexture orthographicTexture;
        [SerializeField] private Material material;
        [Range(0f,1f)] public float splitHeight = 0.5f;
    
        private static readonly int PerspectiveTex = Shader.PropertyToID("_PerspectiveTex");
        private static readonly int OrthographicTex = Shader.PropertyToID("_OrthographicTex");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int SplitHeight = Shader.PropertyToID("_SplitHeight");

        private void OnRenderImage(RenderTexture src, RenderTexture dest){
            material.SetTexture(PerspectiveTex, perspectiveTexture);
            material.SetTexture(OrthographicTex, orthographicTexture);
            material.SetTexture(MainTex, src);
            material.SetFloat(SplitHeight, splitHeight);
        
            Graphics.Blit(src, dest, material);
        }
    }
}
