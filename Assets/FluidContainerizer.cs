using System;
using System.Collections;
using System.Collections.Generic;
using MeshVoxelizer.Scripts;
using PBDFluid;
using PBDFluid.Scripts;
using UnityEngine;
using UnityEngine.Assertions;

public class FluidContainerizer : MonoBehaviour
{
    private VoxelizerDemo voxelizerDemo;
    private MeshHollower meshHollower;
    
    public List<Box3> ExteriorVoxels;
    public List<Box3> InteriorVoxels;

    // Start is called before the first frame update
    void Start()
    {
        voxelizerDemo = GetComponentInParent<VoxelizerDemo>();
            
        meshHollower = new MeshHollower(voxelizerDemo.Voxelizer.Voxels);
        
        CalculateExterior();
        CalculateInterior();
        
        Assert.IsTrue(ExteriorVoxels.Count > 0, "Exterior is empty");
        Assert.IsTrue(InteriorVoxels.Count > 0, "Interior is empty");
        Assert.IsTrue(IsReady(),"IsReady is implemented wrong");
    }

    public bool IsReady() => ExteriorVoxels.Count > 0 && InteriorVoxels.Count > 0;
    
    private void CalculateExterior()
    {
        ExteriorVoxels = new List<Box3>();
        for (var z = 0; z < meshHollower.HullVoxels.GetLength(2); z++)
            for (var y = 0; y < meshHollower.HullVoxels.GetLength(1); y++)
                for (var x = 0; x < meshHollower.HullVoxels.GetLength(0); x++)
                    if (meshHollower.HullVoxels[x, y, z])
                        ExteriorVoxels.Add(voxelizerDemo.GetVoxel(x,y,z));
    }

    private void CalculateInterior() {
        InteriorVoxels = new List<Box3>();
        for (var z = 0; z < meshHollower.Visited.GetLength(2); z++)
            for (var y = 0; y < meshHollower.Visited.GetLength(1); y++)
                for (var x = 0; x < meshHollower.Visited.GetLength(0); x++)
                    if (!meshHollower.Visited[x, y, z])
                        InteriorVoxels.Add(voxelizerDemo.GetVoxel(x,y,z));
    }

    private void OnDrawGizmos()
    {
        DrawInteriorVoxelsGizmo();
        DrawExteriorVoxelsGizmo();
    }
    
    //GIZMOS
    private void DrawInteriorVoxelsGizmo() => 
        InteriorVoxels.ForEach(voxel => Gizmos.DrawWireCube(voxel.Center,voxel.Size));

    private void DrawExteriorVoxelsGizmo() => 
        ExteriorVoxels.ForEach(voxel => Gizmos.DrawWireCube(voxel.Center,voxel.Size));
    
    
    
}
