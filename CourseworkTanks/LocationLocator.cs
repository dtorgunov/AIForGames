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


        public Tuple<int, int> Retreat(GridSquare threat)
        {
            int mapWidth = localMap.GetLength(0);
            int mapHeigth = localMap.GetLength(1);

            List<Cell> obs = new List<Cell>();
            //Assumes the player is not at the border of the map.
            for (int i = hero.X - 1; i <= hero.X + 1; i++)
            {
                for (int j = hero.Y - 1; j <= hero.Y + 1; j++)
                {

                }
            }

            //Base case. Should never be reached.
            return RandomDodge(mapWidth, mapHeigth, threat);
        }

        /// <summary>
        /// Dodges randomly when no hiding place can be found.
        /// </summary>
        /// <param name="w">The width of the board.</param>
        /// <param name="h">The height of the board.</param>
        /// <param name="t">The target to escape from.</param>
        /// <returns>A Tuple with the coordinates of the destination.</returns>
        private Tuple<int, int> RandomDodge(int w, int h, GridSquare t)
        {
            Random r = new Random();
            
            int xDiff;
            if(hero.X > t.X)
                xDiff = Math.Abs(hero.X - t.X);
            else
                xDiff = Math.Abs(t.X - hero.X);
            
            int yDiff;
            if (hero.Y > t.Y)
                yDiff = Math.Abs(hero.Y - t.Y);
            else
                yDiff = Math.Abs(t.Y - hero.Y);

            if (xDiff < yDiff) //Closer on the x-axis, dodge vertically
            {
                if (localMap[hero.X, hero.Y + 1] != Cell.Rock && localMap[hero.X, hero.Y - 1] != Cell.Rock)
                {
                    if (r.Next(2) == 0)
                        return new Tuple<int, int>(hero.X, hero.Y + 1);
                    else
                        return new Tuple<int, int>(hero.X, hero.Y - 1);
                }
                if (localMap[hero.X, hero.Y + 1] == Cell.Rock && localMap[hero.X, hero.Y - 1] != Cell.Rock)
                    return new Tuple<int, int>(hero.X, hero.Y - 1);
                if (localMap[hero.X, hero.Y - 1] == Cell.Rock && localMap[hero.X, hero.Y + 1] != Cell.Rock)
                    return new Tuple<int, int>(hero.X, hero.Y + 1);
            }
            else //Closer on the y-axis
            {
                if (localMap[hero.X + 1, hero.Y] != Cell.Rock && localMap[hero.X - 1, hero.Y] != Cell.Rock)
                {
                    if (r.Next(2) == 0)
                        return new Tuple<int, int>(hero.X + 1, hero.Y);
                    else
                        return new Tuple<int, int>(hero.X - 1, hero.Y);
                }
                if (localMap[hero.X + 1, hero.Y] == Cell.Rock && localMap[hero.X - 1, hero.Y] != Cell.Rock)
                    return new Tuple<int, int>(hero.X, hero.Y - 1);
                if (localMap[hero.X - 1, hero.Y] == Cell.Rock && localMap[hero.X + 1, hero.Y] != Cell.Rock)
                    return new Tuple<int, int>(hero.X, hero.Y + 1);
            }

            //Base Case - used if no easy dodges are avalible to the AI, chooses a location to pathfind to.
            return RandomKnown(w, h);
        }

        /// <summary>
        /// Gives a random known node to path to.
        /// </summary>
        /// <param name="w">The width of the board.</param>
        /// <param name="h">The height of the board.</param>
        /// <returns>A Tuple with the coordinates of the destination.</returns>
        private Tuple<int, int> RandomKnown(int w, int h)
        {
            Random r = new Random();
            Cell c;
            int x;
            int y;
            do
            {
                x = r.Next(w);
                y = r.Next(h);
                c = localMap[w, h];
            } while (c != Cell.Empty);

            return new Tuple<int, int>(x, y);
        }

    }
}
