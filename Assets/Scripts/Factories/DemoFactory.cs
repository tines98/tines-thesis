using System.Collections;
using System.Collections.Generic;
using MeshVoxelizer.Scripts;
using PBDFluid;
using UnityEngine;

public class DemoFactory{
    /// <summary>
    /// Factory method for <see cref="FluidBodyMeshDemo"/> 
    /// </summary>
    /// <param name="renderSettings">Render settings to give created demo</param>
    /// <param name="prefab">3D model to use for the demo</param>
    /// <param name="simulationSize">size of the simulation</param>
    /// <param name="barSize">size of the bar</param>
    public static void CreateDemo(DemoRenderSettings renderSettings, GameObject prefab, Vector3 simulationSize, Vector3 barSize){
        var demoGameObject = new GameObject("Created Demo");
        demoGameObject.tag = "Demo";
        var demo = demoGameObject.AddComponent<FluidBodyMeshDemo>();
        
        // Set simulation bounds
        demo.simulationBounds = new Bounds(Vector3.zero, simulationSize);
        
        // Set bar size
        demo.barChartBounds = new Bounds(new Vector3(0, (barSize.y-simulationSize.y)/2f, 0), barSize);
        
        // Set render settings
        demo.renderSettings = renderSettings;

        var pos = new Vector3(0, 
                              demo.simulationBounds.max.y-prefab.transform.localScale.y/2f,
                              0);
        
        CreateVoxelizer(demoGameObject.transform, prefab, pos);
    }


    /// <summary>
    /// Creates a MeshVoxelizer gameObject
    /// </summary>
    /// <remarks>prefab rotation and position is discarded</remarks>
    /// <param name="parent">parent transform to give created object</param>
    /// <param name="prefab">Prefab to copy mesh from</param>
    static void CreateVoxelizer(Transform parent, GameObject prefab, Vector3 position){
        var voxelizerGameObject = Object.Instantiate(prefab, 
                                                     position, 
                                                     Quaternion.identity, 
                                                     parent);
        // Insert Prefab as child of voxelizer
        voxelizerGameObject.AddComponent<VoxelizerDemo>();
    }
}
