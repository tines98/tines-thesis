using System.Collections.Generic;
using Demo;
using MeshVoxelizer.Scripts;
using PBDFluid.Scripts;
using UnityEngine;
using UnityEngine.Assertions;
using Utility;

namespace SimulationObjects.FluidObject{
    public class FluidVoxels : FluidObject{
        [SerializeField] private bool drawGizmo;
        [SerializeField] private float drawGizmoRadius;
        private FluidContainerizer fluidContainerizer;
        private VoxelizedMesh voxelizerDemo;
        private List<Box3> voxels;
        private bool start;
    
        // Start is called before the first frame update
        void Start(){
            fluidContainerizer = GetComponent<FluidContainerizer>();
            Assert.IsNotNull(fluidContainerizer);
        
            FluidDemo = GetComponentInParent<FluidDemo>();
            Assert.IsNotNull(FluidDemo);
        
            voxelizerDemo = GetComponentInParent<VoxelizedMesh>();
            if (voxelizerDemo == null) voxelizerDemo = GetComponent<VoxelizedMesh>();
            Assert.IsNotNull(voxelizerDemo);
        }

    
        /// <summary> Creates fluid particles </summary>
        private void CreateParticles(){
            start = true;
            voxels = fluidContainerizer.InteriorVoxels;
            ParticleSource = new ParticlesFromVoxels(FluidDemo.Radius * 2,
                                                     voxels,
                                                     voxelizerDemo.realVolume,
                                                     voxelizerDemo.realVolume > 0f, 
                                                     Matrix4x4.identity);//transform.localToWorldMatrix);
            ParticleSource.CreateParticles();
        
            // LoggingUtility.LogInfo($"FluidVoxels {name} har a total of {ParticleSource.NumParticles} fluid particles!");
        }
    
        private void Update(){
            if (start) return;
            if (fluidContainerizer.IsReady) CreateParticles();
        }

        private void OnDrawGizmos(){
            Gizmos.color = Color.blue;
            if (drawGizmo) DrawFluidVoxelsGizmo(Matrix4x4.identity);
            // if (!drawGizmo) DrawFluidVoxelsGizmo2(Matrix4x4.identity);
        }
    
        private void DrawFluidVoxelsGizmo(Matrix4x4 trs) => 
            voxels.ForEach(voxel => Gizmos.DrawWireSphere(voxel.Center, drawGizmoRadius));
    }
}
