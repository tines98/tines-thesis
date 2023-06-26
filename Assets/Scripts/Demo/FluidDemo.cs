using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Factories;
using PBDFluid.Scripts;
using SimulationObjects;
using SimulationObjects.FluidBoundaryObject;
using SimulationObjects.FluidObject;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using Utility;

namespace Demo{
    public class FluidDemo : MonoBehaviour{
        [NonSerialized] private const float Density = 1000.0f;

        [NonSerialized] public Bounds SimulationBounds = new Bounds(Vector3.zero, new Vector3(6, 10, 6));
        [NonSerialized] public ParticleSize ParticleSize = ParticleSize.Medium;
        [NonSerialized] public Bounds BarChartBounds;
        [NonSerialized] [Range(0f, 5f)] public float DeathPlaneHeight;

        [Header("Run")] 
        [SerializeField] private bool run;
        [SerializeField] private bool stopDemo;
        public float realVolume;
        
        public FluidDemoRenderSettings renderSettings = new();
        
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

        public String VolumeText;
        

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Starts the fluid simulation
        /// </summary>
        /// <remarks>Can be called from the outside, such as from UI</remarks>
        public void StartDemo(){
            if (hasStarted) return;
            run = true;
            fluidBoundaryObjects = new List<FluidBoundaryObject>();
            try{
                Debug.Log("Epsilon = " + Epsilon);
                GetFluidBoundaryObjects();
                GetFluidObjects();
                LoggingUtility.LogWithColor($"Fluid Demo: Found {fluidBoundaryObjects.Count} fluidBoundaryObjects!",
                                            Color.green);
                LoggingUtility.LogWithColor("Fluid Demo: Found {fluidObjects.Length} fluidObjects!", Color.green);

                fluidContainerizer = GetComponentInChildren<FluidContainerizer>();

                CreateBarChart();
                CreateFunnel();
                CreateDeathPlane();
                
                CreateFluid();
                CreateBoundary();

                fluid.Bounds = new Bounds(transform.position, 
                                          SimulationBounds.size);
                solver = new FluidSolver(fluid, 
                                         fluid.Bounds, 
                                         boundary);
                volume = new RenderVolume(fluid.Bounds, 
                                          Radius);
                volume.CreateMesh(renderSettings.volumeMaterial);
            }
            catch{
                wasError = true;
                throw;
            }

            hasStarted = true;
            StartCoroutine(DemoStep());
        }


        /// <summary>
        /// Creates a <see cref="DeathPlane"/> using the <see cref="DeathPlaneFactory"/>
        /// </summary>
        private void CreateDeathPlane() =>
            DeathPlane = DeathPlaneFactory.CreateDeathPlane(transform,
                                                            SimulationBounds,
                                                            fluidContainerizer.GlobalMeshBounds,
                                                            barChart.Bounds,
                                                            Radius * 2);


        /// <summary>
        /// Creates a <see cref="FluidBoundaryCylinderCup"/> using the <see cref="CylinderBarFactory"/>
        /// </summary>
        private void CreateBarChart() =>
            fluidBoundaryObjects.Add(barChart = CylinderBarFactory.CreateBarChart(transform, 
                                                                                  BarChartBounds.center, 
                                                                                  BarChartBounds.size, 
                                                                                  renderSettings.cylinderMesh, 
                                                                                  renderSettings.cylinderMaterial, 
                                                                                  Radius));


        /// <summary>
        /// Creates a <see cref="Funnel"/> using the <see cref="FunnelFactory"/>
        /// </summary>
        private void CreateFunnel() =>
            fluidBoundaryObjects.Add(FunnelFactory.CreateFunnel(transform,
                                                                barChart.Bounds,
                                                                fluidContainerizer.GlobalMeshBounds,
                                                                SimulationBounds,
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
        /// <value>The radius for particles</value>
        public float Radius =>
            ParticleSize switch{
                ParticleSize.Low => 0.1f,
                ParticleSize.Medium => 0.08f,
                ParticleSize.High => 0.06f,
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
        private void GetFluidObjects() => 
            fluidObjects = GetComponentsInChildren<FluidObject>();

        public void CreateText(){    
            var pos = transform.position + SimulationBounds.min + Vector3.right * 1.5f;
            var text = Instantiate(renderSettings.floatingTextPrefab, pos, Quaternion.identity, transform);
            var textMeshPro = text.GetComponent<TextMeshPro>();
            textMeshPro.text = Math.Round(realVolume, 2).ToString();
        }


        private void Update(){
            if (wasError) return;
            if (!hasStarted){
                if (run) StartDemo();
                return;
            }
            
            // if (run) DemoStep();

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
            StopCoroutine(DemoStep());
        }

        private float Epsilon => ParticleSize switch{
            ParticleSize.Low => 5f,
            ParticleSize.Medium => renderSettings.epsilon,
            ParticleSize.High => 60f,
            _ => throw new ArgumentOutOfRangeException()
        };

        private bool isRun => run;

        /// <summary>
        /// Updates the solver and volume
        /// </summary>
        private IEnumerator DemoStep(){ 
            while (run){
                UpdateDeathPlane();
                solver.StepPhysics(renderSettings.deltaTime,
                                   DeathPlane.transform.position,
                                   DeathPlane.size,
                                   renderSettings.overrideEpsilon 
                                       ? renderSettings.epsilon 
                                       : Epsilon);
                volume.FillVolume(fluid,
                                  solver.Hash,
                                  solver.Kernel);

                yield return new WaitForSeconds(renderSettings.deltaTime);
            }
        }
        

        private void UpdateDeathPlane() => DeathPlane.SliderHasChanged(DeathPlaneHeight);


        /// <summary>
        /// Draws the fluid particles
        /// </summary>
        private void DrawFluidParticles() =>
            fluid.Draw(null,
                       renderSettings.sphereMesh,
                       renderSettings.fluidParticleMaterial,
                       0);


        /// <summary>
        /// Draws the boundary particles
        /// </summary>
        private void DrawBoundaryParticles() =>
            boundary.Draw(null,
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
            var particleSources = from fluidBoundaryObject in fluidBoundaryObjects 
                                  select fluidBoundaryObject.ParticleSource;
            
            var particleSource = new ParticlesFromSeveralBounds(Radius * 2, 
                                                                particleSources.ToArray());
            particleSource.CreateParticles();
            boundary = new FluidBoundary(particleSource,
                                         Radius,
                                         Density);
        }


        /// <summary>
        /// Puts every fluid object into a particle source and creates a new FluidBody
        /// </summary>
        private void CreateFluid(){
            var particleSources = from fluidObject in fluidObjects 
                                  select fluidObject.ParticleSource;
            var particleSource = new ParticlesFromSeveralBounds(Radius * 2, particleSources.ToArray());
            particleSource.CreateParticles();
            fluid = new FluidBody(particleSource, Radius, Density, transform.localToWorldMatrix);
        }


        private void OnDrawGizmos(){
            //Simulation Bounds
            Gizmos.color = Color.yellow;
            if (renderSettings.drawSimulationBounds) DrawSimulationBounds();
            Gizmos.color = Color.white;
            if (renderSettings.drawBarChart && renderSettings.cylinderMesh != null) DrawBarCylinderGizmo();
            
            if (!run) return;
            var data = new Vector4[fluid.NumParticles];
            fluid.Positions.GetData(data);
            var anyZeroes = data.ToList().FindIndex((pos) => pos == Vector4.zero);
            if (anyZeroes>=0) Debug.Log($"Holy shit there is one at index {anyZeroes}");
            Gizmos.color = Color.blue;
            DrawFluidParticlesGizmo(data.ToList());
        }

        private void DrawFluidParticleGizmo(Vector4 particle) => 
            Gizmos.DrawWireSphere(transform.localToWorldMatrix * particle, Radius);

        private void DrawFluidParticlesGizmo(List<Vector4> data) => 
            data.ToList().ForEach(DrawFluidParticleGizmo); 


        /// <summary>
        /// Draws a gizmo showing the barchart cylinder
        /// </summary>
        private void DrawBarCylinderGizmo() =>
            Gizmos.DrawWireMesh(renderSettings.cylinderMesh,
                                transform.position + BarChartBounds.center,
                                Quaternion.identity,
                                new Vector3(BarChartBounds.size.x,
                                            BarChartBounds.size.y / 2f,
                                            BarChartBounds.size.z));


        /// <summary>
        /// Draws a gizmo showing the simulation bounds
        /// </summary>
        private void DrawSimulationBounds() =>
            Gizmos.DrawWireCube(transform.position + SimulationBounds.center, SimulationBounds.size);
    }
}