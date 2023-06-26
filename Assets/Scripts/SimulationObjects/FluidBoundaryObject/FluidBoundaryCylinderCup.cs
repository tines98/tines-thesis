using System;
using System.Collections.Generic;
using Demo;
using UnityEngine;
using UnityEngine.Assertions;
using Utility;

namespace SimulationObjects.FluidBoundaryObject{
    public class FluidBoundaryCylinderCup : FluidBoundaryObject{
        [SerializeField] public float height;
        [SerializeField] public float radius;
        [NonSerialized] public Bounds Bounds;
        [SerializeField] private bool drawGizmo;
    
        public void CreateParticles()
        {
            FluidDemo = GetComponentInParent<FluidDemo>();
            Assert.IsNotNull(FluidDemo);
            // ParticleSource = new ParticlesFromBounds(FluidBodyMeshDemo.Radius() * 2, OuterBounds(), InnerBounds());
            var spacing = FluidDemo.Radius;// * 2f;
            ParticleSource = new ParticlesFromList(spacing, CreateCylinderCup(spacing), Matrix4x4.identity);
            // LoggingUtility.LogInfo($"FluidBoundaryCylinderCup {name} har a total of {ParticleSource.NumParticles} boundary particles!");
            Bounds = new Bounds(transform.position, 
                                new Vector3(radius * 2f, 
                                            height, 
                                            radius * 2f));
        }

        List<Vector3> CreateCylinderCup(float spacing){
            var cylinderCup = new List<Vector3>();
            var halfHeight = (height / 2f) + spacing * 2f;
            cylinderCup.AddRange(CreateFloor(spacing, 
                                             radius, 
                                             halfHeight));
            cylinderCup.AddRange(CreateCylinder(spacing, 
                                                radius, 
                                                -halfHeight, 
                                                halfHeight));
            //extra bottom guard
            // cylinderCup.AddRange(CreateCylinder(spacing, radius+spacing/2f, -halfHeight, halfHeight));
            // //extra floor
            // cylinderCup.AddRange(CreateFloor(spacing, halfHeight-spacing/2f));
            return cylinderCup;
        }

        List<Vector3> CreateFloor(float spacing, float outerRadius, float halfHeight){
            var floorParticles = new List<Vector3>();
            var r = outerRadius;
            var numAddedParticles = 9999;
            //keep trying to add circles of particles until no particles are added
            while (numAddedParticles>0){
                var particles = ParticleCircle(spacing, r, -halfHeight);
                numAddedParticles = particles.Count;
                floorParticles.AddRange(particles);
                r -= spacing;
            }
            floorParticles.Add(transform.position-Vector3.up*halfHeight);
            floorParticles.AddRange(ParticleCircle(spacing,r,halfHeight));
            return floorParticles;
        }

        List<Vector3> CreateCylinder(float spacing, float cylinderRadius, float minY, float maxY){
            var particleFunnel = new List<Vector3>();
            for (var y = minY; y < maxY; y += spacing)
                particleFunnel.AddRange(ParticleCircle(spacing, 
                                                       cylinderRadius, 
                                                       y));
            return particleFunnel;
        }
    
        List<Vector3> ParticleCircle(float spacing, float r, float particleHeight){
            var totalPoints = (int) (2 * r * Mathf.PI / spacing);
            if (totalPoints <= 0) return new List<Vector3>(0);
            var particleCircle = new List<Vector3>(totalPoints);
            var theta = 2 * Mathf.PI / totalPoints;
            for (var particleIndex = 0; particleIndex < totalPoints; particleIndex++)
                particleCircle.Add(transform.position 
                                 + PlaceParticle(r,
                                                 theta,
                                                 particleHeight, 
                                                 particleIndex));
            return particleCircle;
        }



        Vector3 PlaceParticle(float r,
                              float theta,
                              float particleHeight,
                              int particleIndex) => new(r * Mathf.Cos(theta * particleIndex),
                                                        particleHeight,
                                                        r * Mathf.Sin(theta * particleIndex));

        private void OnDrawGizmos(){
            if (!drawGizmo) return;
            Gizmos.color = Color.red;
            ParticleSource.Positions.ForEach(particle => 
                Gizmos.DrawWireCube(particle, 
                                    new Vector3(ParticleSource.Spacing,
                                                ParticleSource.Spacing,
                                                ParticleSource.Spacing)));
        }
    }
}
