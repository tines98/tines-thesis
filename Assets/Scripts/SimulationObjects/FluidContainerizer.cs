using System;
using System.Collections.Generic;
using MeshVoxelizer.Scripts;
using PBDFluid.Scripts;
using UnityEngine;
using UnityEngine.Assertions;

namespace SimulationObjects{
    public class FluidContainerizer : MonoBehaviour
    {
        [SerializeField] private bool drawGrid;
        [SerializeField] private bool drawMeshBounds;
        public VoxelizerDemo voxelizerDemo;
        private MeshHollower meshHollower;
        [NonSerialized] public Bounds GlobalMeshBounds;

        public List<Box3> ExteriorVoxels;
        public List<Box3> InteriorVoxels;

        // Start is called before the first frame update
        private void Start()
        {
            voxelizerDemo = GetComponentInParent<VoxelizerDemo>();
            if (voxelizerDemo == null) voxelizerDemo = GetComponent<VoxelizerDemo>();
            voxelizerDemo.SetVoxelizedMeshVisibility(false);
            
            meshHollower = new MeshHollower(voxelizerDemo.Voxelizer.Voxels);
            ExteriorVoxels = new List<Box3>(meshHollower.HullVoxels.Count);
            InteriorVoxels = new List<Box3>(voxelizerDemo.Voxels.Count);

            CalculateExterior();
            CalculateInterior();

            var trs = transform.localToWorldMatrix;
            GlobalMeshBounds = new Bounds(trs.MultiplyPoint(voxelizerDemo.meshBounds.center), trs.MultiplyVector(voxelizerDemo.meshBounds.size));
            GlobalMeshBounds.SetMinMax(Vector3.Min(GlobalMeshBounds.min, GlobalMeshBounds.max),
                                       Vector3.Max(GlobalMeshBounds.min, GlobalMeshBounds.max));
            Debug.Log(GlobalMeshBounds.size);
            Debug.Log(voxelizerDemo.radius);
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
                ExteriorVoxels.Add(voxelizerDemo.GetVoxel(point.X, 
                                                          point.Y, 
                                                          point.Z)));

    
        /// <summary>Puts the voxels from the voxelized mesh into InteriorVoxels</summary>
        private void CalculateInterior() => InteriorVoxels = voxelizerDemo.Voxels;
    
    
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            if (drawMeshBounds)
                DrawMeshBounds();
        
            Gizmos.color = Color.grey;
            if (drawGrid) 
                DrawGridGizmo(meshHollower.Voxels, transform.localToWorldMatrix);
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
                        var box = voxelizerDemo.GetVoxel(x-1, 
                                                         y-1, 
                                                         z-1);
                        Gizmos.DrawWireCube(trs.MultiplyPoint(box.Center),
                                            trs.MultiplyVector(box.Size));
                    }
                }
            }
        }
    }
}
