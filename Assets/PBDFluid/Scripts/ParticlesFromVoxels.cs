using System.Collections.Generic;
using System.Linq;
using MeshVoxelizer.Scripts;
using UnityEngine;
using UnityEngine.Assertions;

namespace PBDFluid.Scripts
{
    public class ParticlesFromVoxels : ParticleSource
    {
        /// <summary>
        /// List of each voxel
        /// </summary>
        private readonly List<Box3> voxels;
        /// <summary>
        /// transformation matrix to transform each particle by
        /// </summary>
        private Matrix4x4 trs;
        /// <summary>
        /// volume loss from voxelization to compensate for
        /// </summary>
        private readonly float realVolume;

        private bool useRealVolume;
        
        public ParticlesFromVoxels(float spacing, List<Box3> voxels, float realVolume, bool useRealVolume,  Matrix4x4 trs) : base(spacing) {
            this.voxels = voxels;
            this.trs = trs;
            this.realVolume = realVolume;
            this.useRealVolume = useRealVolume;
            Assert.IsTrue(voxels.Count > 0, "voxels is empty");
            Positions = new List<Vector3>();
        }
        
        /// <summary>
        /// Returns the volume of a single particle
        /// </summary>
        private float ParticleVolume => 
            4f/3f * Mathf.PI * HalfSpacing * HalfSpacing * HalfSpacing;
        
        
        /// <summary>
        /// Finds out how many extra particles are needed to compensate for volume loss in voxelization
        /// </summary>
        private int ExtraVoxelsCount => 
            Mathf.RoundToInt(realVolume / ParticleVolume) - voxels.Count;


        /// <summary>
        /// Seeds particles in each voxel
        /// </summary>
        public override void CreateParticles(){
            if (!useRealVolume){
                voxels.ForEach(voxel => CreateParticleInVoxel(voxel,false));
                return;
            }
            if (ExtraVoxelsCount < 0){
                Debug.Log("EXTRA VOXELS COUNT IS NEGATIVE");
                voxels.Take(voxels.Count-ExtraVoxelsCount)
                      .ToList()
                      .ForEach(voxel => CreateParticleInVoxel(voxel, false));
            }
            else{
                //first pass
                voxels.ForEach(voxel => CreateParticleInVoxel(voxel,false));
                //second pass to make up for voxelization volume loss
                var extraVoxels = voxels.Take(ExtraVoxelsCount).ToList();
                extraVoxels.ForEach(voxel => CreateParticleInVoxel(voxel,true));
            }
        }
        
        
        private void CreateParticleInVoxel(Box3 voxel, bool shouldOffset){
            const float offset = .001f;
            var offsetVector = new Vector3(offset, offset, offset);
            var pos = voxel.Center;
            if (shouldOffset) pos += offsetVector;
            Positions.Add(trs.MultiplyPoint(pos));
        }
    }
}
