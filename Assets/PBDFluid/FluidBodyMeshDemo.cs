using System;
using UnityEngine;
using System.Collections.Generic;
using MeshVoxelizer.Scripts;
using PBDFluid.Scripts;
using UnityEngine.Assertions;

namespace PBDFluid
{
    public class FluidBodyMeshDemo : MonoBehaviour
    {
        //Constants
        private const float TimeStep = 1.0f / 60.0f;
        
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Bounds simulationBounds;
        
        [Header("Materials")]
        [SerializeField] private Material fluidParticleMat;
        [SerializeField] private Material boundaryParticleMat;
        [SerializeField] private Material volumeMat;
        
        [Header("Render Booleans")]
        [SerializeField] private bool drawGrid;
        [SerializeField] private bool drawBoundaryParticles;
        [SerializeField] private bool drawFluidParticles;
        [SerializeField] private bool drawFluidVolume;
        [SerializeField] private bool drawSimulationBounds;
        
        [Header("Simulation Settings")]
        [SerializeField] private SIMULATION_SIZE simulationSize = SIMULATION_SIZE.MEDIUM;
        [SerializeField] private bool run = true;
        [SerializeField] private Mesh sphereMesh;

        public DeathPlane deathPlane;
        private FluidBoundaryObject[] fluidBoundaryObjects;
        private FluidObject[] fluidObjects;
        
        [NonSerialized] private const float Density = 1000.0f;

        private bool hasStarted;
        private FluidBody fluid;
        private FluidBoundary boundary;
        private FluidSolver solver;
        private RenderVolume volume;
        private bool wasError;

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Starts the fluid simulation
        /// </summary>
        /// <remarks>Can be called from the outside, such as from UI</remarks>
        public void StartDemo() {
            if (hasStarted) return;
            try
            {
                run = true;
                GetFluidBoundaryObjects();
                GetFluidObjects();
                GetDeathPlane();
                LoggingUtility.LogWithColor($"Fluid Demo: Found {fluidBoundaryObjects.Length} fluidBoundaryObjects!",Color.green);
                LoggingUtility.LogWithColor("Fluid Demo: Found {fluidObjects.Length} fluidObjects!",Color.green);

                CreateFluid();
                CreateBoundary();
                
                var bounds = simulationBounds;
                fluid.Bounds = bounds;
                solver = new FluidSolver(fluid, bounds, boundary);

                volume = new RenderVolume(bounds, Radius());
                volume.CreateMesh(volumeMat);
            }
            catch{
                wasError = true;
                throw;
            }
            hasStarted = true;
        }
        
        
        /// <summary>
        /// Adjusts the Radius based on the SIMULATION_SIZE enum.
        /// A smaller radius means more particles.
        /// If the number of particles is to low or high
        /// the bitonic sort shader will throw a exception
        /// as it has a set range it can handle but this can be manually changes in the BitonicSort.cs script.
        /// </summary>
        /// <remarks>
        /// A smaller radius may also require more solver steps
        /// as the boundary is thinner and the fluid many step
        /// through and leak out.
        /// </remarks>
        /// <returns>The radius for particles</returns>
        public float Radius() =>  simulationSize switch {
            SIMULATION_SIZE.LOW => 0.1f,
            SIMULATION_SIZE.MEDIUM => 0.08f,
            SIMULATION_SIZE.HIGH => 0.06f,
            _ => 0.08f
        };

        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Searches child gameobjects for FluidBoundaryObjects
        /// </summary>
        private void GetFluidBoundaryObjects() => fluidBoundaryObjects = GetComponentsInChildren<FluidBoundaryObject>();
        
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Searches child gameobjects for FluidObject components
        /// </summary>
        private void GetFluidObjects() => fluidObjects = GetComponentsInChildren<FluidObject>();

        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Searches child gameobjects for a DeathPlane component
        /// </summary>
        private void GetDeathPlane() => deathPlane = GetComponentInChildren<DeathPlane>();

        private void Update() {
            if(wasError) return;
            if (!hasStarted){
                if (run) StartDemo();
                return;
            }
            if (run) {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                solver.StepPhysics(TimeStep, deathPlane.transform.position, deathPlane.size);
                volume.FillVolume(fluid, solver.Hash, solver.Kernel);
            }

            volume.Hide = !drawFluidVolume;

            if (drawBoundaryParticles)
                boundary.Draw(mainCamera, sphereMesh, boundaryParticleMat, 0, Color.red, deathPlane.transform.position, deathPlane.size);
            if (drawFluidParticles)
                fluid.Draw(mainCamera, sphereMesh, fluidParticleMat, 0);
        }

        private void OnDestroy() {
            boundary?.Dispose();
            fluid?.Dispose();
            solver?.Dispose();
            volume?.Dispose();
        }

        private void OnRenderObject() {
            var cam = Camera.current;
            if (cam != Camera.main) return;
            
            if(drawGrid)
                solver.Hash.DrawGrid(cam, Color.yellow);
        }

        /// <summary>
        /// Puts every fluid boundary object into a particle source and creates a new FluidBody
        /// </summary>
        private void CreateBoundary() {
            var particleSources = new ParticleSource[fluidBoundaryObjects.Length];
            for (var index = 0; index < fluidBoundaryObjects.Length; index++)
                particleSources[index] = fluidBoundaryObjects[index].ParticleSource;
            var particleSource = new ParticlesFromSeveralBounds(Radius() * 2, particleSources);
            particleSource.CreateParticles();
            boundary = new FluidBoundary(particleSource,Radius(),Density);
        }

        /// <summary>
        /// Puts every fluid object into a particle source and creates a new FluidBody
        /// </summary>
        private void CreateFluid()
        {
            var particleSources = new ParticleSource[fluidObjects.Length];
            for (var index = 0; index < fluidObjects.Length; index++)
                particleSources[index] = fluidObjects[index].ParticleSource;
            Assert.IsTrue(particleSources.Length > 0);
            var particleSource = new ParticlesFromSeveralBounds(Radius() * 2, particleSources);
            particleSource.CreateParticles();
            fluid = new FluidBody(particleSource, Radius(), Density, transform.localToWorldMatrix);
        }
        
        private void OnDrawGizmos() {
            //Simulation Bounds
            Gizmos.color = Color.yellow;
            if (drawSimulationBounds) DrawSimulationBounds();
        }

        /// <summary>
        /// Draws a gizmo showing the simulation bounds
        /// </summary>
        private void DrawSimulationBounds() => 
            Gizmos.DrawWireCube(transform.position+simulationBounds.center,simulationBounds.size);

    }
}
