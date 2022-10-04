using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

namespace PBDFluid.Scripts
{
    public class MeshHollower
    {
        public readonly List<Point> HullVoxels;
        private readonly int width;
        private readonly int height;
        private readonly int depth;
        private readonly bool[,,] visited;
        public readonly int[,,] voxels;

        public MeshHollower(int[,,] voxels)
        {
            this.voxels = PadGrid(voxels);
            width = this.voxels.GetLength(0);
            height = this.voxels.GetLength(1);
            depth = this.voxels.GetLength(2);
            
            //+2 due to padding
            visited = new bool[width, height, depth];
            HullVoxels = new List<Point>();
            DFS(FindMeshVoxel());
            Debug.Log($"hull voxels size is {HullVoxels.Count}");
        }

        private static int[,,] PadGrid(int[,,] grid) {
            var w = grid.GetLength(0) + 2;
            var h = grid.GetLength(1) + 2;
            var d = grid.GetLength(2) + 2;
            var paddedGrid = new int[w,h,d];
            for (var z = 1; z < d-1; z++) 
                for (var y = 1; y < h-1; y++) 
                    for (var x = 1; x < w-1; x++) 
                        paddedGrid[x, y, z] = grid[x-1, y-1, z-1];
            return paddedGrid;
        }

        /** Searches grid until it finds a voxel within mesh, then returns its position */
        private Point FindMeshVoxel() {
            for (var z = 0; z < depth; z++)
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++) 
                        if (IsInMesh(x, y, z))
                            return new Point(x, y, z); 
            throw new Exception("Could not find the mesh");
        }

        // ReSharper disable once InconsistentNaming
        private void DFS(Point start) {
            var stack = new Stack<Point>();
            stack.Push(start);
            while (stack.Count > 0) {
                var point = stack.Pop();
                if (IsVisited(point)) continue;
                SetVisited(point);
                if (!IsInMesh(point)) {
                    SetAsHullVoxel(point);
                    continue;
                }
                Neighbours(point).ForEach(neighbour => stack.Push(neighbour));
            }
        }

        private void SetAsHullVoxel(Point point) => HullVoxels.Add(new Point(point.X-1,point.Y-1,point.Z-1));
        
        /** Searches through neighbouring cells, and returns those that can be moved to */
        private List<Point> Neighbours(Point point) => (from point1 in Point.TwentySixNeighbourhood 
                                                        where CanGo(point.Move(point1)) 
                                                        select point.Move(point1)).ToList();

        /** Returns true if Point is both within bounds, and not visited */
        private bool CanGo(Point point) => CanGo(point.X, point.Y, point.Z);
        private bool CanGo(int x, int y, int z) => IsWithinBounds(x,y,z) &&
                                                   !IsVisited(x,y,z);
        
        /** Returns true if a point is within bounds with regards to padding */
        private bool IsWithinBounds(int x, int y, int z) => (x >= 0 && x < width) &&
                                                            (y >= 0 && y < height) &&
                                                            (z >= 0 && z < depth);
        
        /** Sets this point as visited */
        private void SetVisited(Point point) => visited[point.X, point.Y, point.Z] = true;
        
        /** Returns true if point is visited */
        private bool IsVisited(Point point) => IsVisited(point.X,point.Y,point.Z);
        private bool IsVisited(int x, int y, int z) => visited[x, y, z];

        /** Returns true if Point is within the voxelized mesh */
        private bool IsInMesh(Point point) => IsInMesh(point.X, point.Y, point.Z);
        private bool IsInMesh(int x, int y, int z) => IsWithinBounds(x,y,z) &&
                                                      voxels[x, y, z] > 0;
        
    }
}