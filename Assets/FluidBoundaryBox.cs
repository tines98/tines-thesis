using System;
using PBDFluid;
using UnityEngine;
using UnityEngine.Assertions;

public class FluidBoundaryBox : FluidBoundaryObject
{
    [SerializeField] private Vector3 size;
    private void Start()
    {
        FluidBodyMeshDemo = GetComponentInParent<FluidBodyMeshDemo>();
        Assert.IsNotNull(FluidBodyMeshDemo);
        ParticleSource = new ParticlesFromBounds(FluidBodyMeshDemo.Radius() * 2, OuterBounds(), InnerBounds());
        LoggingUtility.LogInfo($"FluidBoundaryBox {name} har a total of {ParticleSource.NumParticles} boundary particles!");
    }

    /// <returns>The bounds of the box</returns>
    private Bounds OuterBounds() => new Bounds(transform.position, size);
    
    /// <summary>
    /// Calculates the inner bounding box, so the walls of the box is 1 particle thick
    /// </summary>
    /// <returns>The calculated inner bounds</returns>
    private Bounds InnerBounds() => new Bounds(transform.position, size - (Vector3.one * FluidBodyMeshDemo.Radius() * 2f * 1.2f));

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        var outerBounds = OuterBounds();
        Gizmos.DrawWireCube(outerBounds.center,outerBounds.size);

        if (FluidBodyMeshDemo != null) {
            var innerBounds = InnerBounds();
            Gizmos.DrawWireCube(innerBounds.center,innerBounds.size);
        }
    }
}
