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
        private FluidContainerizer fluidContainerizer;
        private VoxelizerDemo voxelizerDemo;
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

    
        /// <summary> Creates fluid particles </summary>
        private void CreateParticles(){
            start = true;
            voxels = fluidContainerizer.InteriorVoxels;
            ParticleSource = new ParticlesFromVoxels(FluidDemo.Radius() * 2, 
                                                     voxels, 
                                                     transform.localToWorldMatrix);
            ParticleSource.CreateParticles();
        
            LoggingUtility.LogInfo($"FluidVoxels {name} har a total of {ParticleSource.NumParticles} fluid particles!");
        }
    
        private void Update(){
            if (start) return;
            if (fluidContainerizer.IsReady()) CreateParticles();
        }

        private void OnDrawGizmos(){
            Gizmos.color = Color.blue;
            if (drawGizmo) DrawFluidVoxelsGizmo();
        }
    
        private void DrawFluidVoxelsGizmo() => voxels.ForEach(voxel => Gizmos.DrawWireCube(voxel.Center, 
                                                                  voxel.Size));
    }
}
