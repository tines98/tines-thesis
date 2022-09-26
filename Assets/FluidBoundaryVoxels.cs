using System.Collections;
using System.Collections.Generic;
using MeshVoxelizer.Scripts;
using PBDFluid;
using PBDFluid.Scripts;
using UnityEngine;
using UnityEngine.Assertions;

public class FluidBoundaryVoxels : FluidBoundaryObject
{
    [SerializeField] private bool drawGizmo;
    
    private VoxelizerDemo voxelizerDemo;
    private FluidContainerizer fluidContainerizer;
    private List<Box3> voxels;
    
    // Start is called before the first frame update
    void Start()
    {
        fluidContainerizer = GetComponent<FluidContainerizer>();
        Assert.IsNotNull(fluidContainerizer);
        
        fluidBodyMeshDemo = GetComponentInParent<FluidBodyMeshDemo>();
        Assert.IsNotNull(fluidBodyMeshDemo);
        
        voxelizerDemo = GetComponentInParent<VoxelizerDemo>();
        Assert.IsNotNull(voxelizerDemo);
        
        voxels = fluidContainerizer.exteriorVoxels;
        ParticleSource = new ParticlesFromVoxels(fluidBodyMeshDemo.Radius(), voxels);
        ParticleSource.CreateParticles();
        
        Debug.Log($"Boundary Particles for object {this.name} is {ParticleSource.NumParticles}");
        voxelizerDemo.HideVoxelizedMesh();
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (drawGizmo) voxels.ForEach(voxel => Gizmos.DrawWireCube(voxel.Center,voxel.Size));
    }
}
