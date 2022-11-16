using UnityEngine;

public class FunnelFactory 
{
    public static void CreateFunnel(Transform parent, 
                             Vector3 localPos, 
                             float lowerRadius, 
                             float upperRadius, 
                             float height){

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
    }
}
