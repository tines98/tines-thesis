using System;
using System.Collections;
using System.Collections.Generic;
using MeshVoxelizer.Scripts;
using PBDFluid;
using UnityEngine;

public class DemoManager : MonoBehaviour{
    [SerializeField] private GameObject prefab;
    [SerializeField] private DemoRenderSettings renderSettings;
    [SerializeField] private Vector3 simulationSize;
    [SerializeField] private Vector3 barSize;
    

    // Start is called before the first frame update
    void Start() => CreateDemo();

    
    /// <summary>
    /// Creates a <see cref="FluidBodyMeshDemo"/> using the <see cref="DemoFactory"/>
    /// </summary>
    void CreateDemo() => DemoFactory.CreateDemo(renderSettings, prefab, simulationSize, barSize);

    
    // GIZMOS
    private void OnDrawGizmos(){
        Gizmos.color = Color.yellow;
        DrawSimulationBounds(new Bounds(Vector3.zero, simulationSize));
        
        Gizmos.color = Color.red;
        DrawBarCylinderGizmo(new Bounds(new Vector3(0, (barSize.y-simulationSize.y)/2f, 0), barSize));
    }

    /// <summary>
    /// Draws a gizmo showing the barchart cylinder
    /// </summary>
    private void DrawBarCylinderGizmo(Bounds bounds) => 
        Gizmos.DrawWireMesh(renderSettings.cylinderMesh, 
                            transform.position + bounds.center,
                            Quaternion.identity,
                            new Vector3(bounds.size.x, 
                                        bounds.size.y/2f, 
                                        bounds.size.z));
    
    /// <summary>
    /// Draws a gizmo showing the simulation bounds
    /// </summary>
    private void DrawSimulationBounds(Bounds bounds) => 
        Gizmos.DrawWireCube(transform.position+bounds.center,bounds.size);
}
