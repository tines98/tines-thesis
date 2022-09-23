using System.Collections.Generic;
using MeshVoxelizer.Scripts;
using UnityEngine;

namespace PBDFluid.Scripts
{
    public class FluidContainerFromMesh
    {
        public ParticleSource FluidParticleSource;
        public ParticleSource BoundaryParticleSource;
        
        private readonly VoxelizerDemo voxelizerDemo;
        private readonly MeshHollower meshHollower;
        
        private readonly Bounds bounds;
        private List<Box3> exteriorVoxels;
        private List<Box3> interiorVoxels;
        

        public FluidContainerFromMesh(VoxelizerDemo voxelizerDemo, float radius, float density)
        {
            this.voxelizerDemo = voxelizerDemo;
            bounds = new Bounds(voxelizerDemo.Bounds.Center,voxelizerDemo.Bounds.Size);
            
            meshHollower = new MeshHollower(this.voxelizerDemo.Voxelizer.Voxels);
            
            CalculateExterior();
            CalculateInterior();
            
            CreateFluid(radius);
            CreateBoundary(radius);
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
        
        private void CreateFluid(float radius) {
            var diameter = radius * 2;
            var exclusion = new List<Bounds>();
            for (var x = 0; x < meshHollower.Visited.GetLength(0); x++) {
                for (var y = 0; y < meshHollower.Visited.GetLength(1); y++) {
                    for (var z = 0; z < meshHollower.Visited.GetLength(2); z++) {
                        if (!meshHollower.Visited[x, y, z]) continue;
                        var voxel = voxelizerDemo.GetVoxel(x, y, z);
                        var bound = new Bounds(voxel.Center, voxel.Size);
                        exclusion.Add(bound);
                    }
                }
            }
            FluidParticleSource = new ParticlesFromBounds(diameter, bounds, exclusion);
        }

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
}