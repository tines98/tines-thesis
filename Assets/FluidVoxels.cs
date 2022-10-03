using System;
using System.Collections;
using System.Collections.Generic;
using MeshVoxelizer.Scripts;
using PBDFluid;
using PBDFluid.Scripts;
using UnityEngine;
using UnityEngine.Assertions;

public class FluidVoxels : FluidObject
{
    [SerializeField] private bool drawGizmo;
    
    private FluidContainerizer fluidContainerizer;
    private VoxelizerDemo voxelizerDemo;
    private List<Box3> voxels;
    private bool start = false;
    
    // Start is called before the first frame update
    void Start()
    {
        fluidContainerizer = GetComponent<FluidContainerizer>();
        Assert.IsNotNull(fluidContainerizer);
        
        fluidBodyMeshDemo = GetComponentInParent<FluidBodyMeshDemo>();
        Assert.IsNotNull(fluidBodyMeshDemo);
        
        voxelizerDemo = GetComponentInParent<VoxelizerDemo>();
        Assert.IsNotNull(voxelizerDemo);
    }

    private void CreateParticles()
    {
        start = true;
        voxels = fluidContainerizer.InteriorVoxels;
        ParticleSource = new ParticlesFromVoxels(fluidBodyMeshDemo.Radius() * 2, voxels, transform.localToWorldMatrix);
        ParticleSource.CreateParticles();
        
        Debug.Log($"Fluid Particles for object {name} is {ParticleSource.NumParticles}");
        voxelizerDemo.HideVoxelizedMesh();
    }
    
    private void Update()
    {
        if (start) return;
        if (fluidContainerizer.IsReady()) CreateParticles();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (drawGizmo) voxels.ForEach(voxel => Gizmos.DrawWireCube( 
            transform.localToWorldMatrix *voxel.Center,
            transform.localToWorldMatrix*voxel.Size));
    }
}
