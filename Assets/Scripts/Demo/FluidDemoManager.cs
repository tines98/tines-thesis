using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Factories;
using TMPro;
using UnityEngine;
using Utility;

namespace Demo{
    public class FluidDemoManager : MonoBehaviour{
        [SerializeField] private List<ScaleModel> scaleModels;
        [SerializeField] private FluidDemoRenderSettings renderSettings;
        [SerializeField] private Vector3 simulationSize;
        [SerializeField] private Vector3 barSize;
        [SerializeField] private int barNotches = 5;
        [SerializeField] private ParticleSize particleSize;
        [SerializeField] private Material material;
        
        [NonSerialized] public int CurrentDemoIndex;
        private CameraResizer cameraResizer;
        [NonSerialized] public bool FinishedAllDemos;

        private List<FluidDemo> fluidDemos;
        private bool hasCreated;
        private Camera mainCamera;
        private float largestVolume;
        [SerializeField] private bool useRealVolume;

        public int DemoCount => 
            fluidDemos.Count;

        /// <summary>
        /// Returns the current demo
        /// </summary>
        private FluidDemo CurrentDemo => 
            fluidDemos[CurrentDemoIndex % DemoCount];

        /// <summary>
        /// Returns the first demo's position
        /// </summary>
        private Vector3 FirstDemoPosition => DemoPosition(0);

        /// <summary>
        /// Returns the last demo's position
        /// </summary>
        private Vector3 LastDemoPosition => DemoPosition(scaleModels.Count - 1);
        
        /// <summary>
        /// Returns the mid point of the box formed by all demos combined
        /// </summary>
        private Vector3 MidPoint => FirstDemoPosition + (LastDemoPosition - FirstDemoPosition)/2f;

        /// <summary>
        /// Calculates the z-position the camera needs to line up the two projections
        /// </summary>
        private float CameraDepth =>
            - simulationSize.z / 2f
            - mainCamera.orthographicSize / Mathf.Tan(cameraResizer.GetFOV() / 2f * Mathf.Deg2Rad);
        
        /// Start is called before the first frame update
        void Start(){
            mainCamera = Camera.main;
            if (useRealVolume){
                var maxVolume = GetLargestVolume();
                var barDiameter = CalculateBarSize(maxVolume);
                barSize = new Vector3(barDiameter, 
                                      simulationSize.y / 2f, 
                                      barDiameter);
            }
            renderSettings.cylinderMaterial
                          .mainTextureScale = new Vector2(1,barNotches);
            CreateBackPlane();
            CreateDemos();
            if (mainCamera != null) 
                cameraResizer = mainCamera.GetComponent<CameraResizer>();
            PlaceCameraAtDemo();
        }

        /// <summary>
        /// Creates the plane at the back of the simulations
        /// </summary>
        void CreateBackPlane(){
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.GetComponent<MeshRenderer>().sharedMaterial = renderSettings.cylinderMaterial;
            // Rotate Plane
            plane.transform.rotation = Quaternion.Euler(90, 0, 0);
            // Scale Plane
            var xScale = simulationSize.x * scaleModels.Count / 10f;
            var zScale = simulationSize.y / 20f;
            plane.transform.localScale = new Vector3(xScale, 1, zScale);
            // Translate Plane
            plane.transform.position = new Vector3(MidPoint.x, 
                                                   -5 * zScale, 
                                                   simulationSize.z / 2f);
        }

        float CalculateBarSize(float volume) => Mathf.Sqrt(volume / 
                                                           (Mathf.PI * simulationSize.y / 2f)) * 2f;

        Mesh GetMesh(ScaleModel scaleModel) => 
            scaleModel.prefab.GetComponent<MeshFilter>() 
                ? scaleModel.prefab.GetComponent<MeshFilter>().sharedMesh 
                : scaleModel.prefab.GetComponentInChildren<MeshFilter>().sharedMesh;

        float GetLargestVolume(){
            scaleModels.ForEach(scaleModel => {
                scaleModel.realVolume = MeshVolumeCalculator.GetVolumeCS(GetMesh(scaleModel),
                                                                          scaleModel.scale);
            });
            return scaleModels.Max(e => e.realVolume);   
        }


        /// <summary>
        /// Iterates through <see cref="scaleModels"/> and calls <see cref="CreateDemo"/> for each of them
        /// </summary>
        void CreateDemos(){
            fluidDemos = new List<FluidDemo>(scaleModels.Count);
            var index = 0;
            scaleModels.ForEach(_ => fluidDemos.Add(CreateDemo(index++)));
            hasCreated = true;
            if (useRealVolume)
                scaleModels.ForEach(e => Debug.Log($"realVolume of {e.prefab.name} is {e.realVolume}"));
        }
        
        public void UpdateDeathPlane(float value) => 
            CurrentDemo.DeathPlaneHeight = value;

        public void NextDemo(){
            if (FinishedAllDemos) return;
            UpdateDeathPlane(0f);
            CurrentDemoIndex++;
            if (CurrentDemoIndex == scaleModels.Count) AllDemosComplete();
            else PlaceCameraAtDemo();
        }
        
        public void StartDemo() => 
            CurrentDemo.StartDemo();

        public void StopDemo() => 
            CurrentDemo.StopDemo();

        void AllDemosComplete(){
            Debug.Log("All demos complete");
            FinishedAllDemos = true;
            cameraResizer.ResizeTo(simulationSize.y/2f);
            if (useRealVolume)
                fluidDemos.ForEach(a => a.CreateText());
            CreateText();
            MoveCamera(MidPoint, CameraDepth);
        }

        private void CreateText(){
            var pos = transform.position + new Vector3(-simulationSize.x, -simulationSize.y/2f + barSize.y, 0);
            var text = Instantiate(renderSettings.floatingTextPrefab, pos, Quaternion.identity, transform);
            var textMeshPro = text.GetComponent<TextMeshPro>();
            var radius = barSize.x / 2f;
            var realVolume = Mathf.PI * radius * radius * barSize.y;
            textMeshPro.text = Math.Round(realVolume, 2).ToString(CultureInfo.InvariantCulture);
        }


        


        /// <summary>
        /// Creates a <see cref="FluidDemo"/> using the <see cref="FluidDemoFactory"/>
        /// </summary>
        private FluidDemo CreateDemo(int index) => 
            FluidDemoFactory.CreateDemo(renderSettings, 
                                        scaleModels[index],
                                        DemoPosition(index), 
                                        simulationSize, 
                                        barSize,
                                        particleSize,
                                        material,
                                        scaleModels[index].Texture);


        /// <summary>
        /// Calculates the position for a demo
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Vector3 DemoPosition(int index) => transform.position 
                                                 + Vector3.right 
                                                 * simulationSize.x 
                                                 * index;

        /// <summary>
        /// Places the camera in front of the current demo
        /// </summary>
        private void PlaceCameraAtDemo(){
            var demoPos = DemoPosition(CurrentDemoIndex);
            cameraResizer.ResizeTo(simulationSize.y/2f);
            // Places the camera so perspective and ortho camera line up at simulation
            MoveCamera(demoPos, CameraDepth);
            cameraResizer.MoveSplitPoint(barSize.y,simulationSize.y);
        }

        /// <summary>
        /// Moves the camera to the specified position
        /// </summary>
        /// <param name="xy">xy position to place the camera at</param>
        /// <param name="z">depth/z position to place the camera at</param>
        private void MoveCamera(Vector2 xy, float z) => 
            mainCamera.transform.position = new Vector3(xy.x, xy.y, z);

        private Quaternion GetScaleModelRotation(ScaleModel scaleModel){
            if (scaleModel.shouldRotate)
                return FluidDemoFactory.RotateModel(scaleModel.prefab, scaleModel.scale);
            if (scaleModel.setUpAndForwardVectors)
                return Quaternion.LookRotation(scaleModel.forward, scaleModel.up);
            return Quaternion.identity;
        }

        private Vector3 GetScaleModelPosition(ScaleModel scaleModel, Vector3 demoPos, Quaternion rotation) => 
            demoPos + FluidDemoFactory.PlaceModel(scaleModel.prefab, 
                                                  scaleModel.scale, 
                                                  new Bounds(Vector3.zero, simulationSize),
                                                  rotation, 
                                                  ParticleSizeUtility.ToRadius(particleSize));


        // GIZMOS
        private void OnDrawGizmos(){
            if (hasCreated) return;
            var i = 0;
            scaleModels.ForEach(scaleModel => {
                var demoPos = DemoPosition(i++);
                
                // Draw simulation bounds
                Gizmos.color = Color.yellow;
                DrawSimulationBounds(new Bounds(demoPos, simulationSize));
        
                // Draw cylinder
                if (!useRealVolume){
                    Gizmos.color = Color.magenta;
                    DrawBarCylinderGizmo(new Bounds(demoPos 
                                                  + Vector3.up * (barSize.y-simulationSize.y)/2f, 
                                                    barSize));
                }
                Gizmos.color = Color.green;
                DrawScaleModelGizmos(scaleModel, demoPos);
            });
        }

        private void DrawScaleModelGizmos(ScaleModel scaleModel, Vector3 demoPos){
            var rotation = GetScaleModelRotation(scaleModel);
            var position = GetScaleModelPosition(scaleModel, demoPos, rotation);
            
            var meshFilter = scaleModel.prefab.GetComponent<MeshFilter>();
            if (meshFilter == null) meshFilter = scaleModel.prefab.GetComponentInChildren<MeshFilter>();
            
            var actualScale = FluidDemoFactory.AbsVector(rotation * scaleModel.scale);
            var rotateScaleBounds = FluidDemoFactory.RotateScaleBounds(meshFilter.sharedMesh.bounds, rotation, actualScale);

            var pos = position - Vector3.Scale(rotateScaleBounds.center, actualScale - Vector3.one);
            
            // Draw model mesh
            
            DrawModelGizmo(meshFilter.sharedMesh,
                           pos,
                           rotation,
                           scaleModel.scale);
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
