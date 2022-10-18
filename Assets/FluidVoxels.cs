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
        
        FluidBodyMeshDemo = GetComponentInParent<FluidBodyMeshDemo>();
        Assert.IsNotNull(FluidBodyMeshDemo);
        
        voxelizerDemo = GetComponentInParent<VoxelizerDemo>();
        Assert.IsNotNull(voxelizerDemo);
    }

    
    /// <summary> Creates fluid particles </summary>
    private void CreateParticles()
    {
        start = true;
        voxels = fluidContainerizer.InteriorVoxels;
        ParticleSource = new ParticlesFromVoxels(FluidBodyMeshDemo.Radius() * 2, voxels, transform.localToWorldMatrix);
        ParticleSource.CreateParticles();
        
        Debug.Log($"Fluid Particles for object {name} is {ParticleSource.NumParticles}");
        // voxelizerDemo.HideVoxelizedMesh();
    }
    
    private void Update()
    {
        if (start) return;
        if (fluidContainerizer.IsReady()) CreateParticles();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (drawGizmo)
        {
            var localToWorldMatrix = transform.localToWorldMatrix;
            voxels.ForEach(voxel => Gizmos.DrawWireCube(
                localToWorldMatrix.MultiplyPoint(voxel.Center), 
                  localToWorldMatrix.MultiplyVector(voxel.Size)
            ));
        }
    }
}
