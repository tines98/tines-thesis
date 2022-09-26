using System.Collections;
using System.Collections.Generic;
using MeshVoxelizer.Scripts;
using PBDFluid;
using PBDFluid.Scripts;
using UnityEngine;

public class FluidContainerizer : MonoBehaviour
{
    public ParticleSource FluidParticleSource;
    public ParticleSource BoundaryParticleSource;
    
    private VoxelizerDemo voxelizerDemo;
    private MeshHollower meshHollower;
    
    private Bounds bounds;
    public List<Box3> exteriorVoxels;
    public List<Box3> interiorVoxels;
    
    // Start is called before the first frame update
    void Start()
    {
        voxelizerDemo = GetComponentInParent<VoxelizerDemo>();
        bounds = new Bounds(voxelizerDemo.Bounds.Center,voxelizerDemo.Bounds.Size);
            
        meshHollower = new MeshHollower(this.voxelizerDemo.Voxelizer.Voxels);
            
        CalculateExterior();
        CalculateInterior();
    }
    
    private void CalculateExterior()
    {
        exteriorVoxels = new List<Box3>();
        for (var z = 0; z < meshHollower.HullVoxels2.GetLength(2); z++)
            for (var y = 0; y < meshHollower.HullVoxels2.GetLength(1); y++)
                for (var x = 0; x < meshHollower.HullVoxels2.GetLength(0); x++)
                    if (meshHollower.HullVoxels2[x, y, z])
                        exteriorVoxels.Add(voxelizerDemo.GetVoxel(x,y,z));
    }

    private void CalculateInterior() {
        interiorVoxels = new List<Box3>();
        for (var z = 0; z < meshHollower.Visited.GetLength(2); z++)
            for (var y = 0; y < meshHollower.Visited.GetLength(1); y++)
                for (var x = 0; x < meshHollower.Visited.GetLength(0); x++)
                    if (!meshHollower.Visited[x, y, z])
                        interiorVoxels.Add(voxelizerDemo.GetVoxel(x,y,z));
    }
    
    private void CreateFluid(float radius) =>
        FluidParticleSource = new ParticlesFromVoxels(radius * 2, interiorVoxels);

    private void CreateBoundary(float radius) => 
        BoundaryParticleSource = new ParticlesFromVoxels(radius*2, exteriorVoxels);
    
    public void DrawBoundaryGizmo() {
        for (var z = 0; z < meshHollower.Visited.GetLength(2); z++) {
            for (var y = 0; y < meshHollower.Visited.GetLength(1); y++) {
                for (var x = 0; x < meshHollower.Visited.GetLength(0); x++) {
                    if (!meshHollower.HullVoxels2[x, y, z]) continue;
                    var voxel = voxelizerDemo.GetVoxel(x, y, z);
                    Gizmos.DrawCube(voxel.Center, voxel.Size);
                }
            }
        }
    }

    //GIZMOS
    public void DrawInteriorVoxelsGizmo() => 
        interiorVoxels.ForEach(voxel => Gizmos.DrawWireCube(voxel.Center,voxel.Size));

    public void DrawExteriorVoxelsGizmo() => 
        exteriorVoxels.ForEach(voxel => Gizmos.DrawWireCube(voxel.Center,voxel.Size));
    
    
    
}
