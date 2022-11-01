using System.Collections.Generic;
using UnityEngine;

public static class BarChartFactory{
    public static void CreateBarChart(int bars, Transform parent, Vector3 localPos, Vector3 chartSize){
        var barChartGameObject = new GameObject("Barchart"){
            transform ={
                parent = parent,
                localPosition = localPos
            }
        };
        var barChart = barChartGameObject.AddComponent<BarChart>();

        barChart.SetBars(CreateBars(bars, chartSize));
    }

    private static List<Bounds> CreateBars(int bars, Vector3 chartSize){
        var barBounds = new List<Bounds>(bars);
        //Calculate Size of each bar
        var barSize = new Vector3(chartSize.x / bars, chartSize.y, chartSize.z);
        var firstBarMinPos = new Vector3((barSize.x - chartSize.x) / 2f, 0, 0);
        //Calculate Position of each bar
        for (var i = 0; i < bars; i++){
            var pos = firstBarMinPos + new Vector3(barSize.x * i, 0, 0);
            barBounds.Add(new Bounds(pos, barSize));
        }

        return barBounds;
    }
}