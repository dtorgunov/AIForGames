using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridWorld
{
    class LocationLocator
    {

        private Cell[,] localMap;
        private GridSquare hero;

        /// <summary>
        /// Constructs the class.
        /// </summary>
        /// <param name="localMap">Map of the game environment.</param>
        public LocationLocator(Cell[,] localMap, GridSquare hero)
        {
            this.localMap = localMap;
            this.hero = hero;
        }

        /// <summary>
        /// Updates the localMap
        /// </summary>
        /// <param name="localMap">Map of the game environment.</param>
        public void Update(Cell[,] localMap, GridSquare hero)
        {
            this.localMap = localMap;
            this.hero = hero;
        }

        public /*WhateverConnorNeeds*/ Retreat()
        {
            
        }

    }
}
