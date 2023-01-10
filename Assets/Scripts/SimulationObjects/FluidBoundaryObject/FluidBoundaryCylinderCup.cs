using System;
using System.Collections;
using System.Collections.Generic;
using PBDFluid;
using UnityEngine;
using UnityEngine.Assertions;

public class FluidBoundaryCylinderCup : FluidBoundaryObject{
    [SerializeField] public float height;
    [SerializeField] public float radius;
    [NonSerialized] public Bounds bounds;
    
    public void CreateParticles()
    {
        FluidDemo = GetComponentInParent<FluidDemo>();
        Assert.IsNotNull(FluidDemo);
        // ParticleSource = new ParticlesFromBounds(FluidBodyMeshDemo.Radius() * 2, OuterBounds(), InnerBounds());
        var spacing = FluidDemo.Radius() * 2f;
        ParticleSource = new ParticlesFromList(spacing, CreateCylinderCup(spacing));
        LoggingUtility.LogInfo($"FluidBoundaryCylinderCup {name} har a total of {ParticleSource.NumParticles} boundary particles!");
        bounds = new Bounds(transform.position, 
                            new Vector3(radius * 2f, 
                                        height, 
                                        radius * 2f));
    }

    List<Vector3> CreateCylinderCup(float spacing){
        var cylinderCup = new List<Vector3>();
        var halfHeight = height / 2f;
        cylinderCup.AddRange(CreateFloor(spacing, halfHeight));
        cylinderCup.AddRange(CreateCylinder(spacing, halfHeight));
        return cylinderCup;
    }

    List<Vector3> CreateFloor(float spacing, float halfHeight){
        var floorParticles = new List<Vector3>();
        var r = radius-spacing;
        var numAddedParticles = 9999;
        while (numAddedParticles>0){
            var particles = ParticleCircle(r, spacing, -halfHeight);
            numAddedParticles = particles.Count;
            floorParticles.AddRange(particles);
            r -= spacing;
        }
        floorParticles.AddRange(ParticleCircle(r,spacing,halfHeight));
        return floorParticles;
    }

    List<Vector3> CreateCylinder(float spacing, float halfHeight){
        var particleFunnel = new List<Vector3>();
        
        for (var heightLevel = -halfHeight; heightLevel < halfHeight; heightLevel += spacing)
            particleFunnel.AddRange(ParticleCircle(radius, 
                                                   spacing, 
                                                   heightLevel));
        return particleFunnel;
    }
    
    List<Vector3> ParticleCircle(float r, float spacing, float particleHeight){
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
                          int particleIndex) => new Vector3(r * Mathf.Cos(theta * particleIndex),
                                                            particleHeight,
                                                            r * Mathf.Sin(theta * particleIndex));

}
