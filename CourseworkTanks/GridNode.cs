namespace GridWorld
{
    /// <summary>
    /// Stores data about each Node in the game when they are being searched.
    /// </summary>
    class GridNode
    {
        public int x;
        public int y;
        public Cell cell;
        public int hCost;
        public int gCost;
        public bool walkable;
        public GridNode parent;

       public GridNode(int x, int y, Cell cell){

            this.x = x;
            this.y = y;
            this.cell = cell;

            if (cell == Cell.Empty || cell == Cell.Hero)
            {
                walkable = true;
            }
            else
            {
                walkable = false;
            }

        }

        /// <summary>
        /// Returns the F-cost of this GridNode.
        /// </summary>
        /// <returns>The F-cost of this GridNode.</returns>
        public int fCost
        {
            get
            {
                return hCost + gCost;
            }
        }

        public Tuple<int, int> ToTuple()
        {

            return new Tuple<int, int>(x, y);
        }


    }
}
