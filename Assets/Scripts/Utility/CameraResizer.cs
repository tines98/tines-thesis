using UnityEngine;

namespace Utility{
    public class CameraResizer : MonoBehaviour{
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera orthoCamera;
    
        public void ResizeTo(float size){
            Debug.Log($"resizing camera to {size}");
            mainCamera.orthographicSize = size;
            orthoCamera.orthographicSize = size;
        }

        public void MoveSplitPoint(float barHeight, float simulationHeight){
            var newSplitPoint = barHeight / simulationHeight;
            if (newSplitPoint < 0.5f) return;
            MoveSplitPoint(newSplitPoint);
        }
    
        public void MoveSplitPoint(float newSplitPoint){
            var splitCamera = GetComponent<SplitCamera>();
            splitCamera.splitHeight = newSplitPoint;
        }
    }
}
