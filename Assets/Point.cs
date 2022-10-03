using PBDFluid.Scripts;
using UnityEngine.Assertions;

namespace DefaultNamespace
{
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

        public Point Move(Point point) => Move(point.X, point.Y, point.Z);
        public Point Move(int x, int y, int z) {
            Assert.AreNotEqual(x+y+z,0, "Can't move in no direction");
            return new Point(this.X + x, this.Y + y, this.Z + z);
        }


        public static readonly Point[] SixNeighbourhood =
        {
            new Point(1, 0, 0), // RIGHT
            new Point(0, 1, 0), // UP
            new Point(0, 0, 1), // FORWARD
            new Point(-1, 0, 0), // LEFT
            new Point(0, -1, 0), // DOWN
            new Point(0, 0, -1), // BACK
        };
            
        public static readonly Point[] TwentySixNeighbourhood =
        { 
            //TOP RING
            new Point(  0, 1,  0), // UP
            new Point(  0, 1,  1), // UP FORWARD
            new Point(  1, 1,  1), // UP FORWARD RIGHT
            new Point(  1, 1,  0), // UP RIGHT
            new Point(  1, 1, -1), // UP RIGHT BACK
            new Point(  0, 1, -1), // UP BACK
            new Point( -1, 1, -1), // UP BACK LEFT
            new Point( -1, 1,  0), // UP LEFT
            new Point( -1, 1,  1), // UP FORWARD LEFT
            //BOTTOM RING
            new Point(  0, -1,  0), // DOWN
            new Point(  0, -1,  1), // DOWN FORWARD
            new Point(  1, -1,  1), // DOWN FORWARD RIGHT
            new Point(  1, -1,  0), // DOWN RIGHT
            new Point(  1, -1, -1), // DOWN RIGHT BACK
            new Point(  0, -1, -1), // DOWN BACK
            new Point( -1, -1, -1), // DOWN BACK LEFT
            new Point( -1, -1,  0), // DOWN LEFT
            new Point( -1, -1,  1), // DOWN FORWARD LEFT
            //MIDDLE RING
            new Point(  0, 0,  1), // FORWARD
            new Point(  1, 0,  1), // FORWARD RIGHT
            new Point(  1, 0,  0), // RIGHT
            new Point(  1, 0, -1), // RIGHT BACK
            new Point(  0, 0, -1), // BACK
            new Point( -1, 0, -1), // BACK LEFT
            new Point( -1, 0,  0), // LEFT
            new Point( -1, 0,  1), // FORWARD LEFT
        };
            
    }
}