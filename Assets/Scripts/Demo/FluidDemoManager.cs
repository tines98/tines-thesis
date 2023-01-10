using System.Collections.Generic;
using Factories;
using UnityEngine;

namespace Demo{
    public class FluidDemoManager : MonoBehaviour{
        [SerializeField] private List<GameObject> prefabs;
        [SerializeField] private FluidDemoRenderSettings renderSettings;
        [SerializeField] private Vector3 simulationSize;
        [SerializeField] private Vector3 barSize;

        private bool hasCreated;
    

        // Start is called before the first frame update
        void Start() => CreateDemos();


        /// <summary>
        /// Iterates through <see cref="prefabs"/> and calls <see cref="CreateDemo"/> for each of them
        /// </summary>
        void CreateDemos(){
            int index = 0;
            prefabs.ForEach(prefab => CreateDemo(prefab, index++));
            hasCreated = true;
        }


        /// <summary>
        /// Creates a <see cref="FluidDemo"/> using the <see cref="FluidDemoFactory"/>
        /// </summary>
        void CreateDemo(GameObject prefab, int index) => 
            FluidDemoFactory.CreateDemo(renderSettings, 
                                        prefab, 
                                        DemoPosition(index), 
                                        simulationSize, 
                                        barSize);


        /// <summary>
        /// Calculates the position for a demo
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Vector3 DemoPosition(int index) => Vector3.right 
                                         * simulationSize.x 
                                         * index;
    
        
        // GIZMOS
        private void OnDrawGizmos(){
            if (hasCreated) return;
            
            for (int i = 0; i < prefabs.Count; i++){
                var demoPos = DemoPosition(i);
                Gizmos.color = Color.yellow;
                DrawSimulationBounds(new Bounds(demoPos, simulationSize));
        
                Gizmos.color = Color.red;
                DrawBarCylinderGizmo(new Bounds(demoPos 
                                              + Vector3.up * (barSize.y-simulationSize.y)/2f, 
                                                barSize));
                
                Gizmos.color = Color.green;
                var meshFilter = prefabs[i].GetComponent<MeshFilter>();
                if (meshFilter == null) meshFilter = prefabs[i].GetComponentInChildren<MeshFilter>();
                
                DrawModelGizmo(meshFilter.sharedMesh, 
                               demoPos 
                             + FluidDemoFactory.PlaceModel(prefabs[i],
                                                           new Bounds(Vector3.zero,
                                                                      simulationSize)));
            }
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

        private void DrawModelGizmo(Mesh mesh, Vector3 position) =>
            Gizmos.DrawWireMesh(mesh, transform.position + position);
    }
}
