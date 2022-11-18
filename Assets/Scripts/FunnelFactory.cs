using UnityEngine;

public class FunnelFactory 
{
    public static Funnel CreateFunnel(Transform parent, Vector3 localPos, float lowerRadius, float upperRadius, float height){
        var funnelGameObject = new GameObject("funnel"){
            transform ={
                parent = parent,
                localPosition = localPos
            }
        };
        var funnelComp = funnelGameObject.AddComponent<Funnel>();
        funnelComp.SetLowerRadius(lowerRadius);
        funnelComp.SetUpperRadius(upperRadius);
        funnelComp.SetHeight(height);
        funnelComp.CreateParticles();
        return funnelComp;
    }
    
    public static Funnel CreateFunnel(Transform parent, Bounds barBounds, Bounds meshBounds, float degrees){
        var lowerRadius = CalcLowerRadius(barBounds);
        var upperRadius = CalcUpperRadius(meshBounds);
        var height = CalcHeight(lowerRadius,
                                upperRadius,
                                degrees);
        return CreateFunnel(parent,             
                            CalcPosition(barBounds),
                            lowerRadius,
                            upperRadius,
                            height);
    }

    private static Vector3 CalcPosition(Bounds barBounds) => new Vector3(barBounds.center.x,
                                                                         barBounds.max.y,
                                                                         barBounds.center.z);

    private static float CalcLowerRadius(Bounds barBounds) => 
        Mathf.Min(barBounds.size.x/2f, 
                  barBounds.size.z/2f);
    
    private static float CalcUpperRadius(Bounds meshBounds) => 
        Mathf.Max(meshBounds.size.x,
                  meshBounds.size.z);

    /// <summary>
    /// Calculates the funnel height based on the radius's and the desired funnel slope
    /// </summary>
    /// <param name="lr">lower radius</param>
    /// <param name="ur">upper radius</param>
    /// <param name="degrees">desired funnel slope</param>
    /// <returns>height of the funnel</returns>
    private static float CalcHeight(float lr, float ur, float degrees) => 
        (ur - lr) / Mathf.Tan(degrees);
}
