﻿using System;
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
        /// <param name="id">The ID of the player.</param>
        public LocationLocator(Cell[,] localMap, GridSquare hero, int id)
        {
            Update(localMap, hero, id);
        }

        /// <summary>
        /// Updates the localMap
        /// </summary>
        /// <param name="localMap">Map of the game environment.</param>
        /// <param name="hero">Location of the player.</param>
        /// <param name="id">The ID of the player.</param>
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
                    return RandomDodge(mapWidth, mapHeigth, threat);
            }

            foreach (GridNode gn in obs)
            {
                if (gn.y == hero.Y && SimpleDodgeX(gn.x, gn.y, hero.X).Item1 != -1)
                    return SimpleDodgeX(gn.x, gn.y, hero.X);
                else if (gn.x == hero.X && SimpleDodgeY(gn.y, gn.x, hero.Y).Item1 != -1)
                    return SimpleDodgeY(gn.y, gn.x, hero.Y);
                else if (gn.y > hero.Y)
                {
                    if (gn.y < mapWidth)
                        if (localMap[gn.x, gn.y + 1] == Cell.Empty)
                            return new Tuple<int, int>(gn.x, gn.y + 1);
                        else if (AdvancedDodge(gn).Item1 != -1)
                            return AdvancedDodge(gn);
                }
                else if (gn.y < hero.Y)
                {
                    if (gn.y > 0)
                        if (localMap[gn.x, gn.y - 1] == Cell.Empty)
                            return new Tuple<int, int>(gn.x, gn.y + 1);
                        else if (AdvancedDodge(gn).Item1 != -1)
                            return AdvancedDodge(gn);
                }
            }

            return RandomDodge(mapWidth, mapHeigth, threat);
        }

        /// <summary>
        /// Dodges behind a rock in one of the two cardinal X directions.
        /// </summary>
        /// <param name="gnMain">The axis shared with the Hero.</param>
        /// <param name="gnSec">The axis not shared with the Hero.</param>
        /// <param name="hMain">The Hero coord on the shared axis.</param>
        /// <returns>A Tuple to move to. Will contain -1, -1 if not valid.</returns>
        private Tuple<int, int> SimpleDodgeX(int gnMain, int gnSec, int hMain)
        {
            if (gnMain < hMain && gnMain > 0)
                if (localMap[gnMain - 1, gnSec] == Cell.Empty)
                    return new Tuple<int, int>(gnMain - 1, gnSec);
            if (gnMain > hMain && gnMain < localMap.GetLength(0))
                if (localMap[gnMain + 1, gnSec] == Cell.Empty)
                    return new Tuple<int, int>(gnMain + 1, gnSec);

            return new Tuple<int, int>(-1, -1);
        }

        /// <summary>
        /// Dodges behind a rock in one of the two cardinal Y directions.
        /// </summary>
        /// <param name="gnMain">The axis shared with the Hero.</param>
        /// <param name="gnSec">The axis not shared with the Hero.</param>
        /// <param name="hMain">The Hero coord on the shared axis.</param>
        /// <returns>A Tuple to move to. Will contain -1, -1 if not valid.</returns>
        private Tuple<int, int> SimpleDodgeY(int gnMain, int gnSec, int hMain)
        {
            if (gnMain < hMain && gnMain > 0)
                if (localMap[gnSec, gnMain - 1] == Cell.Empty)
                    return new Tuple<int, int>(gnSec, gnMain - 1);
            if (gnMain > hMain && gnMain < localMap.GetLength(1))
                if (localMap[gnSec, gnMain + 1] == Cell.Empty)
                    return new Tuple<int, int>(gnSec, gnMain + 1);

            return new Tuple<int, int>(-1, -1);
        }

        /// <summary>
        /// Checks if cover either above or below the player can be hidden behind on the X-axis.
        /// </summary>
        /// <param name="gn">The GridNode to hide behind.</param>
        /// <returns>A Tuple to move to. Will contain -1, -1 if not valid.</returns>
        private Tuple<int, int> AdvancedDodge(GridNode gn)
        {
            if (gn.x > hero.X)
            {
                if (gn.x < localMap.GetLength(0))
                    if (localMap[gn.x + 1, gn.y] == Cell.Empty)
                        return new Tuple<int, int>(gn.x + 1, gn.y);
            }
            else if (gn.x < hero.X)
            {
                if (gn.x > 0)
                    if (localMap[gn.x - 1, gn.y] == Cell.Empty)
                        return new Tuple<int, int>(gn.x - 1, gn.y);
            }

            return new Tuple<int, int>(-1, -1);
        }

        /// <summary>
        /// Maps the obstacles surronding the player.
        /// </summary>
        /// <param name="size">How far away from the player to go.</param>
        /// <returns>A list of the nearby obstacles.</returns>
        private List<GridNode> MapObs(int size)
        {
            List<GridNode> obs = new List<GridNode>();

            for (int i = hero.X - size; i <= hero.X + size; i++)
            {
                for (int j = hero.Y - size; j <= hero.Y + size; j++)
                {
                    if(IsValidCoordinate(new Tuple<int, int>(i, j)))
                        if (localMap[i, j] == Cell.Rock)
                            obs.Add(new GridNode(i, j, Cell.Rock));
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

            if (xDiff < yDiff)          //Closer on the x-axis, dodge vertically
                return Dodge(0, 1);
            else                        //Closer on the y-axis, dodge horizontally
                return Dodge(1, 0);
        }

        /// <summary>
        /// Determines where to dodge.
        /// </summary>
        /// <param name="xAdd">How far to look in the X-direction.</param>
        /// <param name="yAdd">How far to look in the Y-direction.</param>
        /// <returns>A Tuple with the coordinates of the destination.</returns>
        private Tuple<int, int> Dodge(int xAdd, int yAdd)
        {
            Random r = new Random();
            if (localMap[hero.X + xAdd, hero.Y + yAdd] != Cell.Rock && localMap[hero.X - xAdd, hero.Y - yAdd] != Cell.Rock)
            {
                if (r.Next(2) == 0)
                    return new Tuple<int, int>(hero.X + xAdd, hero.Y + yAdd);
                else
                    return new Tuple<int, int>(hero.X - xAdd, hero.Y - yAdd);
            }
            if (localMap[hero.X + xAdd, hero.Y + yAdd] == Cell.Rock && localMap[hero.X - xAdd, hero.Y - yAdd] != Cell.Rock)
                return new Tuple<int, int>(hero.X - xAdd, hero.Y - yAdd);
            if (localMap[hero.X, hero.Y - yAdd] == Cell.Rock && localMap[hero.X + xAdd, hero.Y + yAdd] != Cell.Rock)
                return new Tuple<int, int>(hero.X + xAdd, hero.Y + yAdd);

            return RandomKnown(localMap.GetLength(0), localMap.GetLength(0));
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

            if (travel.x > hero.X)
                return new Command(Command.Move.Right, false);
            else if (travel.x < hero.X)
                return new Command(Command.Move.Left, false);
            else if (travel.y > hero.Y)
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

        /// <summary>
        /// Find a random unexplored node that is reachable (i.e. 'pathfindable' from our
        /// current position).
        /// </summary>
        /// <param name="hero">The player's position</param>
        /// <returns>The coordinates of a reachable unexplored node</returns>
        public Tuple<int, int> UnexploredNode(GridSquare hero)
        {
            List<Tuple<int, int>> checkedList = new List<Tuple<int, int>>();
            List<Tuple<int, int>> unexplored = new List<Tuple<int, int>>();
            Stack<Tuple<int, int>> toCheck = new Stack<Tuple<int, int>>();

            Tuple<int, int> start = new Tuple<int, int>(hero.X, hero.Y);
            toCheck.Push(start);
            while (toCheck.Count != 0)
            {
                Tuple<int, int> current = toCheck.Pop();

                if (checkedList.Contains(current))
                {
                    continue;
                }

                if (unexplored.Contains(current))
                {
                    continue;
                }

                if (localMap[current.Item1, current.Item2] != Cell.Unexplored)
                {
                    checkedList.Add(current);
                }
                else
                {
                    unexplored.Add(current);  
                }

                List<Tuple<int, int>> neightbours = GetNodeNeighbours(current);

                foreach (Tuple<int, int> cell in neightbours)
                {
                    if (!(checkedList.Contains(cell))
                        && !(unexplored.Contains(cell)))
                    {
                        toCheck.Push(cell);
                    }
                }
            }

            if (unexplored.Count == 0)
            {
                return null;
            }

            Random r = new Random();
            return unexplored.ElementAt(r.Next(unexplored.Count));
        }

        /// <summary>
        /// Find all the neightbours of a given node, IF that node is not unexplored
        /// or a rock. (This method needs refining!)
        /// </summary>
        /// <param name="node">Coodrinates of a node to start from</param>
        /// <returns>All neighbours of a non-unexplored node</returns>
        private List<Tuple<int, int>> GetNodeNeighbours(Tuple<int, int> node)
        {
            List<Tuple<int, int>> neighbours = new List<Tuple<int, int>>();

            if (localMap[node.Item1, node.Item2] != Cell.Unexplored
             && localMap[node.Item1, node.Item2] != Cell.Rock)
            {
                Stack<Tuple<int, int>> potentialNeighbours
                    = new Stack<Tuple<int, int>>();

                int x = node.Item1;
                int y = node.Item2;

                potentialNeighbours.Push(new Tuple<int, int>(x+1, y));
                potentialNeighbours.Push(new Tuple<int, int>(x-1, y));
                potentialNeighbours.Push(new Tuple<int, int>(x, y+1));
                potentialNeighbours.Push(new Tuple<int, int>(x, y-1));

                foreach (Tuple<int, int> coor in potentialNeighbours)
                {
                    if (IsValidCoordinate(coor))
                    {
                        neighbours.Add(coor);
                    }
                }
            }

            return neighbours;
        }

        /// <summary>
        /// Check if a coordinate is within the bounds of the current map
        /// </summary>
        /// <param name="coor">Coordinate to be checked</param>
        /// <returns>True if coordinate is a valid index into the map array</returns>
        private bool IsValidCoordinate(Tuple<int, int> coor)
        {
            if (coor.Item1 < 0 || coor.Item2 < 0)
            {
                return false;
            }

            if (coor.Item1 > localMap.GetLength(0) - 1
              || coor.Item2 > localMap.GetLength(1) - 1)
            {
                return false;
            }

            return true;
        }
    }
}
