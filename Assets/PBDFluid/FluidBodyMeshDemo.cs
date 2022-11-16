using System;
using UnityEngine;
using PBDFluid.Scripts;
using UnityEngine.Assertions;

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
        
        [Header("Simulation Settings")]
        [SerializeField] private SIMULATION_SIZE simulationSize = SIMULATION_SIZE.MEDIUM;
        [SerializeField] private bool run = true;
        [SerializeField] private Mesh sphereMesh;

        // Fluid & Boundary Objects
        private FluidContainerizer fluidContainerizer;
        private BarChart barChart;
        private FluidBoundaryObject[] fluidBoundaryObjects;
        private FluidObject[] fluidObjects;
        
        // Fluid Demo Objects
        public DeathPlane deathPlane;
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
                GetFluidBoundaryObjects();
                GetFluidObjects();
                GetDeathPlane();
                LoggingUtility.LogWithColor($"Fluid Demo: Found {fluidBoundaryObjects.Length} fluidBoundaryObjects!",Color.green);
                LoggingUtility.LogWithColor("Fluid Demo: Found {fluidObjects.Length} fluidObjects!",Color.green);
                
                barChart = GetComponentInChildren<BarChart>();
                fluidContainerizer = GetComponentInChildren<FluidContainerizer>();
                
                //Place and resize DeathPlane
                var bar = barChart.barBoundsList[0];
                var barTRS = barChart.transform.localToWorldMatrix;
                PlaceDeathPlane(barTRS,bar.center,
                                bar.center.y + bar.size.y);
                ResizeDeathPlane(fluidContainerizer.meshSize);
                
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

        
        /// <summary>
        /// Places the DeathPlane at the given position and height 
        /// </summary>
        /// <param name="barChartPos">Center of a Bar in the barchart</param>
        /// <param name="y">height in which to place the DeathPlane</param>
        /// <param name="trs">TRS Matrix to transform the point</param>
        private void PlaceDeathPlane(Matrix4x4 trs, Vector3 barChartPos, float y) => 
            deathPlane.transform.position = trs.MultiplyPoint(
                new Vector3(barChartPos.x,
                            barChartPos.y + y - (Radius() * 2), //Subtract diameter to make up for mesh dilation
                            barChartPos.z));

        
        /// <summary>
        /// Resizes the DeathPlane based on the given bounds
        /// </summary>
        /// <param name="meshSize">Bounds of the voxelized mesh</param>
        private void ResizeDeathPlane(Vector3 meshSize) => 
            deathPlane.size = new Vector3(meshSize.x*2,
                                          deathPlane.size.y,
                                          meshSize.z*2);
        
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Searches child gameObjects for FluidBoundaryObjects
        /// </summary>
        private void GetFluidBoundaryObjects() => fluidBoundaryObjects = GetComponentsInChildren<FluidBoundaryObject>();
        
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Searches child gameObjects for FluidObject components
        /// </summary>
        private void GetFluidObjects() => fluidObjects = GetComponentsInChildren<FluidObject>();

        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Searches child gameObjects for a DeathPlane component
        /// </summary>
        private void GetDeathPlane() => deathPlane = GetComponentInChildren<DeathPlane>();

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
                
        }

        /// <summary>
        /// Updates the solver and volume
        /// </summary>
        private void DemoStep(){
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            solver.StepPhysics(TimeStep, 
                               deathPlane.transform.position, 
                               deathPlane.size);
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
                          deathPlane.transform.position, 
                          deathPlane.size);

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
