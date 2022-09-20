using System.Collections.Generic;
using MeshVoxelizerProject;
using UnityEngine;

namespace PBDFluid.Scripts
{
    public class ParticlesFromVoxels : ParticleSource
    {
        private List<Box3> m_voxels;
        private Vector3 m_voxelSize;
    
        public ParticlesFromVoxels(float spacing, List<Box3> voxels) : base(spacing)
        {
            m_voxels = voxels;
            m_voxelSize = voxels[0].Size;
        }

        public override void CreateParticles()
        {
            Positions = new List<Vector3>();
            foreach (var voxel in m_voxels)
            {
                //Place a particle at the center of each voxel
                Positions.Add(voxel.Center);
                //TODO: figure out when to place more than one particle per voxel
            }
        }
    }
}
