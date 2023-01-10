using Demo;
using MeshVoxelizer.Scripts;
using UnityEngine;

namespace Factories{
    public static class FluidDemoFactory{
        /// <summary>
        /// Factory method for <see cref="FluidDemo"/> 
        /// </summary>
        /// <param name="renderSettings">Render settings to give created demo</param>
        /// <param name="prefab">3D model to use for the demo</param>
        /// <param name="demoPosition">Position to place the demo object</param>
        /// <param name="simulationSize">size of the simulation</param>
        /// <param name="barSize">size of the bar</param>
        public static void CreateDemo(FluidDemoRenderSettings renderSettings, GameObject prefab, Vector3 demoPosition, Vector3 simulationSize, Vector3 barSize){
            var demoGameObject = new GameObject("Created Demo"){
                transform ={
                    position = demoPosition
                },
                tag = "Demo"
            };
            var demo = demoGameObject.AddComponent<FluidDemo>();

            // Set simulation bounds
            demo.simulationBounds = new Bounds(Vector3.zero, simulationSize);
        
            // Set bar size
            demo.barChartBounds = new Bounds(new Vector3(0, (barSize.y-simulationSize.y)/2f, 0), barSize);
        
            // Set render settings
            demo.renderSettings = renderSettings;

            var pos = PlaceModel(prefab, demo.simulationBounds);
            
            CreateVoxelizer(demoGameObject.transform, prefab, pos);
        }

        public static Vector3 PlaceModel(GameObject prefab, Bounds simulationBounds){
            var meshFilter = prefab.GetComponent<MeshFilter>();
            if (meshFilter == null) meshFilter = prefab.GetComponentInChildren<MeshFilter>();
            var meshBounds = meshFilter.sharedMesh.bounds;
            
            // Top center part of model mesh bounds
            var meshMaxCenter = new Vector3(meshBounds.center.x,
                                            meshBounds.max.y,
                                            meshBounds.center.z);
            // meshMaxCenter scaled to transform scale
            var meshMaxCenterScaled = Vector3.Scale(meshMaxCenter, 
                                                    prefab.transform.localScale);

            
            var simulationTopCenter = new Vector3(simulationBounds.center.x,
                                                  simulationBounds.max.y,
                                                  simulationBounds.center.z);
            
            return simulationTopCenter - meshMaxCenterScaled;
        }


        /// <summary>
        /// Creates a MeshVoxelizer gameObject
        /// </summary>
        /// <remarks>prefab rotation and position is discarded</remarks>
        /// <param name="parent">parent transform to give created object</param>
        /// <param name="prefab">Prefab to copy mesh from</param>
        /// <param name="position">Position to place the model</param>
        private static void CreateVoxelizer(Transform parent, GameObject prefab, Vector3 position){
            var voxelizerGameObject = Object.Instantiate(prefab, 
                                                         Vector3.zero, 
                                                         Quaternion.identity, 
                                                         parent);
            voxelizerGameObject.transform.localPosition = position;
            // Insert Prefab as child of voxelizer
            voxelizerGameObject.AddComponent<VoxelizerDemo>();
        }
    }
}
