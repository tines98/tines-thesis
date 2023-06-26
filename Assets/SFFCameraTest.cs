using System;
using System.Collections;
using System.Collections.Generic;
using Shaders.ScreenSpaceVolume;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Camera))]
public class SFFCameraTest : MonoBehaviour{
    public SSFTest ssfTest;
    public float blurDistance = 0.1f;
    public Material blurMaterial;
    public Material diskMaterial;
    public Material mergeMaterial;
    public Material testMaterial;
    public RenderTexture renderTexture;
    

    private SSF ssf;

    private static readonly int SecondaryTex = Shader.PropertyToID("_SecondaryTex");

    // Start is called before the first frame update
    void Start(){
        ssf = new SSF();
    }

    private void CreateRenderTexture(int w, int h){
        Debug.Log("w = " + w);
        Debug.Log("h = " + h);
        renderTexture = new RenderTexture(w,h,1);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination){
        if (ssfTest == null) return;
        if (ssfTest.PositionsBuffer == null) return;
        if (ssf == null) return;
        if (blurMaterial == null) return;
        if (diskMaterial == null) return;
        if (mergeMaterial == null) return;
        if (testMaterial == null) return;
        if (renderTexture == null) renderTexture = new RenderTexture(source);//CreateRenderTexture(source.width,source.height);
        
        // ssf.CreatePointSprite(ssfTest.PositionsBuffer, 
        //                       ssfTest.SsPositionsBuffer, 
        //                       ssfTest.particleAmount,
        //                       ssfTest.radius,
        //                       Camera.current, 
        //                       renderTexture);
        //
        diskMaterial.SetBuffer("positions",ssfTest.PositionsBuffer);
        diskMaterial.SetFloat("near_plane", Camera.current.nearClipPlane);
        diskMaterial.SetFloat("far_plane", Camera.current.farClipPlane);
        diskMaterial.SetInt("particle_count",ssfTest.particleAmount);
        
        // Graphics.CopyTexture(source,renderTexture);
        
        // testMaterial.SetBuffer("particles", ssfTest.PositionsBuffer);
        Graphics.Blit(source,destination,diskMaterial);
        
        // Graphics.Blit(source,destination,blurMaterial);
        
        // Graphics.Blit(source,destination,material);
    }
}
