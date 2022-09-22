using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace PBDFluid
{

    public class ParticlesFromSeveralBounds : ParticleSource
    {
        public ParticleSource[] ParticleSources;
        public List<int> particle2MatrixMap;


        public ParticlesFromSeveralBounds(float spacing, ParticleSource[] particleSources) : base(spacing){
            ParticleSources = particleSources;
        }

        public override void CreateParticles()
        {
            Positions = new List<Vector3>();
            particle2MatrixMap = new List<int>();
            for (var boundsIdx = 0; boundsIdx < ParticleSources.Length; boundsIdx++){
                ParticleSources[boundsIdx].CreateParticles();
                for (var particleIdx = 0; particleIdx < ParticleSources[boundsIdx].NumParticles; particleIdx++) {
                    Positions.Add(ParticleSources[boundsIdx].Positions[particleIdx]);
                    particle2MatrixMap.Add(boundsIdx);
                }
            }
        }

    }
}