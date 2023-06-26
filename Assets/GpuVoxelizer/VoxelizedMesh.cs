#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using MeshVoxelizer.Scripts;
using Shaders.DeathPlaneCulling;
using SimulationObjects;
using SimulationObjects.FluidBoundaryObject;
using SimulationObjects.FluidObject;
using UnityEngine;
using UnityEngine.Profiling;

[ExecuteAlways]
public class VoxelizedMesh : MonoBehaviour
{
    [SerializeField] MeshFilter _meshFilter;
    [SerializeField] MeshCollider _meshCollider;
    [SerializeField] public float _halfSize = 0.05f;
    [SerializeField] Vector3 _boundsMin;

    [SerializeField] Material _gridPointMaterial;
    [SerializeField] int _gridPointCount;

    [SerializeField] Material _blocksMaterial;

    [SerializeField] ComputeShader _voxelizeComputeShader;
    ComputeBuffer _voxelsBuffer;
    ComputeBuffer _meshVerticesBuffer;
    ComputeBuffer _meshTrianglesBuffer;

    ComputeBuffer _pointsArgsBuffer;
    ComputeBuffer _blocksArgsBuffer;

    Mesh _voxelMesh;

    [SerializeField] bool _drawPointGrid;
    [SerializeField] bool _drawBlocks;
    public float realVolume;

    static readonly int BoundsMin = Shader.PropertyToID("_BoundsMin");
    static readonly int Voxels = Shader.PropertyToID("_Voxels");

    public Bounds MeshBounds => _meshCollider.bounds; 

    public Voxel[] _voxels;
    
    public struct Voxel
    {
        public Vector3 position;
        public float isSolid;
    }

    public void StartVoxelization()
    {
        _pointsArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        _blocksArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        _meshFilter = GetComponent<MeshFilter>();
        _voxelizeComputeShader = Resources.Load("VoxelizeMesh") as ComputeShader;
        if (TryGetComponent(out MeshCollider meshCollider))
        {
            _meshCollider = meshCollider;
        }
        else
        {
            _meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        
        VoxelizeMeshWithGPU();
        AddComponents();
        
        // Debug.Log($" VoxelCount of {gameObject.name} = {VoxelCount}");
        // Debug.Log("GridSize = " + GridSize);
    }

    public Voxel GetVoxelOnIndex(int x, int y, int z) => 
        _voxels[x 
              + y * GridSize.x 
              + z * GridSize.x * GridSize.y];

    public Box3 GetVoxel(int x, int y, int z){
        var voxPos = GetVoxelOnIndex(x, y, z).position;
        return new Box3(voxPos, voxPos + Vector3.one * _halfSize * 2f);
    }
        
    /// <inheritdoc cref="GetVoxel(int,int,int)"/>
    public Box3 GetVoxel(Vector3 point) => new(){
        Min = MeshBounds.min + point * _halfSize*2f,
        Max = MeshBounds.min + (point + Vector3.one) * _halfSize*2f
    };

    private Vector3 VoxelCount =>
        new(_meshCollider.bounds.extents.x / _halfSize,
            _meshCollider.bounds.extents.y / _halfSize,
            _meshCollider.bounds.extents.z / _halfSize);
    
    // private Vector3Int GridSize => new Vector3Int(Mathf.FloorToInt(VoxelCount.x), 
    //                                               Mathf.FloorToInt(VoxelCount.y),
    //                                               Mathf.FloorToInt(VoxelCount.z));
    private Vector3Int GridSize => new Vector3Int(Mathf.CeilToInt(VoxelCount.x), 
                                                  Mathf.CeilToInt(VoxelCount.y),
                                                  Mathf.CeilToInt(VoxelCount.z));
    
    
    public int[,,] GetVoxels(){
        var bounds = _meshCollider.bounds;
        var voxelCount = VoxelCount;
        var voxelsGrid = new int[GridSize.x, GridSize.y, GridSize.z];
        for (var z = 0; z < GridSize.z; z++){
            for (var y = 0; y < GridSize.y; y++){
                for (var x = 0; x < GridSize.x; x++){
                    var index = x 
                              + y * GridSize.x 
                              + z * GridSize.x * GridSize.y;
                    voxelsGrid[x,y,z] = _voxels[index].isSolid is > 0.5f and < 1.5f ? 1 : 0;
                }
            }
        }
        return voxelsGrid;
    }

    void AddComponents(){
        gameObject.AddComponent<FluidContainerizer>();
        gameObject.AddComponent<FluidBoundaryVoxels>();
        gameObject.AddComponent<FluidVoxels>();
        gameObject.AddComponent<DeathPlaneCulling>();
    }

    void OnDisable()
    {
        _pointsArgsBuffer?.Dispose();
        _blocksArgsBuffer?.Dispose();

        _voxelsBuffer?.Dispose();

        _meshTrianglesBuffer?.Dispose();
        _meshVerticesBuffer?.Dispose();
    }

    void Update()
    {
        // VoxelizeMeshWithGPU();
    
        if (_drawPointGrid)
        {
            _gridPointMaterial.SetVector(BoundsMin, new Vector4(_boundsMin.x, _boundsMin.y, _boundsMin.z, 0.0f));
            _gridPointMaterial.SetBuffer(Voxels, _voxelsBuffer);
            _pointsArgsBuffer.SetData(new[] {1, _gridPointCount, 0, 0, 0});
            Graphics.DrawProceduralIndirect(_gridPointMaterial, _meshCollider.bounds, MeshTopology.Points,
                _pointsArgsBuffer);
        }
    
        if (_drawBlocks)
        {
            _blocksArgsBuffer.SetData(new[] {_voxelMesh.triangles.Length, _gridPointCount, 0, 0, 0});
            _blocksMaterial.SetBuffer(Voxels, _voxelsBuffer);
            Graphics.DrawMeshInstancedIndirect(_voxelMesh, 0, _blocksMaterial, _meshCollider.bounds, _blocksArgsBuffer);
        }
    }

    void VoxelizeMeshWithGPU()
    {
        Profiler.BeginSample("Voxelize Mesh (GPU)");
        Bounds bounds = _meshCollider.bounds;
        _boundsMin = bounds.min;
        bool resizeVoxelPointsBuffer = false;
        
        if (_voxels == null 
         || _voxels.Length != GridSize.x * GridSize.y * GridSize.z 
         || _voxelsBuffer == null){
            _voxels = new Voxel[GridSize.x * GridSize.y * GridSize.z];
            resizeVoxelPointsBuffer = true;
        }

        if (resizeVoxelPointsBuffer 
         || _voxelsBuffer == null 
         || !_voxelsBuffer.IsValid()){
            _voxelsBuffer?.Dispose();
            _voxelsBuffer = new ComputeBuffer(GridSize.x * GridSize.y * GridSize.z, 4 * sizeof(float));
        }


        if (resizeVoxelPointsBuffer){
            _voxelsBuffer.SetData(_voxels);
            _voxelMesh = GenerateVoxelMesh(_halfSize * 2.0f);
        }

        if (_meshVerticesBuffer == null 
         || !_meshVerticesBuffer.IsValid()){
            _meshVerticesBuffer?.Dispose();

            var sharedMesh = _meshFilter.sharedMesh;
            _meshVerticesBuffer = new ComputeBuffer(sharedMesh.vertexCount, 3 * sizeof(float));
            _meshVerticesBuffer.SetData(sharedMesh.vertices);
        }

        if (_meshTrianglesBuffer == null 
         || !_meshTrianglesBuffer.IsValid()){
            _meshTrianglesBuffer?.Dispose();

            var sharedMesh = _meshFilter.sharedMesh;
            _meshTrianglesBuffer = new ComputeBuffer(sharedMesh.triangles.Length, sizeof(int));
            _meshTrianglesBuffer.SetData(sharedMesh.triangles);
        }

        var voxelizeKernel = _voxelizeComputeShader.FindKernel("VoxelizeMesh");
        _voxelizeComputeShader.SetInt("_GridWidth", GridSize.x);
        _voxelizeComputeShader.SetInt("_GridHeight", GridSize.y);
        _voxelizeComputeShader.SetInt("_GridDepth", GridSize.z);

        _voxelizeComputeShader.SetFloat("_CellHalfSize", _halfSize);

        _voxelizeComputeShader.SetMatrix("_WorldToLocalMatrix", transform.worldToLocalMatrix);
        _voxelizeComputeShader.SetBuffer(voxelizeKernel, Voxels, _voxelsBuffer);
        _voxelizeComputeShader.SetBuffer(voxelizeKernel, "_MeshVertices", _meshVerticesBuffer);
        _voxelizeComputeShader.SetBuffer(voxelizeKernel, "_MeshTriangleIndices", _meshTrianglesBuffer);
        _voxelizeComputeShader.SetInt("_TriangleCount", _meshFilter.sharedMesh.triangles.Length);

        _voxelizeComputeShader.SetVector(BoundsMin, _boundsMin);

        _voxelizeComputeShader.GetKernelThreadGroupSizes(voxelizeKernel, out uint xGroupSize, out uint yGroupSize,
            out uint zGroupSize);

        _voxelizeComputeShader.Dispatch(voxelizeKernel,
            Mathf.CeilToInt(GridSize.x / (float) xGroupSize),
            Mathf.CeilToInt(GridSize.y / (float) yGroupSize),
            Mathf.CeilToInt(GridSize.z / (float) zGroupSize));
        _gridPointCount = _voxelsBuffer.count;

        var volumeKernel = _voxelizeComputeShader.FindKernel("FillVolume");
        _voxelizeComputeShader.SetBuffer(volumeKernel, Voxels, _voxelsBuffer);
        _voxelizeComputeShader.SetBuffer(volumeKernel, "_MeshVertices", _meshVerticesBuffer);
        _voxelizeComputeShader.SetBuffer(volumeKernel, "_MeshTriangleIndices", _meshTrianglesBuffer);
        _voxelizeComputeShader.GetKernelThreadGroupSizes(voxelizeKernel, out xGroupSize, out yGroupSize,
            out zGroupSize);
        _voxelizeComputeShader.Dispatch(volumeKernel,
            Mathf.CeilToInt(GridSize.x / (float) xGroupSize),
            Mathf.CeilToInt(GridSize.y / (float) yGroupSize),
            Mathf.CeilToInt(GridSize.z / (float) zGroupSize));
        _voxelsBuffer.GetData(_voxels);

        Profiler.EndSample();
    }

    static Mesh GenerateVoxelMesh(float size)
    {
        var mesh = new Mesh();
        Vector3[] vertices =
        {
            //Front
            new Vector3(0, 0, 0), // Front Bottom Left    0
            new Vector3(size, 0, 0), // Front Bottom Right   1
            new Vector3(size, size, 0), // Front Top Right      2
            new Vector3(0, size, 0), // Front Top Left       3

            //Top
            new Vector3(size, size, 0), // Front Top Right      4
            new Vector3(0, size, 0), // Front Top Left          5
            new Vector3(0, size, size), // Back Top Left        6
            new Vector3(size, size, size), // Back Top Right    7

            //Right
            new Vector3(size, 0, 0), // Front Bottom Right      8
            new Vector3(size, size, 0), // Front Top Right      9
            new Vector3(size, size, size), // Back Top Right    10
            new Vector3(size, 0, size), // Back Bottom Right    11

            //Left
            new Vector3(0, 0, 0), // Front Bottom Left          12
            new Vector3(0, size, 0), // Front Top Left          13
            new Vector3(0, size, size), // Back Top Left        14
            new Vector3(0, 0, size), // Back Bottom Left        15

            //Back
            new Vector3(0, size, size), // Back Top Left        16
            new Vector3(size, size, size), // Back Top Right    17
            new Vector3(size, 0, size), // Back Bottom Right    18
            new Vector3(0, 0, size), // Back Bottom Left        19

            //Bottom
            new Vector3(0, 0, 0), // Front Bottom Left          20
            new Vector3(size, 0, 0), // Front Bottom Right      21
            new Vector3(size, 0, size), // Back Bottom Right    22
            new Vector3(0, 0, size) // Back Bottom Left         23
        };

        int[] triangles =
        {
            //Front
            0, 2, 1,
            0, 3, 2,

            // Top
            4, 5, 6,
            4, 6, 7,

            // Right
            8, 9, 10,
            8, 10, 11,

            // Left
            12, 15, 14,
            12, 14, 13,

            // Back
            17, 16, 19,
            17, 19, 18,

            // Bottom
            20, 22, 23,
            20, 21, 22
        };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(_meshCollider.bounds.center, _meshCollider.bounds.size);
    }

    void Reset()
    {
        _meshFilter = GetComponent<MeshFilter>();
        if (TryGetComponent(out MeshCollider meshCollider))
        {
            _meshCollider = meshCollider;
        }
        else
        {
            _meshCollider = gameObject.AddComponent<MeshCollider>();
        }

        var basePath = "Assets/GpuVoxelizer/";
        _gridPointMaterial = AssetDatabase.LoadAssetAtPath<Material>($"{basePath}Materials/GridPointMaterial.mat");
        _voxelizeComputeShader =
            AssetDatabase.LoadAssetAtPath<ComputeShader>($"{basePath}ComputeShaders/VoxelizeMesh.compute");
    }
#endif
}