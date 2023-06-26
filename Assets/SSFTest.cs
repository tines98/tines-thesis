using System;
using System.Collections;
using System.Collections.Generic;
using Shaders.ScreenSpaceVolume;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class SSFTest : MonoBehaviour{

    public float radius = 0.1f;
    public Bounds bounds;
    public int particleAmount;
    
    [NonSerialized]
    public ComputeBuffer PositionsBuffer;
    public ComputeBuffer SsPositionsBuffer;
    
    private Vector4[] positions;
    
    
    // Start is called before the first frame update
    void Start(){
        positions = new Vector4[particleAmount];
        SetPositions();
        PositionsBuffer = new ComputeBuffer(particleAmount, sizeof(float) * 4);
        SsPositionsBuffer = new ComputeBuffer(particleAmount, sizeof(float) * 4);
        PositionsBuffer.SetData(positions);
        SsPositionsBuffer.SetData(positions);
    }

    // Update is called once per frame
    void Update(){
        
    }

    private void SetPositions(){
        for (var i = 0; i < particleAmount; i++){
            positions[i] = RandomPositionInBounds();
        }
        // positions[0] = Vector4.zero;
    }

    private Vector4 RandomPositionInBounds(){
        var r = new Random();
        var position = new Vector4((float) r.NextDouble(), 
                                   (float) r.NextDouble(), 
                                   (float) r.NextDouble(), 
                                   1);
        
        Vector4 min = bounds.min;
        return transform.localToWorldMatrix.MultiplyPoint(min + Vector4.Scale(position, bounds.size));
    }
    

    private void OnDestroy(){
        PositionsBuffer.Dispose();
        SsPositionsBuffer.Dispose();
    }

    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        if (positions == null) return;
        foreach (var position in positions){
            Gizmos.DrawSphere(position, radius);
        }
    }
}
