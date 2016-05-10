using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridWorld
{
    class LocationLocator
    {

        private Cell[,] localMap;

        /// <summary>
        /// Constructs the class.
        /// </summary>
        /// <param name="localMap">Map of the game environment.</param>
        public LocationLocator(Cell[,] localMap)
        {
            this.localMap = localMap;
        }

        /// <summary>
        /// Updates the localMap
        /// </summary>
        /// <param name="localMap">Map of the game environment.</param>
        public void Update(Cell[,] localMap)
        {
            this.localMap = localMap;
        }

        public Cell Retreat()
        {
            
        }

    }
}
