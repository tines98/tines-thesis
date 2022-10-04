using System;
using System.Collections.Generic;
using MeshVoxelizer.Scripts;
using PBDFluid.Scripts;
using UnityEngine;
using UnityEngine.Assertions;

public class FluidContainerizer : MonoBehaviour
{
    [SerializeField] private bool drawGrid;
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
    
    private void CalculateExterior() {
        ExteriorVoxels = new List<Box3>();
        meshHollower.HullVoxels.ForEach(point => ExteriorVoxels.Add(voxelizerDemo.GetVoxel(point.X,point.Y,point.Z)));
    }

    private void CalculateInterior() {
        InteriorVoxels = new List<Box3>(voxelizerDemo.Voxels.Count);
        voxelizerDemo.Voxels.ForEach(voxel => InteriorVoxels.Add( new Box3( voxel.Min, voxel.Max)));
    }

    private void OnDrawGizmos()
    {
        if (!drawGrid) return;
        Gizmos.color = Color.grey;
        var voxels = meshHollower.voxels;
        var localToWorldMatrix = transform.localToWorldMatrix;
        for (var z = 0; z < voxels.GetLength(2); z++) {
            for (var y = 0; y < voxels.GetLength(1); y++) {
                for (var x = 0; x < voxels.GetLength(0); x++) {
                    if (voxels[x,y,z]==1) continue;
                    var box = voxelizerDemo.GetVoxel(x-1, y-1, z-1);
                    Gizmos.DrawWireCube(
                    localToWorldMatrix.MultiplyPoint(box.Center),
                      localToWorldMatrix.MultiplyVector(box.Size)
                    );
                }
            }
        }
    }
}
