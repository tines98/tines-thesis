using System.Collections.Generic;
using MeshVoxelizerProject;
using UnityEngine;
using UnityEngine.Assertions;

namespace PBDFluid
{
    public class MeshHollower
    {
        private int width, height, depth;
        public bool[,,] visited;
        private int[,,] voxels;
        public List<Point> hullVoxels;
        public bool[,,] hullVoxels2;

        public MeshHollower(int[,,] voxels)
        {
            this.voxels = voxels; 
            width = voxels.GetLength(0);
            height = voxels.GetLength(1);
            depth = voxels.GetLength(2);
            
            //+2 due to padding
            visited = new bool[width+2, height+2, depth+2];
            hullVoxels2 = new bool[width+2, height+2, depth+2];
            hullVoxels = new List<Point>();
            
            Point start = new Point(0, 0, 0);
            DFS(start);
        }

        // ReSharper disable once InconsistentNaming
        private void DFS(Point point)
        {
            while (true)
            {
                SetVisited(point);
                if (IsInMesh(point))
                {
                    hullVoxels.Add(point);
                    hullVoxels2[point.x, point.y, point.z] = true;
                    return;
                }

                if (CanGo(point.Move(1, 0, 0))) DFS(point.Move(1, 0, 0));

                if (CanGo(point.Move(0, 1, 0))) DFS(point.Move(0, 1, 0));

                if (CanGo(point.Move(0, 0, 1))) DFS(point.Move(0, 0, 1));

                if (CanGo(point.Move(-1, 0, 0))) DFS(point.Move(-1, 0, 0));

                if (CanGo(point.Move(0, -1, 0))) DFS(point.Move(0, -1, 0));

                if (CanGo(point.Move(0, 0, -1)))
                {
                    point = point.Move(0, 0, -1);
                    continue;
                }

                break;
            }
        }

        /**
         * Returns true if Point is both within bounds, and not visited
         */
        private bool CanGo(Point point) => CanGo(point.x, point.y, point.z);
        private bool CanGo(int x, int y, int z) =>
            OutOfBounds(x,y,z) &&
            !IsVisited(x,y,z);
        
        /**
         * Returns whether a point is out of bounds with regards to padding 
         */
        private bool OutOfBounds(int x, int y, int z) => (x >= 0 && x < width+2) &&
                                                         (y >= 0 && y < height+2) &&
                                                         (z >= 0 && z < depth+2);
        /**
         * Returns whether a point is out of bounds without regards to padding 
         */
        private bool OutOfBoundsNoPadding(Point point) => OutOfBoundsNoPadding(point.x, point.y, point.z);
        private bool OutOfBoundsNoPadding(int x, int y, int z) => (x >= 1 && x < width) &&
                                                               (y >= 1 && y < height) &&
                                                               (z >= 1 && z < depth);
        /**
         * Sets this point as visited
         */
        private void SetVisited(Point point) => visited[point.x, point.y, point.z] = true;
        
        /**
         * Returns true if point is visited
         */
        private bool IsVisited(int x, int y, int z) => visited[x, y, z];

        /**
         * Returns true if Point is within the voxelized mesh
         */
        private bool IsInMesh(Point point) => 
            OutOfBoundsNoPadding(point) &&
            voxels[point.x-1, point.y-1, point.z-1] > 0;

        public struct Point
        {
            public int x, y, z;

            public Point(int x, int y, int z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public Point Move(int x, int y, int z) {
                Assert.AreNotEqual(x+y+z,0, "Can't move in no direction");
                return new Point(this.x + x, this.y + y, this.z + z);
            }
        }
    }
}