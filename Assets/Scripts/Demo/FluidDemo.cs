using System;
using System.Collections.Generic;
using Factories;
using PBDFluid;
using PBDFluid.Scripts;
using SimulationObjects;
using SimulationObjects.FluidBoundaryObject;
using SimulationObjects.FluidObject;
using UnityEngine;
using UnityEngine.Assertions;
using Utility;

namespace Demo{
    public class FluidDemo : MonoBehaviour{
        //Constants
        private const float TimeStep = 1.0f / 60.0f;
        [NonSerialized] private const float Density = 500.0f;

        // Serializables
        public Bounds simulationBounds = new Bounds(Vector3.zero, new Vector3(6, 10, 6));

        public FluidDemoRenderSettings renderSettings = new FluidDemoRenderSettings();

        [SerializeField] private SIMULATION_SIZE simulationSize = SIMULATION_SIZE.MEDIUM;

        public Bounds barChartBounds;
        [SerializeField] [Range(0f, 5f)] public float deathPlaneHeight;

        [Header("Run")] [SerializeField] private bool run;
        [SerializeField] private bool stopDemo;

        private Camera mainCamera;

        // Fluid & Boundary Objects
        private FluidContainerizer fluidContainerizer;
        private FluidBoundaryCylinderCup barChart;
        private List<FluidBoundaryObject> fluidBoundaryObjects;
        private FluidObject[] fluidObjects;

        // Fluid Demo Objects
        [NonSerialized] public DeathPlane DeathPlane;
        private FluidBody fluid;
        private FluidBoundary boundary;
        private FluidSolver solver;
        private RenderVolume volume;

        // Booleans
        private bool hasStarted;
        private bool wasError;

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Starts the fluid simulation
        /// </summary>
        /// <remarks>Can be called from the outside, such as from UI</remarks>
        public void StartDemo(){
            if (hasStarted) return;

            mainCamera = Camera.main;
            run = true;
            fluidBoundaryObjects = new List<FluidBoundaryObject>();
            try{
                GetFluidBoundaryObjects();
                GetFluidObjects();
                GetDeathPlane();
                LoggingUtility.LogWithColor($"Fluid Demo: Found {fluidBoundaryObjects.Count} fluidBoundaryObjects!",
                                            Color.green);
                LoggingUtility.LogWithColor("Fluid Demo: Found {fluidObjects.Length} fluidObjects!", Color.green);

                fluidContainerizer = GetComponentInChildren<FluidContainerizer>();

                CreateBarChart();
                CreateFunnel();
                CreateDeathPlane();

                CreateFluid();
                CreateBoundary();

                var bounds = new Bounds(transform.position, simulationBounds.size);
                fluid.Bounds = bounds;
                solver = new FluidSolver(fluid, bounds, boundary);

                volume = new RenderVolume(bounds, Radius());
                volume.CreateMesh(renderSettings.volumeMaterial);
            }
            catch{
                wasError = true;
                throw;
            }

            hasStarted = true;
        }


        /// <summary>
        /// Creates a <see cref="DeathPlane"/> using the <see cref="DeathPlaneFactory"/>
        /// </summary>
        private void CreateDeathPlane() =>
            DeathPlane = DeathPlaneFactory.CreateDeathPlane(transform,
                                                            fluidContainerizer.meshBounds,
                                                            barChart.bounds,
                                                            Radius() * 2);


        /// <summary>
        /// Creates a <see cref="FluidBoundaryCylinderCup"/> using the <see cref="CylinderBarFactory"/>
        /// </summary>
        private void CreateBarChart() =>
            fluidBoundaryObjects.Add(barChart = CylinderBarFactory.CreateBarChart(transform, 
                                                                                  barChartBounds.center, 
                                                                                  barChartBounds.size));


        /// <summary>
        /// Creates a <see cref="Funnel"/> using the <see cref="FunnelFactory"/>
        /// </summary>
        private void CreateFunnel() =>
            fluidBoundaryObjects.Add(FunnelFactory.CreateFunnel(transform,
                                                                barChart.bounds,
                                                                fluidContainerizer.meshBounds,
                                                                60.0f));


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
        public float Radius() => simulationSize switch{
            SIMULATION_SIZE.LOW => 0.1f,
            SIMULATION_SIZE.MEDIUM => 0.08f,
            SIMULATION_SIZE.HIGH => 0.06f,
            _ => 0.08f
        };


        /// <summary>
        /// Searches child gameObjects for FluidBoundaryObjects
        /// </summary>
        private void GetFluidBoundaryObjects() =>
            fluidBoundaryObjects.AddRange(GetComponentsInChildren<FluidBoundaryObject>());


        /// <summary>
        /// Searches child gameObjects for FluidObject components
        /// </summary>
        private void GetFluidObjects() => fluidObjects = GetComponentsInChildren<FluidObject>();


        /// <summary>
        /// Searches child gameObjects for a DeathPlane component
        /// </summary>
        private void GetDeathPlane() => DeathPlane = GetComponentInChildren<DeathPlane>();


        private void Update(){
            if (wasError) return;
            if (!hasStarted){
                if (run) StartDemo();
                return;
            }

            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            if (run) DemoStep();

            volume.Hide = !renderSettings.drawFluidVolume;

            if (renderSettings.drawBoundaryParticles)
                DrawBoundaryParticles();
            if (renderSettings.drawFluidParticles)
                DrawFluidParticles();
            if (stopDemo) StopDemo();
        }


        /// <summary>
        /// Stops the demo, pausing the fluid in motion, and discarding all compute shaders
        /// </summary>
        public void StopDemo(){
            run = false;
            boundary?.Dispose();
            renderSettings.drawBoundaryParticles = false;
            stopDemo = false;
        }


        /// <summary>
        /// Updates the solver and volume
        /// </summary>
        private void DemoStep(){
            DeathPlane.SliderHasChanged(deathPlaneHeight);
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            solver.StepPhysics(TimeStep,
                               DeathPlane.transform.position,
                               DeathPlane.size);
            volume.FillVolume(fluid,
                              solver.Hash,
                              solver.Kernel);
        }


        /// <summary>
        /// Draws the fluid particles
        /// </summary>
        private void DrawFluidParticles() =>
            fluid.Draw(mainCamera,
                       renderSettings.sphereMesh,
                       renderSettings.fluidParticleMaterial,
                       0);


        /// <summary>
        /// Draws the boundary particles
        /// </summary>
        private void DrawBoundaryParticles() =>
            boundary.Draw(mainCamera,
                          renderSettings.sphereMesh,
                          renderSettings.boundaryParticleMaterial,
                          0,
                          Color.red,
                          DeathPlane.transform.position,
                          DeathPlane.size);

        private void OnDestroy(){
            boundary?.Dispose();
            fluid?.Dispose();
            solver?.Dispose();
            volume?.Dispose();
        }

        private void OnRenderObject(){
            var cam = Camera.current;
            if (cam != Camera.main) return;

            if (hasStarted && renderSettings.drawGrid)
                solver.Hash.DrawGrid(cam, Color.yellow);
        }


        /// <summary>
        /// Puts every fluid boundary object into a particle source and creates a new FluidBody
        /// </summary>
        private void CreateBoundary(){
            var particleSources = new List<ParticleSource>(fluidBoundaryObjects.Count);
            fluidBoundaryObjects.ForEach(obj => particleSources.Add(obj.ParticleSource));
            var particleSource = new ParticlesFromSeveralBounds(Radius() * 2, particleSources.ToArray());
            particleSource.CreateParticles();
            boundary = new FluidBoundary(particleSource,
                                         Radius(),
                                         Density);
        }


        /// <summary>
        /// Puts every fluid object into a particle source and creates a new FluidBody
        /// </summary>
        private void CreateFluid(){
            var particleSources = new ParticleSource[fluidObjects.Length];
            for (var index = 0; index < fluidObjects.Length; index++)
                particleSources[index] = fluidObjects[index].ParticleSource;
            Assert.IsTrue(particleSources.Length > 0);
            var particleSource = new ParticlesFromSeveralBounds(Radius() * 2, particleSources);
            particleSource.CreateParticles();
            fluid = new FluidBody(particleSource, Radius(), Density, transform.localToWorldMatrix);
        }


        private void OnDrawGizmos(){
            //Simulation Bounds
            Gizmos.color = Color.yellow;
            if (renderSettings.drawSimulationBounds) DrawSimulationBounds();
            Gizmos.color = Color.red;
            if (renderSettings.drawBarChart && renderSettings.cylinderMesh != null) DrawBarCylinderGizmo();
        }


        /// <summary>
        /// Draws a gizmo showing the barchart cylinder
        /// </summary>
        private void DrawBarCylinderGizmo() =>
            Gizmos.DrawWireMesh(renderSettings.cylinderMesh,
                                transform.position + barChartBounds.center,
                                Quaternion.identity,
                                new Vector3(barChartBounds.size.x,
                                            barChartBounds.size.y / 2f,
                                            barChartBounds.size.z));


        /// <summary>
        /// Draws a gizmo showing the simulation bounds
        /// </summary>
        private void DrawSimulationBounds() =>
            Gizmos.DrawWireCube(transform.position + simulationBounds.center, simulationBounds.size);
    }
}