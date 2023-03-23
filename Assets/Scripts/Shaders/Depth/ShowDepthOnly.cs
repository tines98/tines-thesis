using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDepthOnly : MonoBehaviour{
    [SerializeField] private Material material;
    private Camera cameraComponent;
    // Start is called before the first frame update
    void Start(){
        cameraComponent = GetComponent<Camera>();
        cameraComponent.depthTextureMode = DepthTextureMode.DepthNormals;
        Debug.Log("Setting depth texture mode");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest){
        material.SetTexture("_MainTex", src);
        Graphics.Blit(src, dest, material);
    }
}
