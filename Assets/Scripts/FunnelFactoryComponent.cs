using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunnelFactoryComponent : MonoBehaviour{
    
    private Bounds barBounds;
    private Bounds meshBounds;

    public void CreateFunnel(){
        var localPos = Vector3.zero;
        var lowerRadius = CalcLowerRadius();
        var upperRadius = CalcUpperRadius();
        var height = CalcHeight(lowerRadius,upperRadius,45f);
        FunnelFactory.CreateFunnel(transform,
                                   localPos,
                                   lowerRadius,
                                   upperRadius,
                                   height);
    }

    private float CalcLowerRadius() => 
        Mathf.Min(barBounds.size.x, 
                  barBounds.size.z);
    
    private float CalcUpperRadius() => 
        Mathf.Max(meshBounds.size.x,
                  meshBounds.size.z);

    /// <summary>
    /// Calculates the funnel height based on the radius's and the desired funnel slope
    /// </summary>
    /// <param name="lr">lower radius</param>
    /// <param name="ur">upper radius</param>
    /// <param name="degrees">desired funnel slope</param>
    /// <returns>height of the funnel</returns>
    private float CalcHeight(float lr, float ur, float degrees) => 
        (ur - lr) / Mathf.Tan(degrees);
    
}
