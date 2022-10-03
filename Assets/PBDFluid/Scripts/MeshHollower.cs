using System.Collections.Generic;
using UnityEngine.Assertions;

namespace PBDFluid.Scripts
{
    public class MeshHollower
    {
        private readonly int width;
        private readonly int height;
        private readonly int depth;
        public readonly bool[,,] Visited;
        public readonly bool[,,] HullVoxels2;
        private readonly int[,,] voxels;

        public MeshHollower(int[,,] voxels)
        {
            this.voxels = voxels;
            width = voxels.GetLength(0);
            height = voxels.GetLength(1);
            depth = voxels.GetLength(2);
            
            //+2 due to padding
            Visited = new bool[width+2, height+2, depth+2];
            HullVoxels2 = new bool[width+2, height+2, depth+2];

            DFS();
        }

        // ReSharper disable once InconsistentNaming
        private void DFS()
        {
            var stack = new Stack<Point>();
            var root = new Point(0,0,0);
            stack.Push(root);
            while (stack.Count > 0) {
                var point = stack.Pop();
                if (IsVisited(point)) continue;
                SetVisited(point);
                if (IsInMesh(point)) {
                    HullVoxels2[point.X, point.Y, point.Z] = true;
                    continue;
                }
                var neighbours = Neighbours(point);
                neighbours.ForEach(neighbour => stack.Push(neighbour));
            }
        }

        private List<Point> Neighbours(Point point) {
            var neighbours = new List<Point>();
            if (CanGo(point.Move(1, 0, 0)))
                neighbours.Add(point.Move(1, 0, 0));

            if (CanGo(point.Move(0, 1, 0))) 
                neighbours.Add(point.Move(0, 1, 0));

            if (CanGo(point.Move(0, 0, 1)))
                neighbours.Add(point.Move(0, 0, 1));

            if (CanGo(point.Move(-1, 0, 0)))
                neighbours.Add(point.Move(-1, 0, 0));

            if (CanGo(point.Move(0, -1, 0)))
                neighbours.Add(point.Move(0, -1, 0));

            if (CanGo(point.Move(0, 0, -1)))
                neighbours.Add(point.Move(0, 0, -1));
            
            return neighbours;
        }

        /**
         * Returns true if Point is both within bounds, and not visited
         */
        private bool CanGo(Point point) => CanGo(point.X, point.Y, point.Z);
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
        private bool OutOfBoundsNoPadding(Point point) => OutOfBoundsNoPadding(point.X, point.Y, point.Z);
        private bool OutOfBoundsNoPadding(int x, int y, int z) => (x >= 1 && x < width) &&
                                                               (y >= 1 && y < height) &&
                                                               (z >= 1 && z < depth);
        /**
         * Sets this point as visited
         */
        private void SetVisited(Point point) => Visited[point.X, point.Y, point.Z] = true;
        
        /**
         * Returns true if point is visited
         */
        private bool IsVisited(Point point) => IsVisited(point.X,point.Y,point.Z);
        private bool IsVisited(int x, int y, int z) => Visited[x, y, z];

        /**
         * Returns true if Point is within the voxelized mesh
         */
        private bool IsInMesh(Point point) => 
            OutOfBoundsNoPadding(point) &&
            voxels[point.X-1, point.Y-1, point.Z-1] > 0;

        public readonly struct Point
        {
            public readonly int X;
            public readonly int Y;
            public readonly int Z;

            public Point(int x, int y, int z)
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
            }

            public Point Move(int x, int y, int z) {
                Assert.AreNotEqual(x+y+z,0, "Can't move in no direction");
                return new Point(this.X + x, this.Y + y, this.Z + z);
            }
            
        }
    }
}