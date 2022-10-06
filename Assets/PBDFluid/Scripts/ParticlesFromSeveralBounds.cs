using System.Collections.Generic;
using UnityEngine;

namespace PBDFluid.Scripts
{
    public class ParticlesFromSeveralBounds : ParticleSource
    {
        private readonly ParticleSource[] particleSources;
        public List<int> Particle2MatrixMap;


        public ParticlesFromSeveralBounds(float spacing, ParticleSource[] particleSources) : base(spacing){
            this.particleSources = particleSources;
        }

        /// <summary>
        /// Goes through every particleSource, and calls CreateParticles method on them if not already called.
        /// Then adds every particle from every particleSource into its own list
        /// </summary>
        public override void CreateParticles()
        {
            Positions = new List<Vector3>();
            Particle2MatrixMap = new List<int>();
            for (var boundsIdx = 0; boundsIdx < particleSources.Length; boundsIdx++){
                if (particleSources[boundsIdx].NumParticles == 0) 
                    particleSources[boundsIdx].CreateParticles();
                for (var particleIdx = 0; particleIdx < particleSources[boundsIdx].NumParticles; particleIdx++) {
                    Positions.Add(particleSources[boundsIdx].Positions[particleIdx]);
                    Particle2MatrixMap.Add(boundsIdx);
                }
            }
        }

    }
}