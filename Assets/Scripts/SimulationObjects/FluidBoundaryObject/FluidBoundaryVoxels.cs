using System.Collections.Generic;
using MeshVoxelizer.Scripts;
using PBDFluid;
using PBDFluid.Scripts;
using UnityEngine;
using UnityEngine.Assertions;

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
        ParticleSource = new ParticlesFromVoxels(FluidDemo.Radius() * 2, 
                                                 voxels, 
                                                 transform.localToWorldMatrix);
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
        if (drawGizmo) DrawBoundaryVoxels();
    }
    
    private void DrawBoundaryVoxels() => voxels.ForEach(voxel => Gizmos.DrawWireCube(voxel.Center, 
                                                                                     voxel.Size));
}
