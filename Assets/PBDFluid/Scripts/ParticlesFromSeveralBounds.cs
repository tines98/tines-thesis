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

        public override void CreateParticles()
        {
            Positions = new List<Vector3>();
            Particle2MatrixMap = new List<int>();
            for (var boundsIdx = 0; boundsIdx < particleSources.Length; boundsIdx++){
                particleSources[boundsIdx].CreateParticles();
                for (var particleIdx = 0; particleIdx < particleSources[boundsIdx].NumParticles; particleIdx++) {
                    Positions.Add(particleSources[boundsIdx].Positions[particleIdx]);
                    Particle2MatrixMap.Add(boundsIdx);
                }
            }
        }

    }
}