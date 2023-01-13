using SimulationObjects.FluidBoundaryObject;
using UnityEngine;

namespace Factories{
    public class CylinderBarFactory
    {
        public static FluidBoundaryCylinderCup CreateBarChart(Transform parent, Vector3 localPos, Vector3 chartSize){
            var barChartGameObject = new GameObject("Barchart"){
                transform ={
                    parent = parent,
                    localPosition = localPos
                }
            };
            var barChart = barChartGameObject.AddComponent<FluidBoundaryCylinderCup>();
            barChart.height = chartSize.y;
            barChart.radius = Mathf.Max(chartSize.x, chartSize.z)/2f;
            barChart.CreateParticles();
            return barChart;
        }
    }
}
