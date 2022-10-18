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
    private bool start;
    
    // Start is called before the first frame update
    void Start()
    {
        fluidContainerizer = GetComponent<FluidContainerizer>();
        Assert.IsNotNull(fluidContainerizer);
        
        FluidBodyMeshDemo = GetComponentInParent<FluidBodyMeshDemo>();
        Assert.IsNotNull(FluidBodyMeshDemo);
        
        voxelizerDemo = GetComponentInParent<VoxelizerDemo>();
        Assert.IsNotNull(voxelizerDemo);
    }

    /// <summary> Creates boundary particles </summary>
    private void CreateParticles()
    {
        start = true;
        voxels = fluidContainerizer.ExteriorVoxels;
        ParticleSource = new ParticlesFromVoxels(FluidBodyMeshDemo.Radius() * 2, voxels, transform.localToWorldMatrix);
        ParticleSource.CreateParticles();
        
        Debug.Log($"Boundary Particles for object {name} is {ParticleSource.NumParticles}");
        // voxelizerDemo.HideVoxelizedMesh();
    }

    private void Update()
    {
        if (start) return;
        if (fluidContainerizer.IsReady()) CreateParticles();
    }

    private void OnDrawGizmos()
    {
        var localToWorldMatrix = transform.localToWorldMatrix;
        Gizmos.color = Color.red;
        if (drawGizmo) voxels.ForEach(voxel => 
            Gizmos.DrawWireCube( 
                localToWorldMatrix.MultiplyPoint(voxel.Center),
                  localToWorldMatrix.MultiplyVector(voxel.Size)
            )
        );
    }
}
