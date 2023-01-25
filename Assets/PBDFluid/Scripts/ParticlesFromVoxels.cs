using System.Collections.Generic;
using System.Linq;
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
        public override void CreateParticles(){
            //first pass
            voxels.ForEach(voxel => CreateParticleInVoxel(voxel,false));
            //second pass to make up for 0,5236 volume error
            var error = CalculateVolumeError(voxels[0], HalfSpacing);
            Debug.Log("error = " + error);
            Debug.Log("test = " + CalculateVolumeError(new Box3(Vector3.zero, new Vector3(2,2,2)), 1f));
            var extraVoxelCount = Mathf.FloorToInt(voxels.Count * error);
            var extraVoxels = voxels.Take(extraVoxelCount).ToList();
            extraVoxels.ForEach(voxel => CreateParticleInVoxel(voxel,true));
        }

        private float CalculateVolumeError(Box3 voxel, float particleRadius){
            var voxelVolume = voxel.Size.x * voxel.Size.y * voxel.Size.z;
            var particleVolume = 4f / 3f * Mathf.PI * particleRadius * particleRadius * particleRadius;
            return (voxelVolume / particleVolume) - 1f;
        }


        private void CreateParticleInVoxel(Box3 voxel, bool shouldOffset){
            var offset = .001f;
            var offsetVector = new Vector3(offset, offset, offset);
            var pos = voxel.Center;
            if (shouldOffset) pos += offsetVector;
            Positions.Add(trs.MultiplyPoint(pos));
        }
    }
}
