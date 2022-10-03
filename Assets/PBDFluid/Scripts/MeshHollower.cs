using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;

namespace PBDFluid.Scripts
{
    public class MeshHollower
    {
        private readonly int width;
        private readonly int height;
        private readonly int depth;
        public readonly bool[,,] Visited;
        public readonly bool[,,] HullVoxels;
        private readonly int[,,] voxels;

        public MeshHollower(int[,,] voxels)
        {
            this.voxels = voxels;
            width = voxels.GetLength(0);
            height = voxels.GetLength(1);
            depth = voxels.GetLength(2);
            
            //+2 due to padding
            Visited = new bool[width+2, height+2, depth+2];
            HullVoxels = new bool[width+2, height+2, depth+2];

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
                    HullVoxels[point.X, point.Y, point.Z] = true;
                    continue;
                }
                var neighbours = Neighbours(point);
                neighbours.ForEach(neighbour => stack.Push(neighbour));
            }
        }

        private List<Point> Neighbours(Point point) => (
                from point1 in Point.TwentySixNeighbourhood 
                where CanGo(point.Move(point1)) 
                select point.Move(point1)).ToList();

        /**
         * Returns true if Point is both within bounds, and not visited
         */
        private bool CanGo(Point point) => CanGo(point.X, point.Y, point.Z);
        private bool CanGo(int x, int y, int z) =>
            IsWithinBounds(x,y,z) &&
            !IsVisited(x,y,z);
        
        /**
         * Returns true if a point is within bounds with regards to padding 
         */
        private bool IsWithinBounds(int x, int y, int z) => (x >= 0 && x < width+2) &&
                                                         (y >= 0 && y < height+2) &&
                                                         (z >= 0 && z < depth+2);
        /**
         * Returns true if a point is within bounds without regards to padding 
         */
        private bool IsWithinBoundsNoPadding(Point point) => IsWithinBoundsNoPadding(point.X, point.Y, point.Z);
        private bool IsWithinBoundsNoPadding(int x, int y, int z) => (x >= 1 && x < width) &&
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
            IsWithinBoundsNoPadding(point) &&
            voxels[point.X-1, point.Y-1, point.Z-1] > 0;

        
    }
}