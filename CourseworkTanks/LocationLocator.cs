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
        private int id;

        /// <summary>
        /// Constructs the class.
        /// </summary>
        /// <param name="localMap">Map of the game environment.</param>
        /// <param name="hero">Location of the player.</param>
        public LocationLocator(Cell[,] localMap, GridSquare hero, int id)
        {
            Update(localMap, hero, id);
        }

        /// <summary>
        /// Updates the localMap
        /// </summary>
        /// <param name="localMap">Map of the game environment.</param>
        /// <param name="hero">Location of the player.</param>
        public void Update(Cell[,] localMap, GridSquare hero, int id)
        {
            this.localMap = localMap;
            this.hero = hero;
            this.id = id;
        }

        /// <summary>
        /// Gives a location to locate to.
        /// </summary>
        /// <param name="threat">The threat to retreat from.</param>
        /// <returns>A Tuple with the coordinates of the destination.</returns>
        public Tuple<int, int> Retreat(GridSquare threat)
        {
            int mapWidth = localMap.GetLength(0);
            int mapHeigth = localMap.GetLength(1);

            int searchSize = 1;
            List<GridNode> obs = MapObs(searchSize);

            while(obs.Count == 0) 
            {
                if (searchSize < 3)
                {
                    searchSize++;
                    obs = MapObs(searchSize);
                }
                else
                {
                    return RandomDodge(mapWidth, mapHeigth, threat);
                }
            }

            foreach (GridNode gn in obs)
            {
                if (gn.Y == hero.Y)
                {
                    if (gn.X < hero.X && gn.X > 0)
                        if (localMap[gn.X - 1, gn.Y] == Cell.Empty)
                            return new Tuple<int, int>(gn.X - 1, gn.Y);
                    if (gn.X > hero.X && gn.X < mapWidth)
                        if (localMap[gn.X + 1, gn.Y] == Cell.Empty)
                            return new Tuple<int, int>(gn.X + 1, gn.Y);
                }
                else if (gn.X == hero.X)
                {
                    if (gn.Y < hero.Y && gn.Y > 0)
                        if (localMap[gn.X, gn.Y - 1] == Cell.Empty)
                            return new Tuple<int, int>(gn.X, gn.Y - 1);
                    if (gn.Y > hero.Y && gn.Y < mapWidth)
                        if (localMap[gn.X, gn.Y + 1] == Cell.Empty)
                            return new Tuple<int, int>(gn.X, gn.Y + 1);
                }
                else if (gn.Y > hero.Y)
                {
                    if (gn.Y < mapWidth)
                        if (localMap[gn.X, gn.Y + 1] == Cell.Empty)
                            return new Tuple<int, int>(gn.X, gn.Y + 1);
                        else if (gn.X > hero.X)
                        {
                            if (gn.X < mapWidth)
                                if (localMap[gn.X + 1, gn.Y] == Cell.Empty)
                                    return new Tuple<int, int>(gn.X + 1, gn.Y);
                        }
                        else if (gn.X < hero.X)
                        {
                            if (gn.X > 0)
                                if (localMap[gn.X - 1, gn.Y] == Cell.Empty)
                                    return new Tuple<int, int>(gn.X - 1, gn.Y);
                        }
                }
                else if (gn.Y < hero.Y)
                {
                    if (gn.Y > 0)
                        if (localMap[gn.X, gn.Y - 1] == Cell.Empty)
                            return new Tuple<int, int>(gn.X, gn.Y + 1);
                        else if (gn.X > hero.X)
                        {
                            if (gn.X < mapWidth)
                                if (localMap[gn.X + 1, gn.Y] == Cell.Empty)
                                    return new Tuple<int, int>(gn.X + 1, gn.Y);
                        }
                        else if (gn.X < hero.X)
                        {
                            if (gn.X > 0)
                                if (localMap[gn.X - 1, gn.Y] == Cell.Empty)
                                    return new Tuple<int, int>(gn.X - 1, gn.Y);
                        }
                }
            }

            //Base case. Should never be reached.
            return RandomDodge(mapWidth, mapHeigth, threat);
        }

        /// <summary>
        /// Maps the obstacles surronding the player.
        /// </summary>
        /// <param name="size">How far away from the player to go.</param>
        /// <returns>A list of the nearby obstacles.</returns>
        private List<GridNode> MapObs(int size)
        {
            List<GridNode> obs = new List<GridNode>();

            //Assumes the player is not at the border of the map.
            for (int i = hero.X - size; i <= hero.X + size; i++)
            {
                for (int j = hero.Y - size; j <= hero.Y + size; j++)
                {
                    if(i < localMap.GetLength(0) && i >= 0 && j < localMap.GetLength(1) && j >= 0)
                        if (localMap[i, j] == Cell.Rock)
                            obs.Add(new GridNode(i, j, (int)Cell.Rock, id));
                }
            }
            return obs;
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
        /// Makes a decision how to attack.
        /// </summary>
        /// <param name="threat">The enemy to attack</param>
        /// <param name="facing">The direction the player is facing.</param>
        /// <param name="mpf">The pathfinder.</param>
        /// <returns>The issued command.</returns>
        public Command Attack(GridSquare threat, PlayerWorldState.Facing facing, MightyPathFinder mpf) 
        {
            if (hero.X == threat.X)
            {
                if (hero.Y > threat.Y)
                {
                    if (facing == PlayerWorldState.Facing.Down)
                        return new Command(Command.Move.Down, true);
                    else if (facing == PlayerWorldState.Facing.Right)
                        return new Command(Command.Move.RotateRight, true);
                    else if (facing == PlayerWorldState.Facing.Left)
                        return new Command(Command.Move.RotateLeft, true);
                    else
                        return new Command(Command.Move.RotateLeft, false);
                }
                else if (hero.Y < threat.Y)
                {
                    if (facing == PlayerWorldState.Facing.Up)
                        return new Command(Command.Move.Up, true);
                    else if (facing == PlayerWorldState.Facing.Right)
                        return new Command(Command.Move.RotateLeft, true);
                    else if (facing == PlayerWorldState.Facing.Left)
                        return new Command(Command.Move.RotateRight, true);
                    else
                        return new Command(Command.Move.RotateLeft, false);
                }
            }
            else if(hero.Y == threat.Y)
            {
                if (hero.X > threat.X)
                {
                    if (facing == PlayerWorldState.Facing.Left)
                        return new Command(Command.Move.Left, true);
                    else if (facing == PlayerWorldState.Facing.Down)
                        return new Command(Command.Move.RotateRight, true);
                    else if (facing == PlayerWorldState.Facing.Up)
                        return new Command(Command.Move.RotateLeft, true);
                    else
                        return new Command(Command.Move.RotateLeft, false);
                }
                else if (hero.X < threat.X)
                {
                    if (facing == PlayerWorldState.Facing.Right)
                        return new Command(Command.Move.Up, true);
                    else if (facing == PlayerWorldState.Facing.Down)
                        return new Command(Command.Move.RotateLeft, true);
                    else if (facing == PlayerWorldState.Facing.Up)
                        return new Command(Command.Move.RotateRight, true);
                    else
                        return new Command(Command.Move.RotateLeft, false);
                }
            }

            GridNode travel = mpf.GetPathToTarget(new Tuple<int, int>(threat.X, threat.Y)).ElementAt(0);

            if (travel.X > hero.X)
                return new Command(Command.Move.Right, false);
            else if (travel.X < hero.X)
                return new Command(Command.Move.Left, false);
            else if (travel.Y > hero.Y)
                return new Command(Command.Move.Up, false);
            else
                return new Command(Command.Move.Down, false);
            
        }

        /// <summary>
        /// Gives a random known node to path to.
        /// </summary>
        /// <param name="w">The width of the board.</param>
        /// <param name="h">The height of the board.</param>
        /// <returns>A Tuple with the coordinates of the destination.</returns>
        public Tuple<int, int> RandomKnown(int w, int h)
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

        /// <summary>
        /// Gives a random unknown node to path to.
        /// </summary>
        /// <param name="w">The width of the board.</param>
        /// <param name="h">The height of the board.</param>
        /// <returns>A Tuple with the coordinates of the destination.</returns>
        public Tuple<int, int> RandomUnknown(int w, int h)
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
            } while (c != Cell.Unexplored);

            return new Tuple<int, int>(x, y);
        }

    }
}
