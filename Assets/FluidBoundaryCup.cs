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
        fluidBodyMeshDemo = GetComponentInParent<FluidBodyMeshDemo>();
        Assert.IsNotNull(fluidBodyMeshDemo);
        ParticleSource = new ParticlesFromBounds(fluidBodyMeshDemo.Radius() * 2, OuterBounds(), InnerBounds());
        Debug.Log($"particles for object {this.name} is {ParticleSource.NumParticles}");
    }
    private Bounds OuterBounds() => new Bounds(transform.position, size);

    private Bounds InnerBounds()
    {
        var innerBounds = new Bounds(transform.position, size - (Vector3.one * fluidBodyMeshDemo.Radius() * 2f * 1.2f));
        innerBounds.max = new Vector3(innerBounds.max.x,size.y/2f,innerBounds.max.z);
        return innerBounds;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        var outerBounds = OuterBounds();
        Gizmos.DrawWireCube(outerBounds.center,outerBounds.size);

        if (fluidBodyMeshDemo != null) {
            var innerBounds = InnerBounds();
            Gizmos.DrawWireCube(innerBounds.center,innerBounds.size);
        }
    }
}
