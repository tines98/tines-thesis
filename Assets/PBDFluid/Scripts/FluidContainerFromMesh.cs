using System.Collections.Generic;
using MeshVoxelizerProject;
using PBDFluid.Scripts;
using UnityEngine;

namespace PBDFluid
{
    public class FluidContainerFromMesh
    {
        public List<Box3> LocalExteriorVoxels;
        public List<Box3> ExteriorVoxels;
        public List<Box3> InteriorVoxels;
        public List<Box3> LocalInteriorVoxels;
        public Bounds bounds;
        public ParticleSource FluidParticleSource;
        public ParticleSource BoundaryParticleSource;
        private VoxelizerDemo _voxelizerDemo;
        private MeshHollower _meshHollower;
        

        public FluidContainerFromMesh(VoxelizerDemo voxelizerDemo, float radius, float density)
        {
            _voxelizerDemo = voxelizerDemo;


            bounds = new Bounds(voxelizerDemo.bounds.Center,voxelizerDemo.bounds.Size);
            
            _meshHollower = new MeshHollower(_voxelizerDemo.m_voxelizer.Voxels);
            
            CalculateExterior();
            CalculateInterior();
            
            CreateFluid(radius);
            CreateBoundary(radius, density);
        }

        private void CalculateExterior()
        {
            ExteriorVoxels = new List<Box3>();
            LocalExteriorVoxels = new List<Box3>();
            foreach (var point in _meshHollower.hullVoxels) {
                ExteriorVoxels.Add(_voxelizerDemo.GetVoxel(point.x,point.y,point.z));
            }
        }

        private void CalculateInterior() {
            InteriorVoxels = new List<Box3>();
            LocalInteriorVoxels = new List<Box3>();
            for (int z = 0; z < _meshHollower.visited.GetLength(2); z++) {
                for (int y = 0; y < _meshHollower.visited.GetLength(1); y++) {
                    for (int x = 0; x < _meshHollower.visited.GetLength(0); x++) {
                        if (!_meshHollower.visited[x, y, z]) {
                            InteriorVoxels.Add(_voxelizerDemo.GetVoxel(x,y,z));
                        } 
                    }
                }
            }
        }


        private void CreateFluid(float radius) {
            var diameter = radius * 2;

            var exclusion = new List<Bounds>();
            for (int x = 0; x < _meshHollower.visited.GetLength(0); x++) {
                for (int y = 0; y < _meshHollower.visited.GetLength(1); y++) {
                    for (int z = 0; z < _meshHollower.visited.GetLength(2); z++) {
                        if (_meshHollower.visited[x, y, z])
                        {
                            var voxel = _voxelizerDemo.GetVoxel(x, y, z);
                            var bound = new Bounds(voxel.Center, voxel.Size);
                            exclusion.Add(bound);
                        }
                    }
                }
            }
            FluidParticleSource = new ParticlesFromBounds(diameter, bounds, exclusion);
        }

        private void CreateBoundary(float radius, float density) {
            var diameter = radius * 2;
            BoundaryParticleSource = new ParticlesFromVoxels(diameter, ExteriorVoxels);
        }
        public void DrawBoundaryGizmo()
        {
            for (int z = 0; z < _meshHollower.visited.GetLength(2); z++) {
                for (int y = 0; y < _meshHollower.visited.GetLength(1); y++) {
                    for (int x = 0; x < _meshHollower.visited.GetLength(0); x++) {
                        if (_meshHollower.hullVoxels2[x, y, z])
                        {
                            var voxel = _voxelizerDemo.GetVoxel(x, y, z, true);
                            Gizmos.DrawCube(voxel.Center, voxel.Size);
                        }
                    }
                }
            }
        }

        public void DrawInteriorVoxelsGizmo()
        {
            Gizmos.color = Color.blue;
            foreach (var voxel in InteriorVoxels)
            {
                Gizmos.DrawWireCube(voxel.Center,voxel.Size);
            }
        }
        
        public void DrawExteriorVoxelsGizmo()
        {
            Gizmos.color = Color.red;
            foreach (var voxel in ExteriorVoxels)
            {
                Gizmos.DrawWireCube( voxel.Center,voxel.Size);
            }
        }
        
    }
}