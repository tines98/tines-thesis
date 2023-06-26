using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

public class VolumeCalcTest : MonoBehaviour{
    public bool useGPU = false;
    // Start is called before the first frame update
    void Start(){
        var mesh = GetComponent<MeshFilter>().sharedMesh;
        var localScale = transform.localScale;
        Profiler.BeginSample("Volume Calculation");
        float volume = useGPU 
                           ? MeshVolumeCalculator.GetVolumeCS(mesh, localScale) 
                           : MeshVolumeCalculator.VolumeOfMesh(mesh, localScale);
        // Debug.Log("volume on GPU = " + volume);
        Profiler.EndSample();
        Debug.Log("volume = " + volume);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
