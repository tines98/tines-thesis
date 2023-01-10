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
        FluidDemo = GetComponentInParent<FluidDemo>();
        Assert.IsNotNull(FluidDemo);
        ParticleSource = new ParticlesFromBounds(FluidDemo.Radius() * 2, OuterBounds());
    }
    
    /// <summary>
    /// Creates a Bounds object corresponding to the fluid box
    /// </summary>
    /// <returns>The Bounds object</returns>
    private Bounds OuterBounds() => new Bounds(transform.position, size);
    
    
    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        var outerBounds = OuterBounds();
        Gizmos.DrawWireCube(outerBounds.center,outerBounds.size);
    }
}
