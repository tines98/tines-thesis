using System.Collections;
using System.Collections.Generic;
using PBDFluid;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class DeathPlaneCulling : MonoBehaviour
{
    private FluidDemo demo;
    private MeshRenderer meshRenderer;
    private Material material;
    
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.material;
        demo = GetComponentInParent<FluidDemo>();
        Assert.IsNotNull(demo);
    }

    private void Foo() {
        material.SetVector("deathPlanePosition", demo.DeathPlane.transform.position);
        material.SetVector("deathPlaneSize",demo.DeathPlane.size);
    }
    
    // Update is called once per frame
    void Update() {
        if (demo.DeathPlane!=null) Foo();
    }
}
