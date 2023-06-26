using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using Vector3 = UnityEngine.Vector3;

public class MeshVolumeCalculator : MonoBehaviour
{
    public static float VolumeOfMesh(Mesh mesh, Vector3 scale) {
        Profiler.BeginSample("Calculate Real Volume");
        var vols = from t in GetTriangles(mesh, scale)
                   select SignedVolumeOfTriangle(t[0], t[1], t[2]);
        Profiler.EndSample();
        var volume = Mathf.Abs(vols.Sum());
        return volume;
    }
    public static float[] VolumeOfMeshArray(Mesh mesh, Vector3 scale) {
        Profiler.BeginSample("Calculate Real Volume");
        var vols = from t in GetTriangles(mesh, scale)
                   select SignedVolumeOfTriangle(t[0], t[1], t[2]);
        Profiler.EndSample();
        return vols.ToArray();
    }

    private static List<Vector3[]> GetTriangles(Mesh mesh, Vector3 scale){
        var triangles = new List<Vector3[]>();
        for (int i = 0; i < mesh.triangles.Length; i+=3){
            triangles.Add(new[]{
                Vector3.Scale(mesh.vertices[mesh.triangles[i]], scale),
                Vector3.Scale(mesh.vertices[mesh.triangles[i + 1]], scale),
                Vector3.Scale(mesh.vertices[mesh.triangles[i + 2]], scale)
            });
        }
        return triangles;
    }

    public static float GetVolumeCS(Mesh mesh, Vector3 scale){
        var shader = Resources.Load("MeshVolumeCalculator") as ComputeShader;
        // if (shader == null) return -1;
        //Volume Buffer
        var triCount = mesh.triangles.Length / 3;
        var volumeBuffer = new ComputeBuffer(triCount, sizeof(float));
        var volumes = new float[triCount];
        // volumeBuffer.SetData(volumes);
        // Vertices Buffer
        var verticesBuffer = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 3);
        verticesBuffer.SetData(mesh.vertices);
        // Triangle Buffer
        var trianglesBuffer = new ComputeBuffer(mesh.triangles.Length, sizeof(int));
        trianglesBuffer.SetData(mesh.triangles);
        
        int kernel = shader.FindKernel("CSMain");
        
        shader.SetBuffer(kernel, "volumes", volumeBuffer);
        shader.SetBuffer(kernel, "vertices", verticesBuffer);
        shader.SetBuffer(kernel, "triangles", trianglesBuffer);
        shader.SetVector("scale", scale);
        
        //Run
        var threads = 128;
        var groups = triCount / threads;
        if (triCount % threads != 0) groups++;
        
        shader.Dispatch(kernel,groups,1,1);
        
        // Get result
        volumeBuffer.GetData(volumes);
        
        //Dispose 
        trianglesBuffer.Dispose();
        verticesBuffer.Dispose();
        volumeBuffer.Dispose();
        
        return volumes.Sum();
    }


    public static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3) {
        var v321 = p3.x*p2.y*p1.z;
        var v231 = p2.x*p3.y*p1.z;
        var v312 = p3.x*p1.y*p2.z;
        var v132 = p1.x*p3.y*p2.z;
        var v213 = p2.x*p1.y*p3.z;
        var v123 = p1.x*p2.y*p3.z;
        return (1.0f/6.0f)*(-v321 + v231 + v312 - v132 - v213 + v123);
    }
}
