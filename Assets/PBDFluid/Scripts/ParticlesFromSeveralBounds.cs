using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace PBDFluid
{

    public class ParticlesFromSeveralBounds : ParticleSource
    {
        public ParticlesFromBounds[] particlesFromBoundsArray;
        public Vector3[] boundsVectors;
        public List<int> particle2MatrixMap;


        public ParticlesFromSeveralBounds(float spacing, ParticlesFromBounds[] particlesFromBoundsArray, Vector3[] boundsVectors) : base(spacing){
            this.particlesFromBoundsArray = particlesFromBoundsArray;
            this.boundsVectors = boundsVectors;
        }

        public override void CreateParticles()
        {
            Positions = new List<Vector3>();
            particle2MatrixMap = new List<int>();
            for (var boundsIdx = 0; boundsIdx < particlesFromBoundsArray.Length; boundsIdx++){
                particlesFromBoundsArray[boundsIdx].CreateParticles();
                for (var particleIdx = 0; particleIdx < particlesFromBoundsArray[boundsIdx].NumParticles; particleIdx++) {
                    Positions.Add(particlesFromBoundsArray[boundsIdx].Positions[particleIdx]);
                    particle2MatrixMap.Add(boundsIdx);
                }
            }
        }

    }
}