using System;
using System.Collections.Generic;
using System.Linq;
using MeshVoxelizerProject;
using UnityEngine;

namespace PBDFluid
{
    public class ParticlesFromMesh: ParticleSource
    {
        public MeshVoxelizer MeshVoxelizer;
        private List<Box3> addedBoxes;
        public VoxelizerDemo voxelizerDemo;


        public ParticlesFromMesh(float spacing, MeshVoxelizer meshVoxelizer, VoxelizerDemo voxelizerDemo) : base(spacing) {
            MeshVoxelizer = meshVoxelizer;
            this.voxelizerDemo = voxelizerDemo;
            CreateParticles();
        }

        public override void CreateParticles()
        {
            Positions = new List<Vector3>();
            var voxels = voxelizerDemo.Voxels;
            foreach (var voxel in voxels) {
                Positions.Add(voxel.Center);
            }
        }
    }
}