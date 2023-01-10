namespace Utility{
    public readonly struct Point
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public Point(int x, int y, int z) {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary> Moves this point by the given direction </summary>
        /// <returns> The new point </returns>
        public Point Move(Point direction) => Move(direction.X, 
                                                   direction.Y, 
                                                   direction.Z);

        /// <inheritdoc cref="Move"/>
        private Point Move(int x, int y, int z) => new Point(X + x, 
                                                             Y + y, 
                                                             Z + z);


        /// <summary> Returns the neighbourhood of each 6 directions </summary>
        public static readonly Point[] SixNeighbourhood =
        {
            new Point(1, 0, 0), // RIGHT
            new Point(0, 1, 0), // UP
            new Point(0, 0, 1), // FORWARD
            new Point(-1, 0, 0), // LEFT
            new Point(0, -1, 0), // DOWN
            new Point(0, 0, -1), // BACK
        };
            
        
        /// <summary> Returns the complete neighbourhood directions</summary>
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