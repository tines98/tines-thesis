using System.Collections;
using System.Collections.Generic;
using PBDFluid;
using UnityEngine;
using UnityEngine.Assertions;

public class FluidBox : FluidObject
{
    [SerializeField] private Vector3 size;
    private void Start()
    {
        fluidBodyMeshDemo = GetComponentInParent<FluidBodyMeshDemo>();
        Assert.IsNotNull(fluidBodyMeshDemo);
        ParticleSource = new ParticlesFromBounds(fluidBodyMeshDemo.Radius() * 2, OuterBounds());
    }
    private Bounds OuterBounds() => new Bounds(transform.position, size);
    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        var outerBounds = OuterBounds();
        Gizmos.DrawWireCube(outerBounds.center,outerBounds.size);
    }
}
