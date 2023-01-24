using System.Collections.Generic;
using Demo;
using UnityEngine;
using UnityEngine.Assertions;
using Utility;

namespace SimulationObjects.FluidBoundaryObject{
    public class FluidBoundaryCup : FluidBoundaryObject
    {
        [SerializeField] private Vector3 size;
        private void Start()
        {
            FluidDemo = GetComponentInParent<FluidDemo>();
            Assert.IsNotNull(FluidDemo);
            // ParticleSource = new ParticlesFromBounds(FluidBodyMeshDemo.Radius() * 2, OuterBounds(), InnerBounds());
            var spacing = FluidDemo.Radius() * 2;
            ParticleSource = new ParticlesFromList(spacing, CreateCup(spacing), Matrix4x4.identity);
            LoggingUtility.LogInfo($"FluidBoundaryCup {name} har a total of {ParticleSource.NumParticles} boundary particles!");
        }

        private List<Vector3> CreateCup(float spacing){
            var posList = new List<Vector3>();
            var halfSize = size * 0.5f;
            var floorMin = -halfSize;
            var floorMax = new Vector3(halfSize.x, -halfSize.y,halfSize.z);
            CreateFloor(spacing, 
                        posList, 
                        floorMin, 
                        floorMax, 
                        -halfSize.y);
            CreateWalls(spacing, 
                        posList, 
                        floorMin, 
                        floorMax,
                        -halfSize.y,
                        halfSize.y);
            return posList;
        }

        private void CreateWalls(float spacing, List<Vector3> posList, Vector3 min, Vector3 max, float yMin, float yMax){
            for (var y = yMin; y < yMax; y+=spacing)
                CreateRectanglePerimeter(spacing,posList,min,max,y);
        }

        private void CreateRectanglePerimeter(float spacing, List<Vector3> posList, Vector3 min, Vector3 max, float y){
            var globalPos = transform.position;
            for (var z = min.z; z < max.z; z+=spacing){
                var posMin = new Vector3(min.x, y, z);
                posList.Add(globalPos+posMin);
                var posMax = new Vector3(max.x, y, z);
                posList.Add(globalPos+posMax);
            }
            for (var x = min.x; x < max.x; x+=spacing){
                var posMin = new Vector3(x, y, min.z);
                posList.Add(globalPos+posMin);
                var posMax = new Vector3(x, y, max.z);
                posList.Add(globalPos+posMax);
            }
        }

        private void CreateFloor(float spacing, List<Vector3> posList, Vector3 min, Vector3 max, float y){
            for (var z = min.z; z < max.z; z+=spacing){
                for (var x = min.x; x < max.x; x+=spacing){
                    var pos = new Vector3(x, y, z);
                    posList.Add(transform.position+pos);
                }
            }
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position,size);
        }
    }
}
