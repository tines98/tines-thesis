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
        /// <param name="prefabScale">scale to apply to model</param>
        /// <param name="demoPosition">Position to place the demo object</param>
        /// <param name="simulationSize">size of the simulation</param>
        /// <param name="barSize">size of the bar</param>
        /// <param name="particleSize">particle radius to use in fluid demo</param>
        /// <param name="material">material to use on model</param>
        public static FluidDemo CreateDemo(FluidDemoRenderSettings renderSettings, 
                                      GameObject prefab, 
                                      Vector3 prefabScale, 
                                      Vector3 demoPosition, 
                                      Vector3 simulationSize, 
                                      Vector3 barSize,
                                      ParticleSize particleSize, 
                                      Material material){
            var demoGameObject = new GameObject("Created Demo"){
                transform = {position = demoPosition},
                tag = "Demo"
            };
            var demo = demoGameObject.AddComponent<FluidDemo>();
            demo.ParticleSize = particleSize;
            // Set simulation bounds
            demo.SimulationBounds = new Bounds(Vector3.zero, simulationSize);
            // Set bar size
            demo.BarChartBounds = new Bounds(new Vector3(0, (barSize.y-simulationSize.y)/2f, 0), barSize);
            // Set render settings
            demo.renderSettings = (FluidDemoRenderSettings) renderSettings.Clone();

            var realPrefab = GetMeshFilterContainingGameObject(prefab);
            var meshBounds = GetMeshBoundsFromPrefab(realPrefab);

            
            var rotation = RotateModel(realPrefab, prefabScale);
            var position = PlaceModel(realPrefab, prefabScale, 
                                      demo.SimulationBounds, 
                                      rotation);
            
            var actualScale = AbsVector(rotation * prefabScale);
            meshBounds = RotateScaleBounds(meshBounds, rotation, actualScale);
            var offset = Vector3.Scale(meshBounds.center, actualScale - Vector3.one);
            
            CreateVoxelizer(demoGameObject.transform, 
                            realPrefab, 
                            demoPosition+position-offset, 
                            rotation, 
                            prefabScale,
                            material);
            return demo;
        }


        /// <summary>
        /// Repositions the model in such a way that the top center part of the mesh
        /// is placed at the top center of the simulation bounds
        /// </summary>
        /// <param name="prefab">Model to reposition</param>
        /// <param name="scale">Extra Scale to apply to model</param>
        /// <param name="simulationBounds">The simulation bounds</param>
        /// <param name="rotation">Rotation that the model is rotated at</param>
        /// <returns>Vector that indicates the needed position change</returns>
        public static Vector3 PlaceModel(GameObject prefab, Vector3 scale, Bounds simulationBounds, Quaternion rotation){
            // Apply Rotation to Scale 
            var actualScale = AbsVector(rotation * scale);
            
            // Get Bounds of Model Mesh, then Rotate and Scale it
            var meshBounds = RotateScaleBounds(GetMeshBoundsFromPrefab(prefab), rotation, actualScale);
            // Debug.Log("meshBounds = " + meshBounds);
            // Top Center of Scaled and Rotated Mesh Bounds
            var meshMaxCenter = GetTopCenterOfBounds(meshBounds);
            // Top Center of Simulation Bounds
            var simulationTopCenter = GetTopCenterOfBounds(simulationBounds);
            return simulationTopCenter - meshMaxCenter;
        }

        public static Bounds RotateScaleBounds(Bounds bounds, Quaternion rotation, Vector3 scale){
            var rotatedBounds = RotateBounds(bounds, rotation);
            var scaledRotatedBounds = new Bounds(rotatedBounds.center, 
                                                 Vector3.Scale(rotatedBounds.size, scale));
            return scaledRotatedBounds;
        }
        
        
        /// <summary>
        /// Takes a bounds, and returns the top, centered
        /// </summary>
        /// <param name="bounds">Bounds to find top center of</param>
        /// <returns>Top Center of given bounds object</returns>
        private static Vector3 GetTopCenterOfBounds(Bounds bounds) => 
            new Vector3(bounds.center.x, 
                        bounds.max.y, 
                        bounds.center.z);

        private static Bounds RotateBounds(Bounds bounds, Quaternion rotation) => 
            new Bounds(rotation * bounds.center, 
                       AbsVector(rotation * bounds.size));

        public static Vector3 AbsVector(Vector3 vector) => 
            new Vector3(Mathf.Abs(vector.x),
                        Mathf.Abs(vector.y), 
                        Mathf.Abs(vector.z));

        
        /// <summary>
        /// Finds the bounds of the mesh attacked to the given prefab
        /// </summary>
        /// <param name="prefab">GameObject with a MeshFilter components attached</param>
        /// <returns>Bounds of the meshFilter's sharedMesh</returns>
        private static Bounds GetMeshBoundsFromPrefab(GameObject prefab){
            var meshFilter = prefab.GetComponent<MeshFilter>();
            if (meshFilter == null) meshFilter = prefab.GetComponentInChildren<MeshFilter>();
            return meshFilter.sharedMesh.bounds;
        }

        private static GameObject GetMeshFilterContainingGameObject(GameObject prefab){
            var meshFilter = prefab.GetComponent<MeshFilter>();
            if (meshFilter == null) meshFilter = prefab.GetComponentInChildren<MeshFilter>();
            return meshFilter.gameObject;
        }


        /// <summary>
        /// Rotates the model so its longest side is vertical
        /// </summary>
        /// <param name="prefab">Model to rotate</param>
        /// <param name="scale">scale of the model</param>
        public static Quaternion RotateModel(GameObject prefab, Vector3 scale){
            // Get mesh bounds
            var meshBounds = GetMeshBoundsFromPrefab(prefab);
            // Find longest mesh side
            var longestAxis = 0;
            var longestAxisLength = -1f;
            for (int axis = 0; axis < 3; axis++){
                if (meshBounds.size[axis] * scale[axis] < longestAxisLength) continue;
                longestAxis = axis;
                longestAxisLength = meshBounds.size[axis] * scale[axis];
            }
            var directionalVector = new Vector3{
                [longestAxis] = 1
            };
            // Calculate rotation needed
            return Quaternion.FromToRotation(directionalVector, Vector3.down);
        }


        /// <summary>
        /// Creates a MeshVoxelizer gameObject
        /// </summary>
        /// <remarks>prefab rotation and position is discarded</remarks>
        /// <param name="parent">parent transform to give created object</param>
        /// <param name="prefab">Prefab to copy mesh from</param>
        /// <param name="position">Position to place the model</param>
        /// <param name="rotation">Rotation to apply to the model</param>
        /// <param name="scale">Scale to apply to the model</param>
        /// <param name="material">Material to apply to the model</param>
        private static void CreateVoxelizer(Transform parent, GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, Material material){
            var voxelizer = new GameObject("voxelizer"){
                transform ={parent = parent}
            };
            var voxelizerGameObject = Object.Instantiate(prefab, 
                                                         position, 
                                                         rotation,
                                                         voxelizer.transform);
            voxelizerGameObject.transform.localScale = scale;
            voxelizerGameObject.GetComponent<MeshRenderer>().material = material;
            voxelizerGameObject.AddComponent<DeathPlaneCulling>();
            voxelizer.AddComponent<VoxelizerDemo>();
        }
    }
}
