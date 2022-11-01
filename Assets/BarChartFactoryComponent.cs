using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BarChartFactoryComponent : MonoBehaviour
{
    public int bars;
    public Vector3 size;
    // Start is called before the first frame update
    void Start()
    {
        var localPos = Vector3.zero;
        BarChartFactory.CreateBarChart(bars, 
                                       transform, 
                                       localPos,
                                       size);
    }

    private void OnDrawGizmos(){
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position,size);
    }
}
