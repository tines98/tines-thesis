using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class MeshVolumeCalculator : MonoBehaviour
{
    public static float VolumeOfMesh(Mesh mesh, Vector3 scale) {
        var vols = from t in GetTriangles(mesh, scale)
                   select SignedVolumeOfTriangle(t[0], t[1], t[2]);
        return Mathf.Abs(vols.Sum());
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


    private static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3) {
        var v321 = p3.x*p2.y*p1.z;
        var v231 = p2.x*p3.y*p1.z;
        var v312 = p3.x*p1.y*p2.z;
        var v132 = p1.x*p3.y*p2.z;
        var v213 = p2.x*p1.y*p3.z;
        var v123 = p1.x*p2.y*p3.z;
        return (1.0f/6.0f)*(-v321 + v231 + v312 - v132 - v213 + v123);
    }
}
