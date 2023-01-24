using System.Collections.Generic;
using Demo;
using MeshVoxelizer.Scripts;
using UnityEngine;
using UnityEngine.Assertions;
using Utility;

namespace SimulationObjects.FluidBoundaryObject{
    public class FluidBoundaryVoxels : FluidBoundaryObject{
        [SerializeField] private bool drawGizmo;
        private VoxelizerDemo voxelizerDemo;
        private FluidContainerizer fluidContainerizer;
        private List<Box3> voxels;
        private bool start;
    
        // Start is called before the first frame update
        void Start(){
            fluidContainerizer = GetComponent<FluidContainerizer>();
            Assert.IsNotNull(fluidContainerizer);
        
            FluidDemo = GetComponentInParent<FluidDemo>();
            Assert.IsNotNull(FluidDemo);
        
            voxelizerDemo = GetComponentInParent<VoxelizerDemo>();
            if (voxelizerDemo == null) voxelizerDemo = GetComponent<VoxelizerDemo>();
            Assert.IsNotNull(voxelizerDemo);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary> Creates boundary particles </summary>
        private void CreateParticles(){
            start = true;
            voxels = fluidContainerizer.ExteriorVoxels;
            var particles = new List<Vector3>(voxels.Count);
            voxels.ForEach(voxel => particles.Add(voxel.Center));
            ParticleSource = new ParticlesFromList(FluidDemo.Radius(), particles, transform.localToWorldMatrix);
            ParticleSource.CreateParticles();
        
            LoggingUtility.LogInfo($"FluidBoundaryVoxels {name} har a total of {ParticleSource.NumParticles} boundary particles!");
            // voxelizerDemo.HideVoxelizedMesh();
        }

        private void Update(){
            if (start) return;
            if (fluidContainerizer.IsReady()) CreateParticles();
        }

        private void OnDrawGizmos(){
            Gizmos.color = Color.red;
            if (drawGizmo) DrawBoundaryVoxels(transform.localToWorldMatrix);
        }
    
        private void DrawBoundaryVoxels(Matrix4x4 trs) => voxels.ForEach(voxel => 
            Gizmos.DrawWireCube(trs.MultiplyPoint(voxel.Center), 
                                trs.MultiplyVector(voxel.Size)));
    }
}
