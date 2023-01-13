using System.Collections.Generic;
using Factories;
using UnityEngine;
using UnityEngine.Serialization;

namespace Demo{
    public class FluidDemoManager : MonoBehaviour{
        [SerializeField] private List<ScaleModel> prefabs;
        [SerializeField] private FluidDemoRenderSettings renderSettings;
        [SerializeField] private Vector3 simulationSize;
        [SerializeField] private Vector3 barSize;
        [SerializeField] private SIMULATION_SIZE particleSize;

        private bool hasCreated;

        // Start is called before the first frame update
        void Start() => CreateDemos();


        /// <summary>
        /// Iterates through <see cref="prefabs"/> and calls <see cref="CreateDemo"/> for each of them
        /// </summary>
        void CreateDemos(){
            int index = 0;
            prefabs.ForEach(prefab => CreateDemo(index++));
            hasCreated = true;
        }


        /// <summary>
        /// Creates a <see cref="FluidDemo"/> using the <see cref="FluidDemoFactory"/>
        /// </summary>
        void CreateDemo(int index) => 
            FluidDemoFactory.CreateDemo(renderSettings, 
                                        prefabs[index].prefab,
                                        prefabs[index].scale,
                                        DemoPosition(index), 
                                        simulationSize, 
                                        barSize,
                                        particleSize);


        /// <summary>
        /// Calculates the position for a demo
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Vector3 DemoPosition(int index) => transform.position + Vector3.right 
                                                              * simulationSize.x 
                                                              * index;
    
        
        // GIZMOS
        private void OnDrawGizmos(){
            if (hasCreated) return;
            
            for (int i = 0; i < prefabs.Count; i++){
                var prefab = prefabs[i].prefab;
                var scale = prefabs[i].scale;
                var meshFilter = prefabs[i].prefab.GetComponent<MeshFilter>();
                if (meshFilter == null) meshFilter = prefab.GetComponentInChildren<MeshFilter>();
                var meshBounds = meshFilter.sharedMesh.bounds;
                var demoPos = DemoPosition(i);
                
                // Draw simulation Bounds
                Gizmos.color = Color.yellow;
                DrawSimulationBounds(new Bounds(demoPos, simulationSize));
        
                // Draw cylinder
                Gizmos.color = Color.red;
                DrawBarCylinderGizmo(new Bounds(demoPos 
                                              + Vector3.up * (barSize.y-simulationSize.y)/2f, 
                                                barSize));
                
                
                var rotation = FluidDemoFactory.RotateModel(prefab, scale);
                var position = demoPos + FluidDemoFactory.PlaceModel(prefab,
                                                                     scale,
                                                                     new Bounds(Vector3.zero, simulationSize),
                                                                     rotation);
                var actualScale = FluidDemoFactory.AbsVector(rotation * scale);
                var rotateScaleBounds = FluidDemoFactory.RotateScaleBounds(meshBounds, rotation, actualScale);
                
                // Draw model mesh
                Gizmos.color = Color.green;
                DrawModelGizmo(meshFilter.sharedMesh,
                               position,
                               rotation,
                               scale,
                               actualScale,
                               rotateScaleBounds);
            }
        }


        /// <summary>
        /// Draws a gizmo showing the barchart cylinder
        /// </summary>
        private void DrawBarCylinderGizmo(Bounds bounds) => 
            Gizmos.DrawWireMesh(renderSettings.cylinderMesh, 
                                bounds.center,
                                Quaternion.identity,
                                new Vector3(bounds.size.x, 
                                            bounds.size.y/2f, 
                                            bounds.size.z));
    
        /// <summary>
        /// Draws a gizmo showing the simulation bounds
        /// </summary>
        private void DrawSimulationBounds(Bounds bounds) => 
            Gizmos.DrawWireCube(bounds.center,bounds.size);
        

        private void DrawModelGizmo(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale, Vector3 scale2, Bounds bounds) =>
            Gizmos.DrawWireMesh(mesh, 
                                position - Vector3.Scale(bounds.center, 
                                                         scale2 - Vector3.one),
                                rotation,
                                scale);
    }
}
