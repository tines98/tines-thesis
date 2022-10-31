using System;
using System.Collections.Generic;
using PBDFluid;
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
        [SerializeField] private bool drawMeshBox;

        private Box3 bounds;
        private FluidBodyMeshDemo fluidBodyMeshDemo;
        private Box3 meshBox;

        private GameObject nonVoxelizedGameObject;
        private Vector3Int numVoxels;
        private GameObject voxelizedGameObject;


        public MeshVoxelizer Voxelizer;

        // Voxels included in the mesh
        public List<Box3> Voxels;

        void Start()
        {
            if (partOfFluidSim) {
                fluidBodyMeshDemo = GetComponentInParent<FluidBodyMeshDemo>();
                Assert.IsNotNull(fluidBodyMeshDemo);
                radius = fluidBodyMeshDemo.Radius();
            }
            StartVoxelization();
        }

        /// <summary>
        /// Starts the voxelization process.
        /// </summary>
        private void StartVoxelization()
        {
            var filter = GetComponent<MeshFilter>();
            var meshRenderer = GetComponent<MeshRenderer>();
            if (filter == null || meshRenderer == null) {
                filter = GetComponentInChildren<MeshFilter>();
                meshRenderer = GetComponentInChildren<MeshRenderer>();
            }
            
            if (filter == null || meshRenderer == null) return;
            meshRenderer.enabled = false;

            var mesh = filter.mesh;
            Assert.IsNotNull(mesh);
            var mat = meshRenderer.material;
            nonVoxelizedGameObject = filter.gameObject;
            
            bounds = new Box3(mesh.bounds.min, mesh.bounds.max);

            var center = nonVoxelizedGameObject.transform.position;
            
            numVoxels = new Vector3Int(
                (int) Math.Ceiling(meshBox.Size.x / (radius * 2)),
                (int) Math.Ceiling(meshBox.Size.y / (radius * 2)),
                (int) Math.Ceiling(meshBox.Size.z / (radius * 2))
            );
            
            Voxelizer = new MeshVoxelizer(numVoxels.x, numVoxels.y, numVoxels.z);
            Voxelizer.Voxelize(mesh.vertices, mesh.triangles, bounds);
            
            
            var localMin = meshBox.Min - center;
            mesh = CreateMesh(Voxelizer.Voxels, Scale(), localMin);

            CreateVoxelizedGameObject(mesh, mat, nonVoxelizedGameObject.transform);
        }

        /// <returns>The scale of a voxel box</returns>
        Vector3 Scale() => new Vector3(
            meshBox.Size.x / numVoxels.x,
            meshBox.Size.y / numVoxels.y,
            meshBox.Size.z / numVoxels.z);

        /// <summary>
        /// Creates the voxelized GameObject
        /// </summary>
        /// <param name="mesh">Mesh for the created gameObject</param>
        /// <param name="mat">Material for the created gameObject</param>
        /// <param name="copyTransform">Transform to copy from</param>
        private void CreateVoxelizedGameObject(Mesh mesh, Material mat, Transform copyTransform)
        {
            voxelizedGameObject = new GameObject("Voxelized") {
                transform = {
                    parent = transform,
                    position = copyTransform.position,
                    rotation = copyTransform.rotation,
                }
            };

            var filter = voxelizedGameObject.AddComponent<MeshFilter>();
            var meshRenderer = voxelizedGameObject.AddComponent<MeshRenderer>();

            filter.mesh = mesh;
            meshRenderer.material = mat;
            
            if (!partOfFluidSim) return;
            // Add components needed for fluid simulation
            voxelizedGameObject.AddComponent<FluidContainerizer>();
            voxelizedGameObject.AddComponent<FluidBoundaryVoxels>();
            voxelizedGameObject.AddComponent<FluidVoxels>();
            voxelizedGameObject.AddComponent<DeathPlaneCulling>();
        } // ReSharper disable Unity.PerformanceAnalysis
        public void HideVoxelizedMesh() => voxelizedGameObject.GetComponent<Renderer>().enabled = false;

        /// <returns>True if voxelized gameObject creation is complete</returns>
        public bool IsFinished() => voxelizedGameObject != null;

        public Matrix4x4 GetVoxelizedMeshMatrix() => voxelizedGameObject.transform.localToWorldMatrix;


        /// <param name="x">X coordinate in the voxel grid</param>
        /// <param name="y">Y coordinate in the voxel grid</param>
        /// <param name="z">Z coordinate in the voxel grid</param>
        /// <returns>A properly scaled and positioned Box3 representing the voxel at the given point</returns>
        public Box3 GetVoxel(int x, int y, int z)
        {
            var point = new Vector3(x, y, z);
            var scale = Scale();
            Assert.IsTrue(numVoxels.x > 0 || numVoxels.y > 0 || numVoxels.z > 0, "Size is 0");
            Assert.IsTrue(scale.x > 0 || scale.y > 0 || scale.z > 0, "Scale is 0");
            var min = meshBox.Min + Vector3.Scale(point, scale);
            var max = min + scale;
            var box = new Box3(min, max);
            return box;
        }

        /// <summary>
        /// Creates a mesh from the result of the voxelization
        /// </summary>
        /// <param name="voxels">Int grid representing which cells are within the original mesh</param>
        /// <param name="scale">Scaling vector</param>
        /// <param name="min">Min vector to place the mesh at</param>
        /// <returns>Voxelized mesh created</returns>
        private Mesh CreateMesh(int[,,] voxels, Vector3 scale, Vector3 min)
        {
            var verts = new List<Vector3>();
            var indices = new List<int>();
            Voxels = new List<Box3>();

            for (var z = 0; z < numVoxels.z; z++)
            {
                for (var y = 0; y < numVoxels.y; y++)
                {
                    for (var x = 0; x < numVoxels.x; x++)
                    {
                        if (voxels[x, y, z] != 1) continue;
                        
                        var pos = min + new Vector3(x * scale.x, y * scale.y, z * scale.z);
                        
                        Voxels.Add(GetVoxel(x,y,z));

                        if (x == numVoxels.x - 1 || voxels[x + 1, y, z] == 0)
                            AddRightQuad(verts, indices, scale, pos);

                        if (x == 0 || voxels[x - 1, y, z] == 0)
                            AddLeftQuad(verts, indices, scale, pos);

                        if (y == numVoxels.y - 1 || voxels[x, y + 1, z] == 0)
                            AddTopQuad(verts, indices, scale, pos);

                        if (y == 0 || voxels[x, y - 1, z] == 0)
                            AddBottomQuad(verts, indices, scale, pos);

                        if (z == numVoxels.z - 1 || voxels[x, y, z + 1] == 0)
                            AddFrontQuad(verts, indices, scale, pos);

                        if (z == 0 || voxels[x, y, z - 1] == 0)
                            AddBackQuad(verts, indices, scale, pos);
                    }
                }
            }

            if (verts.Count > 65000)
            {
                Debug.Log("Mesh has too many verts. You will have to add code to split it up.");
                return new Mesh();
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(indices, 0);

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }

        private void AddRightQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
        {
            int count = verts.Count;

            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 0 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 0 * scale.z));

            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 0 * scale.z));

            indices.Add(count + 2);
            indices.Add(count + 1);
            indices.Add(count + 0);
            indices.Add(count + 5);
            indices.Add(count + 4);
            indices.Add(count + 3);
        }

        private void AddLeftQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
        {
            int count = verts.Count;

            verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 0 * scale.z));
            verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 0 * scale.z));

            verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 0 * scale.z));

            indices.Add(count + 0);
            indices.Add(count + 1);
            indices.Add(count + 2);
            indices.Add(count + 3);
            indices.Add(count + 4);
            indices.Add(count + 5);
        }

        private void AddTopQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
        {
            int count = verts.Count;

            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 0 * scale.z));
            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 0 * scale.z));

            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 0 * scale.z));

            indices.Add(count + 0);
            indices.Add(count + 1);
            indices.Add(count + 2);
            indices.Add(count + 3);
            indices.Add(count + 4);
            indices.Add(count + 5);
        }

        private void AddBottomQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
        {
            int count = verts.Count;

            verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 0 * scale.z));
            verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 0 * scale.z));

            verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 0 * scale.z));

            indices.Add(count + 2);
            indices.Add(count + 1);
            indices.Add(count + 0);
            indices.Add(count + 5);
            indices.Add(count + 4);
            indices.Add(count + 3);
        }

        private void AddFrontQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
        {
            int count = verts.Count;

            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 1 * scale.z));

            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 1 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 1 * scale.z));

            indices.Add(count + 2);
            indices.Add(count + 1);
            indices.Add(count + 0);
            indices.Add(count + 5);
            indices.Add(count + 4);
            indices.Add(count + 3);
        }

        private void AddBackQuad(List<Vector3> verts, List<int> indices, Vector3 scale, Vector3 pos)
        {
            int count = verts.Count;

            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 0 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 0 * scale.z));
            verts.Add(pos + new Vector3(0 * scale.x, 0 * scale.y, 0 * scale.z));

            verts.Add(pos + new Vector3(0 * scale.x, 1 * scale.y, 0 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 1 * scale.y, 0 * scale.z));
            verts.Add(pos + new Vector3(1 * scale.x, 0 * scale.y, 0 * scale.z));

            indices.Add(count + 0);
            indices.Add(count + 1);
            indices.Add(count + 2);
            indices.Add(count + 3);
            indices.Add(count + 4);
            indices.Add(count + 5);
        }

        //GIZMOS

        private void DrawMeshBoxGizmo() => Gizmos.DrawWireCube(meshBox.Center, 
                                                               meshBox.Size);

        private void DrawBoundsGizmo(Matrix4x4 trs) => Gizmos.DrawWireCube(trs.MultiplyPoint(bounds.Center), 
                                                                        trs.MultiplyVector(bounds.Size));

        private void DrawAabbTreeGizmo(Matrix4x4 trs) => Voxelizer?.Bounds.ForEach(box => Gizmos.DrawWireCube(trs.MultiplyPoint(box.Center), trs.MultiplyVector(box.Size)));


        private void OnDrawGizmos()
        {
            if (voxelizedGameObject == null) return;
            
            var trs = voxelizedGameObject.transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            if (drawAABBTree) DrawAabbTreeGizmo(trs);

            Gizmos.color = Color.blue;
            if (drawBounds) DrawBoundsGizmo(trs);
            
            Gizmos.color = Color.magenta;
            if (drawMeshBox) DrawMeshBoxGizmo();
        }
    }
}