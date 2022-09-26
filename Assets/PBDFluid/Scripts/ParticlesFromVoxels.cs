using System;
using System.Collections.Generic;
using MeshVoxelizer.Scripts;
using UnityEngine;
using UnityEngine.Assertions;

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
            Assert.IsFalse(AreVoxelsSmallerThanRadius(),$"Voxels are too small, voxel size is {voxelSize}, and can't be smaller than {Spacing}");
            Positions = new List<Vector3>();
        }

        public override void CreateParticles() => voxels.ForEach(voxel => CreateParticlesInVoxel(voxel));

        private void CreateParticlesInVoxel(Box3 voxel)
        {
            int numX = (int)((voxel.Size.x + HalfSpacing) / Spacing);
            int numY = (int)((voxel.Size.y + HalfSpacing) / Spacing);
            int numZ = (int)((voxel.Size.z + HalfSpacing) / Spacing);
            Assert.IsTrue(numX>0 || numY>0 || numZ>0, "Voxel too small for particle size");
            
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

        private bool AreVoxelsSmallerThanRadius() => voxelSize.x < Spacing 
                                            || voxelSize.y < Spacing
                                            || voxelSize.z < Spacing;
    }
    
}
