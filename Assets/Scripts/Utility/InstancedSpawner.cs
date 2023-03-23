using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancedSpawner : MonoBehaviour{
    public Mesh mesh;
    public Material material;
    public Bounds bounds;
    public Vector3[] positions;
    private Matrix4x4[] matrices;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer positionsBuffer;
    
    
    // Start is called before the first frame update
    void Start(){
        CreateArgBuffer(mesh.GetIndexCount(0));
        positionsBuffer = new ComputeBuffer(positions.Length, sizeof(float) * 4);
        positionsBuffer.SetData(positions);
    }
    
    private void CreateArgBuffer(uint indexCount)
    {
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = indexCount;
        args[1] = (uint)positions.Length;

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
    }

    private void OnDestroy(){
        argsBuffer.Release();
        positionsBuffer.Release();
    }

    // Update is called once per frame
    void Update(){
        material.SetBuffer("positions", positionsBuffer);
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
        
    }

    private void OnDrawGizmos(){
        var transformPosition = transform.position;
        Gizmos.DrawWireCube(transformPosition + bounds.center, bounds.size);
        foreach (var position in positions){
            Gizmos.DrawWireMesh(mesh, transformPosition + (Vector3) position );
        }
    }
}
