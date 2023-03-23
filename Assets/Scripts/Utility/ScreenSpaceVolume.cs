using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ScreenSpaceVolume : MonoBehaviour{
    [SerializeField] private Material material;
    private Camera cameraComponent;

    private void Start(){
        cameraComponent = GetComponent<Camera>();
        cameraComponent.depthTextureMode = DepthTextureMode.DepthNormals;
        Debug.Log("Setting depth texture mode");
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest){
        material.SetTexture("_MainTex", src);
        Graphics.Blit(src, dest, material);
    }
}
