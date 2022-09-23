using UnityEngine;
using System.Collections.Generic;
using MeshVoxelizer.Scripts;
using PBDFluid.Scripts;

namespace PBDFluid
{
    public class FluidBodyMeshDemo : MonoBehaviour
    {
        //Constants
        private const float TimeStep = 1.0f / 60.0f;
        //Serialized Fields
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Bounds simulationBounds;
        
        [SerializeField] private VoxelizerDemo fluidBounds;
        [SerializeField] private SerializedMatrix[] boundsMatrices; 
        [SerializeField] private List<Vector3> boundarySizes;

        [Header("Materials")]
        [SerializeField] private Material fluidParticleMat;
        [SerializeField] private Material boundaryParticleMat;
        [SerializeField] private Material volumeMat;
        
        [Header("Render Booleans")]
        [SerializeField] private bool drawGrid;
        [SerializeField] private bool drawBoundaryParticles;
        [SerializeField] private bool drawFluidParticles;
        [SerializeField] private bool drawFluidVolume;
        [SerializeField] private bool drawExteriorVoxels; 
        [SerializeField] private bool drawInteriorVoxels;
        
        [Header("Simulation Settings")]
        [SerializeField] private SIMULATION_SIZE simulationSize = SIMULATION_SIZE.MEDIUM;
        [SerializeField] private bool run = true;
        [SerializeField] private Mesh sphereMesh;
        
        private float radius = 0.01f;
        private float density;

        private bool hasStarted;
        private FluidBody fluid;
        private FluidBoundary boundary;
        private FluidSolver solver;
        private RenderVolume volume;
        private bool wasError;
        private ParticlesFromSeveralBounds particleSource;
        private int[] particles2Bounds;
        private FluidContainerFromMesh fluidContainerFromMesh;

        // ReSharper disable Unity.PerformanceAnalysis
        private void StartDemo()
        {
            radius = 0.08f;
            density = 1000.0f;

            //A smaller radius means more particles.
            //If the number of particles is to low or high
            //the bitonic sort shader will throw a exception 
            //as it has a set range it can handle but this 
            //can be manually changes in the BitonicSort.cs script.

            //A smaller radius may also require more solver steps
            //as the boundary is thinner and the fluid many step
            //through and leak out.

            switch(simulationSize)
            {
                case SIMULATION_SIZE.LOW:
                    radius = 0.1f;
                    break;

                case SIMULATION_SIZE.MEDIUM:
                    radius = 0.08f;
                    break;

                case SIMULATION_SIZE.HIGH:
                    radius = 0.06f;
                    break;
            }

            try
            {
                fluidContainerFromMesh = new FluidContainerFromMesh(fluidBounds, radius, density);
                CreateBoundaries();
                CreateFluid();
                
                var bounds = simulationBounds;
                fluid.Bounds = bounds;
                solver = new FluidSolver(fluid, bounds, boundary);

                volume = new RenderVolume(bounds, radius);
                volume.CreateMesh(volumeMat);
            }
            catch{
                wasError = true;
                throw;
            }
            hasStarted = true;
            fluidBounds.HideVoxelizedMesh();
        }

        private void Update()
        {
            if(wasError) return;
            if (!hasStarted){
                if (run) StartDemo();
                return;
            }
            if (run) {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                solver.StepPhysics(TimeStep, particles2Bounds, BoundsMatricesAsMatrix());
                volume.FillVolume(fluid, solver.Hash, solver.Kernel);
            }

            volume.Hide = !drawFluidVolume;

            if (drawBoundaryParticles)
                boundary.Draw(mainCamera, sphereMesh, boundaryParticleMat, 0, Color.red);
            if (drawFluidParticles)
                fluid.Draw(mainCamera, sphereMesh, fluidParticleMat, 0);
        }

        private void OnDestroy()
        {
            boundary?.Dispose();
            fluid.Dispose();
            solver.Dispose();
            volume.Dispose();
        }

        private Matrix4x4[] BoundsMatricesAsMatrix() {
            var extra = fluidBounds.IsFinished() ? 1 : 0;
            var result = new Matrix4x4[boundsMatrices.Length + extra];
            
            int i;
            for (i = 0; i < boundsMatrices.Length; i++)
                result[i] = transform.localToWorldMatrix * boundsMatrices[i].GetMatrix();
            
            if (extra>0)
                result[i] = transform.localToWorldMatrix * fluidBounds.GetVoxelizedMeshMatrix();
            
            return result;
        }

        private void OnRenderObject() {
            var cam = Camera.current;
            if (cam != Camera.main) return;
            
            if(drawGrid)
                solver.Hash.DrawGrid(cam, Color.yellow);
        }

        private ParticlesFromBounds CreateBoundary(Bounds bounds)
        {
            Bounds outerBounds = new Bounds();
            var center = bounds.center;
            var size = bounds.size;
            outerBounds.SetMinMax(center-(size/2.0f), center+(size/2.0f));
                
            //Make the boundary 1 particle thick.
            //The multiple by 1.2 adds a little of extra
            //thickness in case the radius does not evenly
            //divide into the bounds size. You might have
            //particles missing from one side of the source
            //bounds other wise.
            const float thickness = 1;
            var diameter = radius * 2;
            size.x -= diameter * thickness * 1.2f;
            size.y -= diameter * thickness * 1.2f;
            size.z -= diameter * thickness * 1.2f;
            var innerBounds = new Bounds();
            innerBounds.SetMinMax(center-(size/2.0f), center+(size/2.0f));
            //The source will create a array of particles
            //evenly spaced between the inner and outer bounds.
            return new ParticlesFromBounds(diameter, outerBounds,innerBounds);
        }
        private void CreateBoundaries() {
            var particlesFromBoundsArray = new ParticleSource[boundarySizes.Count +1 ];
            int i;
            for (i=0; i < boundarySizes.Count; i++)
            {
                var bound = new Bounds(boundsMatrices[i].position, boundarySizes[i]);
                particlesFromBoundsArray[i] = CreateBoundary(bound);
            }
            particlesFromBoundsArray[i] = fluidContainerFromMesh.BoundaryParticleSource;
            
            particleSource = new ParticlesFromSeveralBounds(radius * 2, particlesFromBoundsArray);
            particleSource.CreateParticles();
            
            particles2Bounds = particleSource.Particle2MatrixMap.ToArray();
            boundary = new FluidBoundary(particleSource, radius, density, particles2Bounds, BoundsMatricesAsMatrix());
        }
        
        private void CreateFluid() {
            fluidContainerFromMesh.FluidParticleSource.CreateParticles();
            fluid = new FluidBody(fluidContainerFromMesh.FluidParticleSource, radius, density,fluidBounds.GetVoxelizedMeshMatrix());
        }
        
        private void OnDrawGizmos() {
            //Simulation Bounds
            Gizmos.color = Color.yellow;
            DrawSimulationBounds();

            //Extra Boundaries
            Gizmos.color = Color.green;
            DrawBoundariesGizmo();
            
            //Interior Voxels
            Gizmos.color = Color.blue;
            if (drawInteriorVoxels)
                fluidContainerFromMesh?.DrawInteriorVoxelsGizmo();

            //Exterior Voxels
            Gizmos.color = Color.green;
            if (drawExteriorVoxels) 
                fluidContainerFromMesh?.DrawExteriorVoxelsGizmo();
        }

        private void DrawSimulationBounds() => 
            Gizmos.DrawWireCube(transform.position+simulationBounds.center,simulationBounds.size);
        
        private void DrawBoundariesGizmo()
        {
            var a = BoundsMatricesAsMatrix();
            for (var i = 0; i < a.Length; i++) {
                // var pos = transform.position + boundsMatrices[i].position;
                var pos = a[i].MultiplyPoint(Vector3.zero);
                var size = i < boundarySizes.Count ? boundarySizes[i] : Vector3.one;
                CustomGizmos.DrawRotatableCubeGizmo(a[i],pos,size);
            }
        }

    }
}
