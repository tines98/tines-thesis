using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MeshVoxelizer.Scripts;
using PBDFluid.Scripts;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

namespace SimulationObjects{
    public class FluidContainerizer : MonoBehaviour
    {
        [SerializeField] private bool drawGrid;
        [SerializeField] private bool drawMeshBounds;
        [SerializeField] private bool drawGrid2;
        [SerializeField] private float drawGrid2Radius;
        // public VoxelizerDemo voxelizerDemo;
        public VoxelizedMesh voxelizedMesh;
        private MeshHollower meshHollower;
        [NonSerialized] public Bounds GlobalMeshBounds;

        public List<Box3> ExteriorVoxels;
        public List<Box3> InteriorVoxels;

        // Start is called before the first frame update
        private void Start()
        {
            // voxelizerDemo = GetComponentInParent<VoxelizerDemo>();
            // if (voxelizerDemo == null) voxelizerDemo = GetComponent<VoxelizerDemo>();
            // voxelizerDemo.SetVoxelizedMeshVisibility(false);
            
            voxelizedMesh = GetComponent<VoxelizedMesh>();
            var voxels = voxelizedMesh.GetVoxels();
            Profiler.BeginSample("MeshHollowing");
            meshHollower = new MeshHollower(voxels);
            Profiler.EndSample();
            ExteriorVoxels = new List<Box3>(meshHollower.HullVoxels.Count);
            // InteriorVoxels = new List<Box3>(voxelizedMesh._voxels.Length);

            CalculateExterior();
            CalculateInterior3();

            var trs = transform.localToWorldMatrix;
            GlobalMeshBounds = new Bounds((voxelizedMesh.MeshBounds.center), (voxelizedMesh.MeshBounds.size));
            GlobalMeshBounds.SetMinMax(Vector3.Min(GlobalMeshBounds.min, GlobalMeshBounds.max),
                                       Vector3.Max(GlobalMeshBounds.min, GlobalMeshBounds.max));
            // Debug.Log(GlobalMeshBounds.size);
            Assert.IsTrue(ExteriorVoxels.Count > 0, "Exterior is empty");
            Assert.IsTrue(InteriorVoxels.Count > 0, "Interior is empty");
            Assert.IsTrue(IsReady,"IsReady is implemented wrong");
        }

    
        /// <returns>True if the fluid container is done being containerized</returns>
        public bool IsReady => ExteriorVoxels!=null 
                            && InteriorVoxels!=null 
                            && ExteriorVoxels.Count > 0 
                            && InteriorVoxels.Count > 0;
    
    
        /// <summary>
        /// Calculates the voxels for each point in the hull voxels from meshHollower
        /// <see cref="MeshHollower"/>
        /// </summary>
        private void CalculateExterior() => 
            meshHollower.HullVoxels.ForEach(point => 
                ExteriorVoxels.Add(voxelizedMesh.GetVoxel(new Vector3(point.X,point.Y,point.Z))));
    
        
        private void CalculateExterior2(){
            ExteriorVoxels = new List<Box3>();
            var voxelSize = Vector3.one * voxelizedMesh._halfSize * 2f;
            foreach (var voxel in voxelizedMesh._voxels){
                if (voxel.isSolid > 1.5f){
                    ExteriorVoxels.Add( new Box3{
                        Min = voxel.position, 
                        Max = voxel.position + voxelSize});
                }
            }
        }

        /// <summary>Puts the voxels from the voxelized mesh into InteriorVoxels</summary>
        // private void CalculateInterior() => InteriorVoxels = voxelizerDemo.Voxels;

        private void CalculateInterior3(){
            InteriorVoxels = new List<Box3>();
            // Debug.Log("CONTAINERIZER voxelizedMesh._halfSize = " + voxelizedMesh._halfSize);
            var voxelSize = Vector3.one * voxelizedMesh._halfSize * 2f;
            foreach (var voxel in voxelizedMesh._voxels){
                if (voxel.isSolid is > 0.5f and < 1.5f){
                    var voxelBox = new Box3(voxel.position, voxel.position + voxelSize);
                    InteriorVoxels.Add(voxelBox);
                }
            }
        }

        private void CalculateInterior2(int[,,] voxels){
            InteriorVoxels = new List<Box3>();
            for (var z = 0; z < voxels.GetLength(2); z++){
                for (var y = 0; y < voxels.GetLength(1); y++){
                    for (var x = 0; x < voxels.GetLength(0); x++){
                        if (voxels[x, y, z] != 1) continue;
                        var voxel = voxelizedMesh.GetVoxelOnIndex(x, y, z);
                        InteriorVoxels.Add(voxelizedMesh.GetVoxel(x,y,z));
                    }
                }
            }
        }   
    
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            if (drawMeshBounds)
                DrawMeshBounds();
            if (drawGrid2)
                DrawGrid2Gizmo();
            
            Gizmos.color = Color.grey;
            if (drawGrid) 
                DrawGridGizmo(meshHollower.Voxels, transform.localToWorldMatrix);
        }

        void DrawGrid2Gizmo(){
            var voxelSize = Vector3.one * 0.08f * 2f;
            foreach (var voxel in voxelizedMesh._voxels){
                if (voxel.isSolid < 0.5f) 
                    Gizmos.color = Color.gray;
                else if (voxel.isSolid < 1.5f) 
                    Gizmos.color = Color.blue;
                else 
                    Gizmos.color = Color.red;
                var voxelBox = new Box3(voxel.position, voxel.position + voxelSize);
                Gizmos.DrawWireSphere(voxel.position, drawGrid2Radius);
                // Gizmos.DrawWireCube(voxelBox.Center, voxelBox.Size);
            }
        }
    
    
        /// <summary>Draws the mesh bounds</summary>
        private void DrawMeshBounds() => Gizmos.DrawWireCube(GlobalMeshBounds.center,
                                                             GlobalMeshBounds.size);

    
        /// <summary>Draws the grid</summary>
        private void DrawGridGizmo(int[,,] voxels, Matrix4x4 trs){
            for (var z = 0; z < voxels.GetLength(2); z++) {
                for (var y = 0; y < voxels.GetLength(1); y++) {
                    for (var x = 0; x < voxels.GetLength(0); x++) {
                        if (voxels[x,y,z] == 1) continue;
                        var box = voxelizedMesh.GetVoxel(x-1, 
                                                         y-1, 
                                                         z-1);
                        Gizmos.DrawWireCube((box.Center),
                                            (box.Size));
                    }
                }
            }
        }
    }
}
