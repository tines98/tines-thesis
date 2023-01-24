using System;
using System.Collections.Generic;
using Demo;
using SimulationObjects;
using SimulationObjects.FluidBoundaryObject;
using SimulationObjects.FluidObject;
using UnityEngine;
using UnityEngine.Assertions;

namespace MeshVoxelizer.Scripts
{
    public class VoxelizerDemo : MonoBehaviour
    {
        /// <summary>
        /// Boolean to determine whether this voxelizer is part of a fluid simulation, or stand alone  
        /// </summary>
        public bool partOfFluidSim = true;

        /// <summary>
        /// half size of a voxel, is set by fluid simulation. or set manually if standalone
        /// </summary>
        public float radius;

        [Header("Gizmos")] 
        [SerializeField] private bool drawBounds;
        [SerializeField] private bool drawAABBTree;

        public Bounds meshBounds;
        private FluidDemo fluidDemo;

        private GameObject nonVoxelizedGameObject;
        [NonSerialized] public Vector3Int NumVoxels;
        private GameObject voxelizedGameObject;

        public MeshVoxelizer Voxelizer;
        

        // Voxels included in the mesh
        public List<Box3> Voxels;

        void Start()
        {
            if (partOfFluidSim) {
                fluidDemo = GetComponentInParent<FluidDemo>();
                Assert.IsNotNull(fluidDemo);
                radius = fluidDemo.Radius();
            }
            StartVoxelization();
            FillVoxels(Voxelizer.Voxels);
            Debug.Log($"num voxels is {Voxels.Count}");
        }

        private void StartVoxelization(){
            //Get filter and renderer
            var filter = GetMeshFilter();
            var meshRenderer = GetMeshRenderer();
            
            //Get non voxelized object
            nonVoxelizedGameObject = filter.gameObject;
            
            // Get mesh from model
            var mesh = filter.sharedMesh;
            meshBounds = mesh.bounds;

            // Get scaled mesh size
            var lossyScale = nonVoxelizedGameObject.transform.lossyScale;
            var meshSize = Vector3.Scale(meshBounds.size, 
                                         lossyScale);
            
            // Calculate num voxels along each axis
            NumVoxels = CalcNumVoxels(meshSize);
            Debug.Log("NumVoxels = " + NumVoxels);
            
            // Do Voxelization
            Voxelizer = new MeshVoxelizer(NumVoxels.x, 
                                          NumVoxels.y, 
                                          NumVoxels.z);
            Voxelizer.Voxelize(mesh.vertices,
                               mesh.triangles,
                               new Box3(meshBounds));
            
            // Create mesh
            mesh = MeshCreator.CreateMesh(Voxelizer.Voxels, 
                                          NumVoxels, 
                                          Scale(), 
                                          meshBounds.min);
            
            // Create the finished voxelized gameObject
            CreateVoxelizedGameObject(mesh, 
                                      meshRenderer.material, 
                                      nonVoxelizedGameObject.transform);
        }

        
        private void FillVoxels(int[,,] voxels){
            Voxels = new List<Box3>();
            for (var z = 0; z < NumVoxels.z; z++){
                for (var y = 0; y < NumVoxels.y; y++){
                    for (var x = 0; x < NumVoxels.x; x++){
                        if (voxels[x, y, z] != 1) continue;
                        Voxels.Add(GetVoxel(x,y,z));
                    }
                }
            }
        }
        

        private MeshFilter GetMeshFilter(){
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null) 
                meshFilter = GetComponentInChildren<MeshFilter>();
            return meshFilter;
        }
        
        
        private MeshRenderer GetMeshRenderer(){
            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null) 
                meshRenderer = GetComponentInChildren<MeshRenderer>();
            return meshRenderer;
        }

        
        private Vector3Int CalcNumVoxels(Vector3 size) =>
            new Vector3Int((int) Math.Abs(Math.Ceiling(size.x / (radius * 2))),
                           (int) Math.Abs(Math.Ceiling(size.y / (radius * 2))),
                           (int) Math.Abs(Math.Ceiling(size.z / (radius * 2))));

        
        /// <returns>The scale of a voxel box</returns>
        Vector3 Scale() => new Vector3(
            meshBounds.size.x / NumVoxels.x,
            meshBounds.size.y / NumVoxels.y,
            meshBounds.size.z / NumVoxels.z);

        
        /// <summary>
        /// Creates the voxelized GameObject
        /// </summary>
        /// <param name="mesh">Mesh for the created gameObject</param>
        /// <param name="mat">Material for the created gameObject</param>
        /// <param name="copyTransform">Transform to copy from</param>
        private void CreateVoxelizedGameObject(Mesh mesh, Material mat, Transform copyTransform){
            voxelizedGameObject = new GameObject($"Voxelized {copyTransform.gameObject.name}") {
                transform = {
                    parent = copyTransform.parent,
                    localPosition = copyTransform.localPosition,
                    localRotation = copyTransform.localRotation,
                    localScale = copyTransform.lossyScale
                }
            };

            var filter = voxelizedGameObject.AddComponent<MeshFilter>();
            var meshRenderer = voxelizedGameObject.AddComponent<MeshRenderer>();
            filter.mesh = mesh;
            meshRenderer.material = mat;
            // meshRenderer.enabled = false;
            
            if (!partOfFluidSim) return;
            // Add components needed for fluid simulation
            voxelizedGameObject.AddComponent<FluidContainerizer>();
            voxelizedGameObject.AddComponent<FluidBoundaryVoxels>();
            voxelizedGameObject.AddComponent<FluidVoxels>();
            voxelizedGameObject.AddComponent<DeathPlaneCulling>();
        } 
        
        
        // ReSharper disable Unity.PerformanceAnalysis
        public void SetVoxelizedMeshVisibility(bool shouldBeVisible) => voxelizedGameObject.GetComponent<Renderer>().enabled = shouldBeVisible;

        
        /// <returns>True if voxelized gameObject creation is complete</returns>
        public bool IsFinished() => voxelizedGameObject != null;

        public Matrix4x4 GetVoxelizedMeshMatrix() => voxelizedGameObject.transform.localToWorldMatrix;


        /// <param name="x">X coordinate in the voxel grid</param>
        /// <param name="y">Y coordinate in the voxel grid</param>
        /// <param name="z">Z coordinate in the voxel grid</param>
        /// <returns>
        /// A properly scaled and positioned Box3 representing the voxel at the given point
        /// </returns>
        public Box3 GetVoxel(int x, int y, int z) => GetVoxel(new Vector3(x, y, z));
        
        
        /// <inheritdoc cref="GetVoxel(int,int,int)"/>
        public Box3 GetVoxel(Vector3 point){
            // Scale it to mesh bounds
            var scale = Scale();
            var min = meshBounds.min + Vector3.Scale(point, scale);
            var max = meshBounds.min + Vector3.Scale(point + Vector3.one, scale);
            return new Box3(min, max);
        }
        
        
        //GIZMOS
        private void OnDrawGizmos()
        {
            if (voxelizedGameObject == null) return;
            
            var trs = voxelizedGameObject.transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            if (drawAABBTree) DrawAabbTreeGizmo(trs);
            if (!drawBounds) return;
            Gizmos.color = Color.blue;
            DrawBoundsGizmo(trs);
            var mesh = nonVoxelizedGameObject.GetComponentInChildren<MeshFilter>().sharedMesh;
            DrawModelGizmo(mesh, nonVoxelizedGameObject.transform);
            
            if (nonVoxelizedGameObject == null) return;
            var mesh2 = voxelizedGameObject.GetComponent<MeshFilter>().sharedMesh;
            DrawModelGizmo(mesh2,voxelizedGameObject.transform);
        }
        
        private void DrawBoundsGizmo(Matrix4x4 trs) => 
            Gizmos.DrawWireCube(trs.MultiplyPoint(meshBounds.center), 
                                trs.MultiplyVector(meshBounds.size));


        private void DrawAabbTreeGizmo(Matrix4x4 trs) => 
            Voxelizer?.Bounds.ForEach(box => Gizmos.DrawWireCube(trs.MultiplyPoint(box.Center), 
                                                                 trs.MultiplyVector(box.Size)));

        
        private void DrawModelGizmo(Mesh mesh, Transform modelTransform) =>
            Gizmos.DrawWireMesh(mesh, 
                                modelTransform.position,
                                modelTransform.rotation,
                                modelTransform.lossyScale);
    }
}