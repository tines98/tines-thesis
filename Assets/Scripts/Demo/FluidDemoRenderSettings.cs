using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FluidDemoRenderSettings
{
    [Header("Material Settings")]
    public Material fluidParticleMaterial;
    public Material boundaryParticleMaterial;
    public Material volumeMaterial;
    
    [Header("Mesh Settings")]
    public Mesh sphereMesh;
    public Mesh cylinderMesh;
    
    [Header("Gizmo Settings")]
    public bool drawGrid;
    public bool drawBoundaryParticles;
    public bool drawFluidParticles;
    public bool drawFluidVolume;
    public bool drawSimulationBounds;
    public bool drawBarChart;
}
