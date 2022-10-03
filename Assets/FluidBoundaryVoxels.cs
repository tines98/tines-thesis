using System;
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
        voxels = fluidContainerizer.ExteriorVoxels;
        ParticleSource = new ParticlesFromVoxels(fluidBodyMeshDemo.Radius(), voxels, transform.localToWorldMatrix);
        ParticleSource.CreateParticles();
        
        Debug.Log($"Boundary Particles for object {name} is {ParticleSource.NumParticles}");
        voxelizerDemo.HideVoxelizedMesh();
    }

    private void Update()
    {
        if (start) return;
        if (fluidContainerizer.IsReady()) CreateParticles();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (drawGizmo) voxels.ForEach(voxel => Gizmos.DrawWireCube(voxel.Center,voxel.Size));
    }
}
