using System.Collections.Generic;
using MeshVoxelizer.Scripts;
using UnityEngine;

namespace PBDFluid.Scripts
{
    public class ParticlesFromVoxels : ParticleSource
    {
        private readonly List<Box3> voxels;
        private Vector3 voxelSize;
    
        public ParticlesFromVoxels(float spacing, List<Box3> voxels) : base(spacing)
        {
            this.voxels = voxels;
            voxelSize = voxels[0].Size;
        }

        public override void CreateParticles()
        {
            Positions = new List<Vector3>();
            foreach (var voxel in voxels) {
                CreateParticlesInVoxel(voxel);
            }
        }

        private void CreateParticlesInVoxel(Box3 voxel)
        {
            int numX = (int)((voxel.Size.x + HalfSpacing) / Spacing);
            int numY = (int)((voxel.Size.y + HalfSpacing) / Spacing);
            int numZ = (int)((voxel.Size.z + HalfSpacing) / Spacing);

            Positions = new List<Vector3>();

            for (int z = 0; z < numZ; z++)
            {
                for (int y = 0; y < numY; y++)
                {
                    for (int x = 0; x < numX; x++)
                    {
                        Vector3 pos = new Vector3();
                        pos.x = Spacing * x + voxel.Min.x + HalfSpacing;
                        pos.y = Spacing * y + voxel.Min.y + HalfSpacing;
                        pos.z = Spacing * z + voxel.Min.z + HalfSpacing;
                        Positions.Add(pos);
                    }
                }
            }
        }
    }
}
