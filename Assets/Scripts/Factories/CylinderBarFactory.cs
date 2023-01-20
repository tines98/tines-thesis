using SimulationObjects.FluidBoundaryObject;
using UnityEngine;

namespace Factories{
    public class CylinderBarFactory
    {
        public static FluidBoundaryCylinderCup CreateBarChart(Transform parent, Vector3 localPos, Vector3 chartSize, Mesh mesh, Material material){
            var barChartGameObject = new GameObject("Barchart"){
                transform ={
                    parent = parent,
                    localPosition = localPos
                }
            };
            AddMesh(barChartGameObject, mesh, material);
            var barChart = barChartGameObject.AddComponent<FluidBoundaryCylinderCup>();
            barChart.height = chartSize.y;
            barChart.radius = Mathf.Max(chartSize.x, chartSize.z)/2f;
            barChart.CreateParticles();
            barChartGameObject.transform.localScale = new Vector3(barChart.radius*2f, 
                                                                  barChart.height*0.5f, 
                                                                  barChart.radius*2f);
            return barChart;
        }

        private static void AddMesh(GameObject obj, Mesh mesh, Material material){
            var meshFilter = obj.AddComponent<MeshFilter>();
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            meshFilter.mesh = mesh;
            meshRenderer.material = material;
            // InvertNormals(meshFilter);
        }
        
        static void InvertNormals(MeshFilter meshFilter)
        {
            var normals = meshFilter.mesh.normals;
            for(var i = 0; i < normals.Length; i++){
                normals[i] = -normals[i];
            }
            meshFilter.mesh.normals = normals;

            int[] triangles = meshFilter.mesh.triangles;
            for (var i = 0; i < triangles.Length; i+=3){
                (triangles[i], triangles[i + 2]) = (triangles[i + 2], triangles[i]);
            }
            meshFilter.mesh.triangles = triangles;
        }
    }
}
