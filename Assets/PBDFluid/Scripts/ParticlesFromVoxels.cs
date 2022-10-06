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
        private readonly Vector3 voxelSize;
        private Matrix4x4 trs;
        
        public ParticlesFromVoxels(float spacing, List<Box3> voxels, Matrix4x4 trs) : base(spacing) {
            this.voxels = voxels;
            this.trs = trs;
            Assert.IsTrue(voxels.Count > 0, "voxels is empty");
            voxelSize = voxels[0].Size;
            Positions = new List<Vector3>();
        }

        
        /// <summary>
        /// Seeds particles in each voxel
        /// </summary>
        public override void CreateParticles() => voxels.ForEach(voxel => CreateParticlesInVoxel(voxel));

        
        /// <summary>
        /// Seeds particles in a voxel
        /// </summary>
        private void CreateParticlesInVoxel(Box3 voxel)
        {
            var numX = (int)((voxel.Size.x + HalfSpacing) / Spacing);
            var numY = (int)((voxel.Size.y + HalfSpacing) / Spacing);
            var numZ = (int)((voxel.Size.z + HalfSpacing) / Spacing);
            Assert.IsTrue(numX>0 || numY>0 || numZ>0, "Voxel too small for particle size");
            
            for (var z = 0; z < numZ; z++) {
                for (var y = 0; y < numY; y++) {
                    for (var x = 0; x < numX; x++) {
                        var pos = new Vector3 {
                            x = Spacing * x + voxel.Min.x + HalfSpacing,
                            y = Spacing * y + voxel.Min.y + HalfSpacing,
                            z = Spacing * z + voxel.Min.z + HalfSpacing
                        };
                        Positions.Add(trs.MultiplyPoint(pos));
                    }
                }
            }
        }
        
        /// <returns>true if voxel size is smaller than radius</returns>
        private bool AreVoxelsSmallerThanRadius() => voxelSize.x < Spacing 
                                                  || voxelSize.y < Spacing 
                                                  || voxelSize.z < Spacing;
    }
    
}
