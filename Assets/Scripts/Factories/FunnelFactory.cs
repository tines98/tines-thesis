using SimulationObjects.FluidBoundaryObject;
using UnityEngine;

namespace Factories{
    public static class FunnelFactory 
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
            Debug.Log("upperRadius = " + upperRadius);
            Debug.Log("lowerRadius = " + lowerRadius);
        
            var height = CalcHeight(lowerRadius,
                                    upperRadius,
                                    degrees);
            var maxHeight = MaxHeight(barBounds, meshBounds);
            if (height > maxHeight){
                height = maxHeight;
            }
            return CreateFunnel(parent,             
                                CalcPosition(barBounds,parent.position),
                                lowerRadius,
                                upperRadius,
                                height);
        }

        private static Vector3 CalcPosition(Bounds barBounds, Vector3 parentPos) => 
            new Vector3(barBounds.center.x,
                        barBounds.max.y, 
                        barBounds.center.z) - parentPos;

        private static float CalcLowerRadius(Bounds barBounds) => 
            Mathf.Min(barBounds.extents.x, 
                      barBounds.extents.z);

        private static float MaxHeight(Bounds barBounds, Bounds meshBounds) => 
            meshBounds.min.y - barBounds.max.y - 0.06f; 

        private static Vector3 FlatSizeVector(Bounds bounds) => new Vector3(bounds.extents.x,0,bounds.extents.z);

        private static float CalcUpperRadius(Bounds meshBounds) => FlatSizeVector(meshBounds).magnitude; 
            // Mathf.Max(0.06f +meshBounds.size.x/2f,
            //           0.06f +meshBounds.size.z/2f);

        /// <summary>
        /// Calculates the funnel height based on the radius's and the desired funnel slope
        /// </summary>
        /// <param name="lr">lower radius</param>
        /// <param name="ur">upper radius</param>
        /// <param name="degrees">desired funnel slope</param>
        /// <returns>height of the funnel</returns>
        private static float CalcHeight(float lr, float ur, float degrees) => 
            (ur - lr) / Mathf.Tan(degrees); 
        
        private static float CalcDegrees(float lr, float ur, float height) =>
            Mathf.Atan((ur - lr) / height); 
    }
}
