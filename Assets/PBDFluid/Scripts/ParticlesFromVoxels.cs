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
        private Matrix4x4 trs;
        
        public ParticlesFromVoxels(float spacing, List<Box3> voxels, Matrix4x4 trs) : base(spacing) {
            this.voxels = voxels;
            this.trs = trs;
            Assert.IsTrue(voxels.Count > 0, "voxels is empty");
            Positions = new List<Vector3>();
        }

        
        /// <summary>
        /// Seeds particles in each voxel
        /// </summary>
        public override void CreateParticles() => voxels.ForEach(voxel => CreateParticleInVoxel(voxel));


        private void CreateParticleInVoxel(Box3 voxel){
            Positions.Add(trs.MultiplyPoint(voxel.Center));
        }
        
        /// <summary>
        /// Seeds particles in a voxel
        /// </summary>
        private void CreateParticlesInVoxel(Box3 voxel){
            var scaledVoxelSize = trs.MultiplyVector(voxel.Size);
            var numX = (int)((scaledVoxelSize.x + HalfSpacing) / Spacing);
            var numY = (int)((scaledVoxelSize.y + HalfSpacing) / Spacing);
            var numZ = (int)((scaledVoxelSize.z + HalfSpacing) / Spacing);
            Assert.IsTrue(numX>0 || numY>0 || numZ>0, $"Voxel too small for particle size {scaledVoxelSize.x + HalfSpacing} {Spacing}");
            
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
    }
}
