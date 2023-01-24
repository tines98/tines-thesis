using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraResizer : MonoBehaviour{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera perpectiveCamera;
    [SerializeField] private Camera orthoCamera;
    
    public void ResizeTo(float size){
        Debug.Log($"resizing camera to {size}");
        mainCamera.orthographicSize = size;
        orthoCamera.orthographicSize = size;
    }

    public void MoveSplitPoint(float barHeight, float simulationHeight){
        var splitCamera = GetComponent<SplitCamera>();
        var newSplitPoint = barHeight / simulationHeight;
        if (newSplitPoint < 0.5f) return;
        splitCamera.splitHeight = newSplitPoint;
    }
}
