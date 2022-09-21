using UnityEngine;
using System.Collections.Generic;
using MeshVoxelizerProject;

namespace PBDFluid
{
    public class FluidBodyMeshDemo : MonoBehaviour
    {
        //Constants
        private const float timeStep = 1.0f / 60.0f;
        //Serialized Fields
        public Camera m_mainCamera;
        public Bounds simulationBounds;
        
        public VoxelizerDemo fluidBounds;
        public SerializedMatrix[] boundsMatrices; 
        public List<Vector3> boundarySizes;

        [Header("Materials")]
        public Material m_fluidParticleMat;
        public Material m_boundaryParticleMat;
        public Material m_volumeMat;

        [Header("Render Booleans")]
        public bool m_drawLines = true;
        public bool m_drawGrid = false;
        public bool m_drawBoundaryParticles = false;
        public bool m_drawFluidParticles = false;
        public bool m_drawFluidVolume = true;
        public bool m_drawExteriorVoxels;
        public bool m_drawInteriorVoxels;

        [Header("Simulation Settings")]
        public SIMULATION_SIZE m_simulationSize = SIMULATION_SIZE.MEDIUM;
        public bool m_run = true;
        public Mesh m_sphereMesh;
        
        private float radius = 0.01f;
        private float density;

        private bool m_hasStarted;
        private FluidBody m_fluid;
        private FluidBoundary m_boundary;
        private FluidSolver m_solver;
        private RenderVolume m_volume;
        private bool wasError;
        private ParticlesFromSeveralBounds particleSource;
        private int[] particles2Bounds;
        private FluidContainerFromMesh m_fluidContainerFromMesh;

        private void StartDemo()
        {
            radius = 0.08f;
            density = 1000.0f;

            //A smaller radius means more particles.
            //If the number of particles is to low or high
            //the bitonic sort shader will throw a exception 
            //as it has a set range it can handle but this 
            //can be manually changes in the BitonicSort.cs script.

            //A smaller radius may also requre more solver steps
            //as the boundary is thinner and the fluid many step
            //through and leak out.

            switch(m_simulationSize)
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
                m_fluidContainerFromMesh = new FluidContainerFromMesh(fluidBounds, radius, density);
                CreateBoundaries();
                m_fluidContainerFromMesh.FluidParticleSource.CreateParticles();
                m_fluid = new FluidBody(m_fluidContainerFromMesh.FluidParticleSource, radius, density,fluidBounds.GetVoxelizedMeshMatrix());
                
                var bounds = simulationBounds;
                m_fluid.Bounds = bounds;
                m_solver = new FluidSolver(m_fluid, bounds, m_boundary);

                m_volume = new RenderVolume(bounds, radius);
                m_volume.CreateMesh(m_volumeMat);
            }
            catch{
                wasError = true;
                throw;
            }
            m_hasStarted = true;
            fluidBounds.HideVoxelizedMesh();
        }

        private void Update()
        {
            if(wasError) return;
            if (!m_hasStarted){
                if (m_run){
                    StartDemo();
                }
                return;
            }
            if (m_run)
            {
                m_solver.StepPhysics(timeStep, particles2Bounds, BoundsMatricesAsMatrix());
                m_volume.FillVolume(m_fluid, m_solver.Hash, m_solver.Kernel);
            }

            m_volume.Hide = !m_drawFluidVolume;

            if (m_drawBoundaryParticles){
                m_boundary.Draw(m_mainCamera, m_sphereMesh, m_boundaryParticleMat, 0, Color.red);
            }

            if (m_drawFluidParticles){
                m_fluid.Draw(m_mainCamera, m_sphereMesh, m_fluidParticleMat, 0);
            }
        }

        private void OnDestroy()
        {
            m_boundary.Dispose();
            m_fluid.Dispose();
            m_solver.Dispose();
            m_volume.Dispose();
        }

        private Matrix4x4[] BoundsMatricesAsMatrix()
        {
            var result = new Matrix4x4[boundsMatrices.Length+1];
            int i;
            for (i = 0; i < boundsMatrices.Length; i++) {
                result[i] = boundsMatrices[i].GetMatrix();
            }
            result[i] = fluidBounds.GetVoxelizedMeshMatrix();
            return result;
        }

        private void OnRenderObject()
        {
            var cam = Camera.current;
            if (cam != Camera.main) return;

            if (m_drawLines){                
                //Outer Container Bounds
                // foreach (Bounds bounds in particleSource.BoundsList){
                //     DrawBounds(camera, Color.green, bounds);
                // }
                // foreach (Bounds bounds in particleSource.Exclusion){
                //     DrawBounds(camera, Color.red, bounds);
                // }

                // DrawBounds(camera, Color.blue, m_fluidSource);
            }

            if(m_drawGrid){
                m_solver.Hash.DrawGrid(cam, Color.yellow);
            }
        }

        private ParticlesFromBounds CreateBoundary(Bounds bounds)
        {
            Bounds outerBounds = new Bounds();
            var center = transform.position + bounds.center;
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
            Bounds innerBounds = new Bounds();
            innerBounds.SetMinMax(center-(size/2.0f), center+(size/2.0f));
            //The source will create a array of particles
            //evenly spaced between the inner and outer bounds.
            return new ParticlesFromBounds(diameter, outerBounds,innerBounds);
        }
        private void CreateBoundaries() {
            var particlesFromBoundsArray = new ParticleSource[boundarySizes.Count +1 ];
            var boundsVectors = new Vector3[boundarySizes.Count+1];
            int i;
            for (i=0; i < boundarySizes.Count; i++)
            {
                var bound = new Bounds(boundsMatrices[i].position, boundarySizes[i]);
                particlesFromBoundsArray[i] = CreateBoundary(bound);
                boundsVectors[i] = bound.center;
            }

            particlesFromBoundsArray[i] = m_fluidContainerFromMesh.BoundaryParticleSource;
            boundsVectors[i] = fluidBounds.bounds.Center;

            particleSource = new ParticlesFromSeveralBounds(radius * 2, particlesFromBoundsArray, boundsVectors);
            particleSource.CreateParticles();
            particles2Bounds = particleSource.particle2MatrixMap.ToArray();
            m_boundary = new FluidBoundary(particleSource, radius, density, particles2Bounds, BoundsMatricesAsMatrix());
        }
        
        private void CreateFluid() {
            //The source will create a array of particles
            //evenly spaced inside the bounds. 
            //Multiple the spacing by 0.9 to pack more
            //particles into bounds.
            var diameter = radius * 2;
            var bounds = new Bounds(fluidBounds.bounds.Center, fluidBounds.bounds.Size);
            var exclusion = new List<Bounds>();
            foreach (var nonVoxel in fluidBounds.NonVoxels) {
                exclusion.Add(new Bounds(nonVoxel.Center,nonVoxel.Size));
            }
            var particlesFromBounds = new ParticlesFromBounds(diameter, bounds, exclusion);
            Debug.Log("Fluid Particles = " + particlesFromBounds.NumParticles);
            m_fluid = new FluidBody(particlesFromBounds, radius, density, fluidBounds.GetVoxelizedMeshMatrix());
        }

        private static readonly IList<int> Cube = new[]
        {
            0, 1, 1, 2, 2, 3, 3, 0,
            4, 5, 5, 6, 6, 7, 7, 4,
            0, 4, 1, 5, 2, 6, 3, 7
        };

        Vector4[] m_corners = new Vector4[8];

        private void GetCorners(Bounds b)
        {
            m_corners[0] = new Vector4(b.min.x, b.min.y, b.min.z, 1);
            m_corners[1] = new Vector4(b.min.x, b.min.y, b.max.z, 1);
            m_corners[2] = new Vector4(b.max.x, b.min.y, b.max.z, 1);
            m_corners[3] = new Vector4(b.max.x, b.min.y, b.min.z, 1);

            m_corners[4] = new Vector4(b.min.x, b.max.y, b.min.z, 1);
            m_corners[5] = new Vector4(b.min.x, b.max.y, b.max.z, 1);
            m_corners[6] = new Vector4(b.max.x, b.max.y, b.max.z, 1);
            m_corners[7] = new Vector4(b.max.x, b.max.y, b.min.z, 1);
        }

        private void DrawBounds(Camera cam, Color col, Bounds bounds)
        {
            GetCorners(bounds);
            DrawLines.LineMode = LINE_MODE.LINES;
            DrawLines.Draw(cam, m_corners, col, Matrix4x4.identity, Cube);
        }

        private void OnDrawGizmos() {
            //Simulation Bounds
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position,simulationBounds.size);

            //Extra Boundaries
            for (var i = 0; i < boundarySizes.Count; i++) {
                Gizmos.color = Color.green;
                CustomGizmos.DrawRotatableCubeGizmo(boundsMatrices[i],boundarySizes[i]);
            }

            if (m_fluidContainerFromMesh != null)
            {
                if (m_drawInteriorVoxels)
                {
                    // m_fluidContainerFromMesh.DrawBoundaryGizmo();
                    m_fluidContainerFromMesh.DrawInteriorVoxelsGizmo();
                }

                if (m_drawExteriorVoxels)
                {
                    m_fluidContainerFromMesh.DrawExteriorVoxelsGizmo();
                }
            }
            
            Gizmos.DrawSphere(fluidBounds.bounds.Center,.1f);
        }

    }
}
