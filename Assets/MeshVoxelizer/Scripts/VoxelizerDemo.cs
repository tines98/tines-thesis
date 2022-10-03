using System;
using System.Collections.Generic;
using PBDFluid;
using PBDFluid.Scripts;
using UnityEngine;
using UnityEngine.Assertions;

namespace MeshVoxelizer.Scripts
{
    public class VoxelizerDemo : MonoBehaviour
    {

        // [SerializeField] private Vector3Int size = new Vector3Int(16, 16, 16);
        private float radius;
        private Vector3Int numVoxels;

        [SerializeField] private bool drawAABBTree;

        public MeshVoxelizer Voxelizer;

        // Voxels included in the mesh
        public List<Box3> Voxels;
        public Box3 Bounds;

        [Header("Gizmos")] 
        [SerializeField] private bool drawBounds;


        private GameObject nonVoxelizedGameObject;
        private GameObject voxelizedGameObject;
        private FluidBodyMeshDemo fluidBodyMeshDemo;

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

            var objScale = nonVoxelizedGameObject.transform.localScale;
            var scaledMin = Vector3.Scale(mesh.bounds.min , objScale);
            var scaledMax = Vector3.Scale(mesh.bounds.max , objScale);
            Debug.Log($"scaledMin {scaledMin}, scaledMax {scaledMax}");
            Bounds = new Box3(scaledMin, scaledMax);

            numVoxels = new Vector3Int(
                (int) (Bounds.Size.x / radius),
                (int) (Bounds.Size.y / radius),
                (int) (Bounds.Size.z / radius)
            );
            
            Voxelizer = new MeshVoxelizer(numVoxels.x, numVoxels.y, numVoxels.z);
            Voxelizer.Voxelize(mesh.vertices, mesh.triangles, Bounds);
            

            mesh = CreateMesh(Voxelizer.Voxels, Scale(), Bounds.Min);

            CreateVoxelizedGameObject(mesh, mat, objScale);

            Debug.Log($"Num Voxels is {Voxels.Count}");
        }

        Vector3 Scale() => new Vector3(
            Bounds.Size.x / numVoxels.x,
            Bounds.Size.y / numVoxels.y,
            Bounds.Size.z / numVoxels.z);

        void Start()
        {
            fluidBodyMeshDemo = GetComponentInParent<FluidBodyMeshDemo>();
            Assert.IsNotNull(fluidBodyMeshDemo);
            radius = fluidBodyMeshDemo.Radius();
            StartVoxelization();
        }

        private void CreateVoxelizedGameObject(Mesh mesh, Material mat, Vector3 scale)
        {
            voxelizedGameObject = new GameObject("Voxelized") {
                transform = {
                    parent = transform,
                    position = nonVoxelizedGameObject.transform.position,
                    rotation = nonVoxelizedGameObject.transform.rotation,
                    localScale = scale
                }
            };

            var filter = voxelizedGameObject.AddComponent<MeshFilter>();
            var meshRenderer = voxelizedGameObject.AddComponent<MeshRenderer>();

            filter.mesh = mesh;
            meshRenderer.material = mat;

            voxelizedGameObject.AddComponent<FluidContainerizer>();
            voxelizedGameObject.AddComponent<FluidBoundaryVoxels>();
            voxelizedGameObject.AddComponent<FluidVoxels>();
        }

        private void OnRenderObject() {
            if (drawAABBTree) 
                Voxelizer?.Bounds.ForEach(box => DrawLines.DrawBounds(Camera.current, Color.red, box, transform.localToWorldMatrix));
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void HideVoxelizedMesh() => voxelizedGameObject.GetComponent<Renderer>().enabled = false;

        public bool IsFinished() => voxelizedGameObject != null;

        public Matrix4x4 GetVoxelizedMeshMatrix() => voxelizedGameObject.transform.localToWorldMatrix;

        public Box3 GetVoxel(int x, int y, int z)
        {
            var point = new Vector3(x, y, z);
            var scale = Scale();
            Assert.IsTrue(numVoxels.x > 0 || numVoxels.y > 0 || numVoxels.z > 0, "Size is 0");
            Assert.IsTrue(scale.x > 0 || scale.y > 0 || scale.z > 0, "Scale is 0");
            var min = Bounds.Min + Vector3.Scale(point, scale);
            var max = min + scale;
            var box = new Box3(min, max);
            Assert.IsTrue((box.Size.x - radius*1.2f) < 0.01f, $"boxSize{box.Size.x} is wrong {radius*1.2f} = {box.Size.x - radius*1.2f}");
            return box;
        }

        private Mesh CreateMesh(int[,,] voxels, Vector3 scale, Vector3 min)
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> indices = new List<int>();
            Voxels = new List<Box3>();

            for (int z = 0; z < numVoxels.z; z++)
            {
                for (int y = 0; y < numVoxels.y; y++)
                {
                    for (int x = 0; x < numVoxels.x; x++)
                    {
                        Assert.IsTrue(x<voxels.GetLength(0));
                        if (voxels[x, y, z] != 1) continue;
                        Vector3 pos = min + new Vector3(x * scale.x, y * scale.y, z * scale.z);
                        var box = new Box3(pos, pos + scale);


                        Voxels.Add(box);

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
        private void OnDrawGizmos()
        {
            if (drawBounds)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(Bounds.Center, Bounds.Size);
            }
        }
    }
}