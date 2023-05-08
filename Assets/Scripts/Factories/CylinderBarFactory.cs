using SimulationObjects.FluidBoundaryObject;
using UnityEngine;

namespace Factories{
    public class CylinderBarFactory
    {
        public static FluidBoundaryCylinderCup CreateBarChart(Transform parent, Vector3 localPos, Vector3 chartSize, Mesh mesh, Material material, float particleRadius){
            var barChartGameObject = new GameObject("Barchart"){
                transform ={
                    parent = parent,
                    localPosition = localPos
                }
            };
            
            AddMesh(barChartGameObject, mesh, material);
            var barChart = barChartGameObject.AddComponent<FluidBoundaryCylinderCup>();
            var radius = Mathf.Max(chartSize.x, chartSize.z) / 2f;
            barChart.height = chartSize.y;
            barChart.radius = radius + particleRadius * 2f;
            barChart.CreateParticles();
            barChartGameObject.transform.localScale = new Vector3(radius*2f, 
                                                                  barChart.height*0.5f, 
                                                                  radius*2f);
            return barChart;
        }

        private static void AddMesh(GameObject obj, Mesh mesh, Material material){
            var meshFilter = obj.AddComponent<MeshFilter>();
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            meshFilter.mesh = mesh;
            meshRenderer.material = material;
            // InvertNormals(meshFilter);
        }
    }
}
