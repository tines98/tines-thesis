using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        
        /// <summary>
        /// Takes in a 3D grid and adds a padding around it
        /// </summary>
        /// <param name="grid">Grid to add padding to</param>
        /// <returns>The new padded grid</returns>
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

        
        /// <summary>
        /// Searches grid until it finds a voxel within mesh
        /// </summary>
        /// <returns>The first point found</returns>
        /// <exception cref="Exception">if no point is found</exception>
        private Point FindMeshVoxel() {
            for (var z = 0; z < depth; z++)
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++) 
                        if (IsInMesh(x, y, z))
                            return new Point(x, y, z); 
            throw new Exception("Could not find the mesh");
        }

        
        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Iterative Depth-First Search implementation
        /// To find a hull of voxels around the mesh   
        /// </summary>
        /// <param name="start">Start point for the algorithm (must be within the mesh)</param>
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

        
        /// <summary>
        /// Adds a point into the HullVoxels list
        /// </summary>
        /// <param name="point">Point to add</param>
        private void SetAsHullVoxel(Point point) => HullVoxels.Add(new Point(point.X-1,point.Y-1,point.Z-1));
        

        /// <summary>
        /// Searches through neighbouring cells, and checks if they fulfill the CanGo method, then adds them to a list
        /// </summary>
        /// <param name="point"></param>
        /// <returns>List of neighbouring cells that can be moved to</returns>
        private List<Point> Neighbours(Point point) => (from point1 in Point.TwentySixNeighbourhood 
                                                        where CanGo(point.Move(point1)) 
                                                        select point.Move(point1)).ToList();


        /// <returns>Returns true if Point is both within bounds, and not visited</returns>
        private bool CanGo(Point point) => CanGo(point.X, point.Y, point.Z);
        
        /// <inheritdoc cref="CanGo(Point)"/>
        private bool CanGo(int x, int y, int z) => IsWithinBounds(x,y,z) &&
                                                   !IsVisited(x,y,z);
   
        
        /// <returns>Returns true if a point is within bounds with regards to padding</returns>
        private bool IsWithinBounds(int x, int y, int z) => (x >= 0 && x < width) &&
                                                            (y >= 0 && y < height) &&
                                                            (z >= 0 && z < depth);
  
        
        /// <summary> Sets a point as visited </summary>
        private void SetVisited(Point point) => visited[point.X, point.Y, point.Z] = true;
   
        
        ///<returns> Returns true if point is visited </returns>
        private bool IsVisited(Point point) => IsVisited(point.X,point.Y,point.Z);
        
        /// <inheritdoc cref="IsVisited(Point)"/>
        private bool IsVisited(int x, int y, int z) => visited[x, y, z];
        
        
        /// <returns>Returns true if Point is within the voxelized mesh</returns>
        private bool IsInMesh(Point point) => IsInMesh(point.X, point.Y, point.Z);
        
        /// <inheritdoc cref="IsInMesh(Point)"/>
        private bool IsInMesh(int x, int y, int z) => IsWithinBounds(x,y,z) &&
                                                      voxels[x, y, z] > 0;
        
    }
}