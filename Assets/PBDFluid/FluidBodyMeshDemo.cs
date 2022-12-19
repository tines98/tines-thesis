using System;
using System.Collections.Generic;
using UnityEngine;
using PBDFluid.Scripts;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace PBDFluid
{
    public class FluidBodyMeshDemo : MonoBehaviour
    {
        //Constants
        private const float TimeStep = 1.0f / 60.0f;
        [NonSerialized] private const float Density = 500.0f;
        
        // Serializables
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
        [SerializeField] private bool drawBarChart;
        
        [Header("Simulation Settings")]
        [SerializeField] private SIMULATION_SIZE simulationSize = SIMULATION_SIZE.MEDIUM;
        [SerializeField] private Mesh sphereMesh;
        [SerializeField] private Mesh cylinderMesh;
        [SerializeField] private Bounds barChartBounds;
        [SerializeField] [Range(0f,5f)] private float deathPlaneHeight;
        [SerializeField] private bool run = true;
        [SerializeField] private bool stopDemo;

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
        public void StartDemo() {
            if (hasStarted) return;
            try
            {
                run = true;
                fluidBoundaryObjects = new List<FluidBoundaryObject>();
                GetFluidBoundaryObjects();
                GetFluidObjects();
                GetDeathPlane();
                LoggingUtility.LogWithColor($"Fluid Demo: Found {fluidBoundaryObjects.Count} fluidBoundaryObjects!",Color.green);
                LoggingUtility.LogWithColor("Fluid Demo: Found {fluidObjects.Length} fluidObjects!",Color.green);
                
                fluidContainerizer = GetComponentInChildren<FluidContainerizer>();
                
                CreateBarChart();
                CreateFunnel();
                CreateDeathPlane();

                CreateFluid();
                CreateBoundary();
                
                var bounds = new Bounds(transform.position,simulationBounds.size);
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

        private void CreateDeathPlane() => 
            DeathPlane = DeathPlaneFactory.CreateDeathPlane(transform, 
                                               fluidContainerizer.meshBounds, 
                                               barChart.bounds, 
                                               Radius() * 2);

        private void CreateBarChart() =>
            fluidBoundaryObjects.Add(
                barChart = CylinderBarFactory.CreateBarChart(transform, 
                                                             barChartBounds.center, 
                                                             barChartBounds.size));

        private void CreateFunnel() => 
            fluidBoundaryObjects.Add(
                FunnelFactory.CreateFunnel(transform,
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
        public float Radius() =>  simulationSize switch {
            SIMULATION_SIZE.LOW => 0.1f,
            SIMULATION_SIZE.MEDIUM => 0.08f,
            SIMULATION_SIZE.HIGH => 0.06f,
            _ => 0.08f
        };


        public void DeathPlaneSliderHasChanged(Slider slider) => DeathPlane.SliderHasChanged(slider.value);
            
       
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Searches child gameObjects for FluidBoundaryObjects
        /// </summary>
        private void GetFluidBoundaryObjects() => fluidBoundaryObjects.AddRange(GetComponentsInChildren<FluidBoundaryObject>());
        
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Searches child gameObjects for FluidObject components
        /// </summary>
        private void GetFluidObjects() => fluidObjects = GetComponentsInChildren<FluidObject>();

        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Searches child gameObjects for a DeathPlane component
        /// </summary>
        private void GetDeathPlane() => DeathPlane = GetComponentInChildren<DeathPlane>();

        private void Update() {
            if (wasError) return;
            if (!hasStarted){
                if (run) StartDemo();
                return;
            }
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            
            if (run) DemoStep();
            
            volume.Hide = !drawFluidVolume;
            
            if (drawBoundaryParticles) 
                DrawBoundaryParticles();
            if (drawFluidParticles) 
                DrawFluidParticles();
            if (stopDemo) StopDemo();
        }

        private void StopDemo(){
            run = false;
            boundary?.Dispose();
            drawBoundaryParticles = false;
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
                       sphereMesh, 
                       fluidParticleMat, 
                       0);
        
        /// <summary>
        /// Draws the boundary particles
        /// </summary>
        private void DrawBoundaryParticles() => 
            boundary.Draw(mainCamera, 
                          sphereMesh, 
                          boundaryParticleMat, 
                          0, 
                          Color.red, 
                          DeathPlane.transform.position, 
                          DeathPlane.size);

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
            // if (drawBarChart) DrawBarChartGizmo();
            // if (drawBarChart && barChart!=null) DrawBarChartGizmo2();
            Gizmos.color = Color.red;
            if (drawBarChart && cylinderMesh!=null) DrawBarCylinderGizmo();
        }

        private void DrawBarCylinderGizmo() => Gizmos.DrawWireMesh(cylinderMesh,
                                                                   transform.position 
                                                                 + barChartBounds.center,
                                                                   Quaternion.identity,
                                                                   new Vector3(barChartBounds.size.x, 
                                                                               barChartBounds.size.y/2f,
                                                                               barChartBounds.size.z));

        /// <summary>
        /// Draws a gizmo showing the simulation bounds
        /// </summary>
        private void DrawSimulationBounds() => 
            Gizmos.DrawWireCube(transform.position+simulationBounds.center,simulationBounds.size);

    }
}
