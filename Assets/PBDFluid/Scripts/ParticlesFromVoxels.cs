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
            foreach (var voxel in voxels)
            {
                //Place a particle at the center of each voxel
                Positions.Add(voxel.Center);
                //TODO: figure out when to place more than one particle per voxel
            }
        }
    }
}
