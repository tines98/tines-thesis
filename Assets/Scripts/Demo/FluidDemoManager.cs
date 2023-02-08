using System;
using System.Collections.Generic;
using Factories;
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
        public int currentDemoIndex;
        private CameraResizer cameraResizer;
        [NonSerialized] public bool FinishedAllDemos;

        private List<FluidDemo> fluidDemos;
        private bool hasCreated;
        private Camera mainCamera;

        public int DemoCount => 
            fluidDemos.Count;

        private FluidDemo CurrentDemo => 
            fluidDemos[currentDemoIndex % DemoCount];

        private Vector3 FirstDemo => 
            DemoPosition(0);

        private Vector3 LastDemo => 
            DemoPosition(scaleModels.Count - 1);


        private float CalculateCameraSize => 
            (LastDemo - FirstDemo).magnitude / 2f;

        private Vector3 MidPoint => 
            FirstDemo + (LastDemo - FirstDemo)/2f;

        private float CameraDepth =>
            - simulationSize.z / 2f
            - mainCamera.orthographicSize / Mathf.Tan(cameraResizer.GetFOV() / 2f * Mathf.Deg2Rad);


        // Start is called before the first frame update
        void Start(){
            // renderSettings.cylinderMaterial.SetVector("_Tiling", new(1,barNotches));
            renderSettings.cylinderMaterial.mainTextureScale = new Vector2(1,barNotches);
            CreateDemos();
            mainCamera = Camera.main;
            if (mainCamera != null) cameraResizer = mainCamera.GetComponent<CameraResizer>();
            PlaceCameraAtDemo();
        }


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
                Gizmos.color = Color.magenta;
                DrawBarCylinderGizmo(new Bounds(demoPos 
                                              + Vector3.up * (barSize.y-simulationSize.y)/2f, 
                                                barSize));
                Gizmos.color = Color.green;
                DrawScaleModelGizmos(scaleModel, demoPos);
            });
        }

        public void NextDemo(){
            if (FinishedAllDemos) return;
            UpdateDeathPlane(0f);
            currentDemoIndex++;
            if (currentDemoIndex == scaleModels.Count) AllDemosComplete();
            else PlaceCameraAtDemo();
        }

        public void UpdateDeathPlane(float value) => 
            CurrentDemo.DeathPlaneHeight = value;

        public void StartDemo() => 
            CurrentDemo.StartDemo();

        public void StopDemo() => 
            CurrentDemo.StopDemo();

        void AllDemosComplete(){
            Debug.Log("All demos complete");
            FinishedAllDemos = true;
            cameraResizer.ResizeTo(CalculateCameraSize);
            cameraResizer.MoveSplitPoint(1f);
            MoveCamera(MidPoint, CameraDepth);
        }


        /// <summary>
        /// Iterates through <see cref="scaleModels"/> and calls <see cref="CreateDemo"/> for each of them
        /// </summary>
        void CreateDemos(){
            fluidDemos = new List<FluidDemo>(scaleModels.Count);
            var index = 0;
            scaleModels.ForEach(_ => fluidDemos.Add(CreateDemo(index++)));
            hasCreated = true;
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
            MoveCamera(demoPos, CameraDepth);
            cameraResizer.MoveSplitPoint(barSize.y,simulationSize.y);
        }

        private void MoveCamera(Vector2 xy, float z) => 
            mainCamera.transform.position = new Vector3(xy.x, xy.y, z);

        private Quaternion GetScaleModelRotation(ScaleModel scaleModel){
            if (scaleModel.shouldRotate)
                return FluidDemoFactory.RotateModel(scaleModel.prefab, scaleModel.scale);
            if (scaleModel.forceRotate)
                return Quaternion.LookRotation(scaleModel.forward, scaleModel.up);
            return Quaternion.identity;
        }

        private Vector3 GetScaleModelPosition(ScaleModel scaleModel, Vector3 demoPos, Quaternion rotation){
            var simulationBounds = new Bounds(Vector3.zero, simulationSize);
            var position = demoPos 
                         + FluidDemoFactory.PlaceModel(scaleModel.prefab, 
                                                       scaleModel.scale, 
                                                       simulationBounds,
                                                       rotation);
            return position;
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
