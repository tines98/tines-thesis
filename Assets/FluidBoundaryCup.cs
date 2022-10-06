using System.Collections;
using System.Collections.Generic;
using PBDFluid;
using UnityEngine;
using UnityEngine.Assertions;

public class FluidBoundaryCup : FluidBoundaryObject
{
    [SerializeField] private Vector3 size;
    private void Start()
    {
        FluidBodyMeshDemo = GetComponentInParent<FluidBodyMeshDemo>();
        Assert.IsNotNull(FluidBodyMeshDemo);
        ParticleSource = new ParticlesFromBounds(FluidBodyMeshDemo.Radius() * 2, OuterBounds(), InnerBounds());
        Debug.Log($"particles for object {this.name} is {ParticleSource.NumParticles}");
    }
    
    
    /// <returns>The bounds of the cup</returns>
    private Bounds OuterBounds() => new Bounds(transform.position, size);
    
    
    /// <summary>
    /// Calculates the inner bounding box, so the walls of the cup is 1 particle thick
    /// Also 
    /// </summary>
    /// <returns>The calculated inner bounds</returns>
    private Bounds InnerBounds()
    {
        var innerBounds = new Bounds(transform.position, size - (Vector3.one * FluidBodyMeshDemo.Radius() * 2f * 1.2f));
        innerBounds.max = new Vector3(innerBounds.max.x,innerBounds.center.y+size.y/2f,innerBounds.max.z);
        return innerBounds;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        var outerBounds = OuterBounds();
        Gizmos.DrawWireCube(outerBounds.center,outerBounds.size);

        if (FluidBodyMeshDemo != null) {
            var innerBounds = InnerBounds();
            Gizmos.DrawWireCube(innerBounds.center,innerBounds.size);
        }
    }
}
