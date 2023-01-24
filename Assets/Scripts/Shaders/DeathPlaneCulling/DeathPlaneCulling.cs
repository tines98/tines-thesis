using Demo;
using UnityEngine;
using UnityEngine.Assertions;

namespace Shaders.DeathPlaneCulling{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class DeathPlaneCulling : MonoBehaviour{
        private FluidDemo demo;
        private MeshRenderer meshRenderer;
        private Material material;
        
        private static readonly int DeathPlanePosition = UnityEngine.Shader.PropertyToID("_DeathPlanePosition");
        private static readonly int DeathPlaneSize = UnityEngine.Shader.PropertyToID("_DeathPlaneSize");

        // Start is called before the first frame update
        void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            material = meshRenderer.material;
            demo = GetComponentInParent<FluidDemo>();
            Assert.IsNotNull(demo);
        }

        private void UpdateMaterial(){
            var deathPlane = demo.DeathPlane;
            material.SetVector(DeathPlanePosition, deathPlane.transform.position);
            material.SetVector(DeathPlaneSize,deathPlane.size);
        }
    
        // Update is called once per frame
        void Update(){
            if (demo.DeathPlane == null) return; 
            UpdateMaterial();
        }
    }
}
