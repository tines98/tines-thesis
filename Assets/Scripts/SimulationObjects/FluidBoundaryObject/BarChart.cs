using System.Collections.Generic;
using PBDFluid;
using PBDFluid.Scripts;
using UnityEngine;
using UnityEngine.Assertions;

public class BarChart : FluidBoundaryObject{
    public float height;
    public  List<Bounds> barBoundsList;


    // Start is called before the first frame update
    private void Start(){
        FluidBodyMeshDemo = GetComponentInParent<FluidBodyMeshDemo>();
        Assert.IsNotNull(FluidBodyMeshDemo);
        if (height <= 0f) height = barBoundsList[0].size.y;
        CreateParticleSource(FluidBodyMeshDemo.Radius()*2);
    }

    private void CreateParticleSource(float diameter){
        ParticleSource = new ParticlesFromSeveralBounds(diameter, CreateParticleSources(diameter).ToArray());
        ParticleSource.CreateParticles();
        LoggingUtility.LogInfo(
            $"BarChart {name} har a total of {ParticleSource.NumParticles} boundary particles!"
        );
    }

    private List<ParticleSource> CreateParticleSources(float diameter){
        var particleSources = new List<ParticleSource>(barBoundsList.Count);
        barBoundsList.ForEach(barBound => 
            particleSources.Add(new ParticlesFromBounds(diameter,
                                                        OuterBounds(barBound), 
                                                        InnerBounds(barBound, diameter))));
        return particleSources;
    }
    
    
    public void SetBars(List<Bounds> bars) => barBoundsList = bars;
    

    /// <summary>
    ///     Calculates the inner bounding box of a bar, so the walls of the cup is 1 particle thick
    /// </summary>
    /// <returns>The calculated inner bounds of a bar</returns>
    private Bounds InnerBounds(Bounds bounds, float diameter){
        var innerBoundsSize = bounds.size 
                            - Vector3.one * diameter * 1.5f;
        var innerBounds = new Bounds(transform.position + bounds.center,
                                     innerBoundsSize);
        //Move the top of the exclusion box up, so it turns into a cup
        var innerBoundsHeight = innerBounds.center.y 
                              + bounds.size.y / 2f;
        innerBounds.max = new Vector3(innerBounds.max.x,
                                      innerBoundsHeight,
                                      innerBounds.max.z);
        return innerBounds;
    }

    private Bounds OuterBounds(Bounds bounds) => new Bounds(transform.position + bounds.center, 
                                                            bounds.size);
    
    private void OnDrawGizmos(){
        if (barBoundsList == null) return;
        Gizmos.color = Color.red;
        barBoundsList.ForEach(DrawBarGizmo);
    }

    //GIZMO
    private void DrawBarGizmo(Bounds bar){
        if (FluidBodyMeshDemo == null) return;
        var outerBounds = OuterBounds(bar);
        Gizmos.DrawWireCube(outerBounds.center, bar.size);
        var diameter = FluidBodyMeshDemo.Radius() * 2;
        var innerBounds = InnerBounds(bar,diameter);
        Gizmos.DrawWireCube(innerBounds.center, innerBounds.size);
    }
}