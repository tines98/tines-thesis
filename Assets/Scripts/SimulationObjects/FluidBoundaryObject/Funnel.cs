using System.Collections.Generic;
using Demo;
using UnityEngine;

namespace SimulationObjects.FluidBoundaryObject{
    public class Funnel : FluidBoundaryObject{
        [SerializeField] private float lowerRadius;
        [SerializeField] private float upperRadius;
        [SerializeField] private float height;
        [SerializeField] private bool drawFunnel;

        public void CreateParticles(){
            FluidDemo = GetComponentInParent<FluidDemo>();
            var spacing = FluidDemo.Radius() * 2;
            var particles = ParticleFunnel(spacing);
            Debug.Log($"funnel has {particles.Count}");
            ParticleSource = new ParticlesFromList(spacing, particles);
        }

        public void SetLowerRadius(float radius) => lowerRadius = radius;
    
        public void SetUpperRadius(float radius) => upperRadius = radius;

        public void SetHeight(float funnelHeight) => height = funnelHeight;

        float CalcFunnelAngle() => Vector3.Angle(Vector3.up,
                                                 new Vector3(0, height, upperRadius)
                                               - new Vector3(0, 0, lowerRadius));
        List<Vector3> ParticleFunnel(float spacing){
            var particleFunnel = new List<Vector3>();
            var angle = CalcFunnelAngle();
            if (angle>45)
                Debug.Log(CalcFunnelAngle());
            for (var heightLevel = 0f; heightLevel < height; heightLevel += spacing){
                var radius = Mathf.Lerp(lowerRadius, upperRadius, heightLevel / height);
                particleFunnel.AddRange(ParticleCircle(radius, spacing, heightLevel));
            }

            return particleFunnel;
        }

        List<Vector3> ParticleCircle(float radius, float spacing, float particleHeight){
            var totalPoints = (int) (2 * radius * Mathf.PI / spacing);
            var particleCircle = new List<Vector3>(totalPoints);
            var theta = 2 * Mathf.PI / totalPoints;
        
            for (var particleIndex = 0; particleIndex < totalPoints; particleIndex++)
                particleCircle.Add(transform.position 
                                 + PlaceParticle(radius,
                                                 theta, 
                                                 particleHeight, 
                                                 particleIndex));
            return particleCircle;
        }

        Vector3 PlaceParticle(float radius,
                              float theta,
                              float particleHeight,
                              int particleIndex) => new Vector3(radius * Mathf.Cos(theta * particleIndex),
                                                                particleHeight,
                                                                radius * Mathf.Sin(theta * particleIndex));

        private void OnDrawGizmos(){
            if (!drawFunnel) return;
            float radius = .08f;
            if (FluidDemo != null) radius = FluidDemo.Radius();
            var particleFunnel = ParticleFunnel(radius*2);
            var camPos = Camera.current.transform.position;
            particleFunnel.Sort((a, b) => (b - camPos).sqrMagnitude
                                                      .CompareTo((a - camPos).sqrMagnitude)); 
            particleFunnel.ForEach(part => Gizmos.DrawSphere(part, radius));
        }
    }
}