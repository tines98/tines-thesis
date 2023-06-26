using System;
using UnityEngine;

namespace Demo{
    [Serializable]
    public class FluidDemoRenderSettings: ICloneable
    {
        [Header("Material Settings")]
        public Material fluidParticleMaterial;
        public Material boundaryParticleMaterial;
        public Material volumeMaterial;
        public Material cylinderMaterial;
    
        [Header("Mesh Settings")]
        public Mesh sphereMesh;
        public Mesh cylinderMesh;
    
        [Header("Gizmo Settings")]
        public bool drawGrid;
        public bool drawBoundaryParticles;
        public bool drawFluidParticles;
        public bool drawFluidVolume;
        public bool drawSimulationBounds;
        public bool drawBarChart;
        public GameObject floatingTextPrefab;
        public bool overrideEpsilon;
        [Range(1,300)]
        public float epsilon;

        public float deltaTime = 1f / 60f;

        public object Clone() => MemberwiseClone();
    }
}
