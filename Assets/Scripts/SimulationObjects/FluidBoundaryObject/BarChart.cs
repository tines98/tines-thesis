using System.Collections.Generic;
using PBDFluid;
using UnityEngine;
using UnityEngine.Assertions;

public class BarChart : FluidBoundaryObject{
    public float height;
    public  List<Bounds> barBoundsList;
    private float spacing;
    private List<Vector3> posList;
    
    // Start is called before the first frame update
    public void CreateParticles(){
        FluidDemo = GetComponentInParent<FluidDemo>();
        Assert.IsNotNull(FluidDemo);
        if (height <= 0f) height = barBoundsList[0].size.y;
        spacing = FluidDemo.Radius() * 2;
        posList = new List<Vector3>();
        CreateBars();
        ParticleSource = new ParticlesFromList(spacing, posList);
    }

    /// <summary>Creates the boundary particles for the entire barchart</summary>
    private void CreateBars() => barBoundsList.ForEach(CreateBar);

    /// <summary>
    /// Creates boundary particles for a bar
    /// </summary>
    /// <param name="barBound">bounds of the bar</param>
    private void CreateBar(Bounds barBound){
        CreateFloor(barBound,
                    barBound.min.y);
        CreateWalls(barBound,
                    barBound.min.y,
                    barBound.max.y);
    }
    
    
    /// <summary>
    /// Creates four walls of particles for a bar
    /// </summary>
    /// <param name="bounds"></param>
    /// <param name="yMin"></param>
    /// <param name="yMax"></param>
    private void CreateWalls(Bounds bounds, float yMin, float yMax){
        for (var y = yMin; y < yMax; y+=spacing)
            CreateRectanglePerimeter(bounds,y);
    }

    
    /// <summary>
    /// Creates a rectangle of particles
    /// </summary>
    /// <param name="bounds"></param>
    /// <param name="y"></param>
    private void CreateRectanglePerimeter(Bounds bounds, float y){
        for (var z = bounds.min.z; z < bounds.max.z; z+=spacing){
            var posMin = new Vector3(bounds.min.x, y, z);
            var posMax = new Vector3(bounds.max.x, y, z);
            posList.Add(posMin);
            posList.Add(posMax);
        }
        for (var x = bounds.min.x; x < bounds.max.x; x+=spacing){
            var posMin = new Vector3(x, y, bounds.min.z);
            var posMax = new Vector3(x, y, bounds.max.z);
            posList.Add(posMin);
            posList.Add(posMax);
        }
    }

    
    /// <summary>
    /// Creates the floor of particles for a bar
    /// </summary>
    /// <param name="bounds">bounds of the bar</param>
    /// <param name="y">y coordinate for the floor</param>
    private void CreateFloor(Bounds bounds, float y){
        for (var z = bounds.min.z; z < bounds.max.z; z+=spacing){
            for (var x = bounds.min.x; x < bounds.max.x; x+=spacing){
                var pos = new Vector3(x, y, z);
                posList.Add(pos);
            }
        }
    }
    
    
    /// <summary>
    /// Sets the list of bounds representing the bars
    /// </summary>
    /// <param name="bars">List of bar Bounds</param>
    public void SetBars(List<Bounds> bars) => barBoundsList = bars;
    
    
    private void OnDrawGizmos(){
        if (barBoundsList == null) return;
        Gizmos.color = Color.red;
        barBoundsList.ForEach(DrawBarGizmo);
    }

    //GIZMO
    private void DrawBarGizmo(Bounds bar) => 
        Gizmos.DrawWireCube(bar.center, bar.size);
    }