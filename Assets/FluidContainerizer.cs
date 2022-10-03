using System.Collections.Generic;
using MeshVoxelizer.Scripts;
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
    
    private void CalculateExterior() {
        ExteriorVoxels = new List<Box3>();
        meshHollower.HullVoxels.ForEach(point => ExteriorVoxels.Add(voxelizerDemo.GetVoxel(point.X,point.Y,point.Z)));
    }

    private void CalculateInterior() => InteriorVoxels = voxelizerDemo.Voxels;
}
