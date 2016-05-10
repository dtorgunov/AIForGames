namespace GridWorld
{
    /// <summary>
    /// Stores data about each Node in the game when they are being searched.
    /// </summary>
    class GridNode
    {
        int x;
        int y;
        int tile;
        int hCost;
        int gCost;
        int fCost;
        bool walkable;
        GridNode parent;

        /// <summary>
        /// Constructor. Takes the relevant values and constructs an instance of the class.
        /// </summary>
        /// <param name="x">The X coordinate of this GridNode.</param>
        /// <param name="y">The Y coordinate of this GridNode.</param>
        /// <param name="tile">The representative value of the object in this GridNode.</param>
        /// <param name="id">The representative value of the player's Snail.</param>
        public GridNode(int x, int y, int tile, int id)
        {
            this.x = x;
            this.y = y;
            this.tile = tile;

            if (tile == id || tile == -id || tile == 0)
            {
                walkable = true;
            }
            else
            {
                walkable = false;
            }

        }

        /// <summary>
        /// Returns a value representing if this GridNode is walkable.
        /// </summary>
        /// <returns>True if walkable, false if not.</returns>
        public bool GetWalkable()
        {
            return walkable;
        }

        /// <summary>
        /// Returns the parent GridNode of this GridNode
        /// </summary>
        /// <returns>The parent GridNode. Will be null for the player's current location.</returns>
        public GridNode GetParent()
        {
            return parent;
        }

        /// <summary>
        /// Sets the value of parent to a specified value.
        /// </summary>
        /// <param name="parent">The new parent of this GridNode.</param>
        public void SetParent(GridNode parent)
        {
            this.parent = parent;
        }

        /// <summary>
        /// Returns the H-cost of this GridNode.
        /// </summary>
        /// <returns>The H-cost of this GridNode.</returns>
        public int GetHCost()
        {
            return hCost;
        }

        /// <summary>
        /// Sets the H-cost of this GridNode and uses it to calculate the F-cost.
        /// </summary>
        /// <param name="hCost">The new H-cost.</param>
        public void SetHCost(int hCost)
        {
            this.hCost = hCost;
            SetFCost();
        }

        /// <summary>
        /// Returns the G-cost of this GridNode.
        /// </summary>  
        /// <returns>The G-cost of this GridNode.</returns>
        public int GetGCost()
        {
            return gCost;
        }

        /// <summary>
        /// Sets the G-cost of this GridNode and uses it to calculate the F-cost.
        /// </summary>
        /// <param name="hCost">The new G-cost.</param>
        public void SetGCost(int gCost)
        {
            this.gCost = gCost;
            SetFCost();
        }

        /// <summary>
        /// Returns the F-cost of this GridNode.
        /// </summary>
        /// <returns>The F-cost of this GridNode.</returns>
        public int GetFCost()
        {
            return fCost;
        }

        /// <summary>
        /// Calculates the F-cost of this GridNode.
        /// </summary>
        public void SetFCost()
        {
            fCost = hCost + gCost;
        }

        /// <summary>
        /// Determines whether this GridNode represents an empty location.
        /// </summary>
        /// <param name="empty">The representative value of an empty GridNode.</param>
        /// <returns>True if it is empty, else false.</returns>
        public bool GetEmpty(int empty)
        {
            if (empty == tile)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Returns a numerical representation of this GridNode.
        /// </summary>
        /// <returns>The representation of this GridNode.</returns>
        public int GetTile()
        {
            return tile;
        }

        /// <summary>
        /// The X-coordinate of this GridNode.
        /// </summary>
        public int X
        {
            get {
                return this.x;
            }
        }

        /// <summary>
        /// The Y-coordinate of this GridNode.
        /// </summary>
        public int Y
        {
            get
            {
                return this.y;
            }
        }


    }
}
