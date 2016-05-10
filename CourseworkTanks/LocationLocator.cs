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
        /// <param name="hero">Location of the player.</param>
        public LocationLocator(Cell[,] localMap, GridSquare hero)
        {
            Update(localMap, hero);
        }

        /// <summary>
        /// Updates the localMap
        /// </summary>
        /// <param name="localMap">Map of the game environment.</param>
        /// <param name="hero">Location of the player.</param>
        public void Update(Cell[,] localMap, GridSquare hero)
        {
            this.localMap = localMap;
            this.hero = hero;
        }


        public Tuple<int, int> Retreat()
        {
            int hX = hero.X;
            int hY = hero.Y;

            
            //Base case. Should never be reached.
            return new Tuple<int,int>(0, 0);
        }

    }
}
