using System.Collections;
using System.Collections.Generic;
using PBDFluid;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class DeathPlaneCulling : MonoBehaviour
{
    private FluidBodyMeshDemo demo;
    private MeshRenderer meshRenderer;
    private Material material;
    
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.material;
        demo = GetComponentInParent<FluidBodyMeshDemo>();
        Assert.IsNotNull(demo);
    }

    private void Foo() {
        material.SetVector("deathPlanePosition", demo.deathPlane.transform.position);
        material.SetVector("deathPlaneSize",demo.deathPlane.size);
    }
    
    // Update is called once per frame
    void Update() {
        if (demo.deathPlane!=null) Foo();
    }
}
