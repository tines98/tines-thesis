using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropCamera : MonoBehaviour{
    [SerializeField] private Material material;
    [SerializeField] private Vector2 crop;
    [SerializeField] private Vector2 offset;
    
    // private Camera camera;
    private static readonly int Crop = Shader.PropertyToID("crop");
    private static readonly int Offset = Shader.PropertyToID("offset1");

    private void Start(){
        // camera = GetComponent<Camera>();
    }

    private void Update(){
        
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest){
        material.SetVector(Crop, crop);
        material.SetVector(Offset, offset);
        Graphics.Blit(src,dest,material);
    }
}
