using System;
using System.Collections.Generic;
using Factories;
using UnityEngine;
using Utility;

namespace Demo{
    public class FluidDemoManager : MonoBehaviour{
        [SerializeField] private List<ScaleModel> prefabs;
        [SerializeField] private FluidDemoRenderSettings renderSettings;
        [SerializeField] private Vector3 simulationSize;
        [SerializeField] private Vector3 barSize;
        [SerializeField] private ParticleSize particleSize;
        [SerializeField] private Material material;
        
        private List<FluidDemo> fluidDemos;
        public int currentDemoIndex;
        private bool hasCreated;
        private Camera mainCamera;
        private CameraResizer cameraResizer;
        [NonSerialized] public bool finishedAllDemos;
        
        public void NextDemo(){
            if (finishedAllDemos) return;
            currentDemoIndex++;
            if (currentDemoIndex == prefabs.Count) AllDemosComplete();
            else PlaceCameraAtDemo();
        }

        public int GetDemoCount() => fluidDemos.Count;
        
        private FluidDemo CurrentDemo() => fluidDemos[currentDemoIndex % GetDemoCount()];

        public void UpdateDeathPlane(float value) => CurrentDemo().deathPlaneHeight = value;

        public void StartDemo() => CurrentDemo().StartDemo();

        public void StopDemo() => CurrentDemo().StopDemo();

        
        // Start is called before the first frame update
        void Start(){
            CreateDemos();
            mainCamera = Camera.main;
            if (mainCamera != null) cameraResizer = mainCamera.GetComponent<CameraResizer>();
            PlaceCameraAtDemo();
        }


        void AllDemosComplete(){
            Debug.Log("All demos complete");
            finishedAllDemos = true;
            var firstDemo = DemoPosition(0);
            var lastDemo = DemoPosition(prefabs.Count - 1);
            var cameraSize = (lastDemo - firstDemo).magnitude;
            cameraResizer.ResizeTo(cameraSize/2f);
            cameraResizer.MoveSplitPoint(1f);
            var midPoint = firstDemo + (lastDemo - firstDemo) / 2f;
            var cameraPosition = new Vector3{
                x = midPoint.x,
                y = midPoint.y,
                z = mainCamera.transform.position.z
            };
            mainCamera.transform.position = cameraPosition;
        }


        /// <summary>
        /// Iterates through <see cref="prefabs"/> and calls <see cref="CreateDemo"/> for each of them
        /// </summary>
        void CreateDemos(){
            fluidDemos = new List<FluidDemo>(prefabs.Count);
            var index = 0;
            prefabs.ForEach(prefab => fluidDemos.Add(CreateDemo(index++)));
            hasCreated = true;
        }


        /// <summary>
        /// Creates a <see cref="FluidDemo"/> using the <see cref="FluidDemoFactory"/>
        /// </summary>
        private FluidDemo CreateDemo(int index) => 
            FluidDemoFactory.CreateDemo(renderSettings, 
                                        prefabs[index],
                                        DemoPosition(index), 
                                        simulationSize, 
                                        barSize,
                                        particleSize,
                                        material);


        /// <summary>
        /// Calculates the position for a demo
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Vector3 DemoPosition(int index) => transform.position 
                                                 + Vector3.right 
                                                 * simulationSize.x 
                                                 * index;

        private void PlaceCameraAtDemo(){
            var demoPos = DemoPosition(currentDemoIndex);
            cameraResizer.ResizeTo(simulationSize.y/2f);
            // Places the camera so perspective and ortho camera line up at simulation
            var cameraPosition = new Vector3{
                x = demoPos.x,
                y = demoPos.y,
                z = -simulationSize.z/2f - mainCamera.orthographicSize / Mathf.Tan(30f * Mathf.Deg2Rad)
            };
            mainCamera.transform.position = cameraPosition;
            cameraResizer.MoveSplitPoint(barSize.y,simulationSize.y);
        }

        
        // GIZMOS
        private void OnDrawGizmos(){
            if (hasCreated) return;
            
            for (int i = 0; i < prefabs.Count; i++){
                var prefab = prefabs[i].prefab;
                var scaleModel = prefabs[i];
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
                
                
                // var rotation = prefabs[i].shouldRotate 
                //                    ? FluidDemoFactory.RotateModel(prefab, scale) 
                //                    : Quaternion.identity;
                Quaternion rotation;
                if (scaleModel.shouldRotate){
                    rotation = FluidDemoFactory.RotateModel(prefab, scale);
                }
                else if (prefabs[i].forceRotate){
                    rotation = Quaternion.LookRotation(scaleModel.forward, scaleModel.up);
                }
                else{
                    rotation = Quaternion.identity;
                }
                var position = demoPos + FluidDemoFactory.PlaceModel(prefab,
                                                                     scale,
                                                                     new Bounds(Vector3.zero, simulationSize),
                                                                     rotation);
                var actualScale = FluidDemoFactory.AbsVector(rotation * scale);
                var rotateScaleBounds = FluidDemoFactory.RotateScaleBounds(meshBounds, rotation, actualScale);

                var pos = position - Vector3.Scale(rotateScaleBounds.center, actualScale - Vector3.one);
                
                // Draw model mesh
                Gizmos.color = Color.green;
                DrawModelGizmo(meshFilter.sharedMesh,
                               pos,
                               rotation,
                               scale);
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
        

        private void DrawModelGizmo(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale) =>
            Gizmos.DrawWireMesh(mesh, position, rotation, scale);
    }
}
