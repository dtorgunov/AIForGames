using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridWorld
{
    class LocationLocator
    {

        private Cell[,] localMap;
        private int id;

        private Tuple<int, int> unexplored;

        /// <summary>
        /// Constructs the class.
        /// </summary>
        /// <param name="localMap">Map of the game environment.</param>
        /// <param name="id">The ID of the player.</param>
        public LocationLocator(Cell[,] localMap,  int id)
        {
            this.unexplored = null;
            Update(localMap, id);
        }

        /// <summary>
        /// Updates the localMap
        /// </summary>
        /// <param name="localMap">Map of the game environment.</param>
        /// <param name="id">The ID of the player.</param>
        public void Update(Cell[,] localMap,  int id)
        {
            this.localMap = localMap;
            this.id = id;
        }

        /// <summary>
        /// Gives a location to locate to.
        /// </summary>
        /// <param name="threat">The threat to retreat from.</param>
        /// <param name="hero">The player</param>
        /// <returns>A Tuple with the coordinates of the destination.</returns>
        public Tuple<int, int> Retreat(GridSquare threat, GridSquare hero)
        {
            int mapWidth = localMap.GetLength(0);
            int mapHeigth = localMap.GetLength(1);

            int searchSize = 1;
            List<GridNode> obs = MapObs(searchSize, hero);

            while(obs.Count == 0) 
            {
                if (searchSize < 3)
                {
                    searchSize++;
                    obs = MapObs(searchSize, hero);
                }
                else
                    return RandomDodge(mapWidth, mapHeigth, threat, hero);
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
                        else if (AdvancedDodge(gn, hero).Item1 != -1)
                            return AdvancedDodge(gn, hero);
                }
                else if (gn.y < hero.Y)
                {
                    if (gn.y > 0)
                        if (localMap[gn.x, gn.y - 1] == Cell.Empty)
                            return new Tuple<int, int>(gn.x, gn.y + 1);
                        else if (AdvancedDodge(gn, hero).Item1 != -1)
                            return AdvancedDodge(gn, hero);
                }
            }

            return RandomDodge(mapWidth, mapHeigth, threat, hero);
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
        /// <param name="hero">The player location</param>
        /// <returns>A Tuple to move to. Will contain -1, -1 if not valid.</returns>
        private Tuple<int, int> AdvancedDodge(GridNode gn, GridSquare hero)
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
        /// <param name="hero">The player location</param>
        /// <returns>A list of the nearby obstacles.</returns>
        private List<GridNode> MapObs(int size, GridSquare hero)
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
        /// <param name="hero">The player location</param>
        /// <returns>A Tuple with the coordinates of the destination.</returns>
        private Tuple<int, int> RandomDodge(int w, int h, GridSquare t, GridSquare hero)
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
                return Dodge(0, 1, hero);
            else                        //Closer on the y-axis, dodge horizontally
                return Dodge(1, 0, hero);
        }

        /// <summary>
        /// Determines where to dodge.
        /// </summary>
        /// <param name="xAdd">How far to look in the X-direction.</param>
        /// <param name="yAdd">How far to look in the Y-direction.</param>
        /// <param name="hero">The player location.</param>
        /// <returns>A Tuple with the coordinates of the destination.</returns>
        private Tuple<int, int> Dodge(int xAdd, int yAdd, GridSquare hero)
        {
            Random r = new Random();
            if (localMap[hero.X + xAdd, hero.Y + yAdd] != Cell.Rock && localMap[hero.X - xAdd, hero.Y - yAdd] != Cell.Rock && localMap[hero.X + xAdd, hero.Y + yAdd] != Cell.Destroyed && localMap[hero.X - xAdd, hero.Y - yAdd] != Cell.Destroyed)
            {
                if (r.Next(2) == 0)
                    return new Tuple<int, int>(hero.X + xAdd, hero.Y + yAdd);
                else
                    return new Tuple<int, int>(hero.X - xAdd, hero.Y - yAdd);
            }
            if (localMap[hero.X - xAdd, hero.Y - yAdd] != Cell.Rock && localMap[hero.X - xAdd, hero.Y - yAdd] != Cell.Destroyed)
                return new Tuple<int, int>(hero.X - xAdd, hero.Y - yAdd);
            if (localMap[hero.X + xAdd, hero.Y + yAdd] != Cell.Rock && localMap[hero.X + xAdd, hero.Y + yAdd] != Cell.Destroyed)
                return new Tuple<int, int>(hero.X + xAdd, hero.Y + yAdd);

            return RandomKnown(localMap.GetLength(0), localMap.GetLength(1));
        }

        /// <summary>
        /// Makes a decision how to attack.
        /// </summary>
        /// <param name="threat">The enemy to attack</param>
        /// <param name="facing">The direction the player is facing.</param>
        /// <param name="toEnemy">The pathfinder.</param>
        /// <param name="hero">The player location</param>
        /// <returns>The issued command.</returns>
        public Command Attack(GridSquare threat, PlayerWorldState.Facing facing, Command toEnemy, GridSquare hero) 
        {
            if (hero.X == threat.X)
            {
                if (hero.Y > threat.Y)
                    return AttackFacing(facing, PlayerWorldState.Facing.Down, PlayerWorldState.Facing.Right, PlayerWorldState.Facing.Left);
                else if (hero.Y < threat.Y)
                    return AttackFacing(facing, PlayerWorldState.Facing.Up, PlayerWorldState.Facing.Left, PlayerWorldState.Facing.Right);
            }
            else if(hero.Y == threat.Y)
            {
                if (hero.X > threat.X)
                    return AttackFacing(facing, PlayerWorldState.Facing.Left, PlayerWorldState.Facing.Down, PlayerWorldState.Facing.Up);
                else if (hero.X < threat.X)
                    return AttackFacing(facing, PlayerWorldState.Facing.Right, PlayerWorldState.Facing.Up, PlayerWorldState.Facing.Down);
            }

            return toEnemy;
            
        }

        /// <summary>
        /// Attacks if the player is facing an opponent.
        /// </summary>
        /// <param name="facing">The direction the player is facing.</param>
        /// <param name="noRotate">The direction to face when facing the enemy.</param>
        /// <param name="rotateRight">The direction to face when one rotateRight away from the enemy.</param>
        /// <param name="rotateLeft">The direction to face when one rotateLeft away from the enemy.</param>
        /// <returns>A command to execute.</returns>
        private Command AttackFacing(PlayerWorldState.Facing facing, PlayerWorldState.Facing noRotate, PlayerWorldState.Facing rotateRight, PlayerWorldState.Facing rotateLeft)
        {
            if (facing == noRotate)
                return new Command(Command.Move.Down, true);
            else if (facing == rotateRight)
                return new Command(Command.Move.RotateRight, true);
            else if (facing == rotateLeft)
                return new Command(Command.Move.RotateLeft, true);
            else
                return new Command(Command.Move.RotateLeft, false);
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

        public Tuple<int, int> RandomReachable(GridSquare hero)
        {
            List<Tuple<int, int>> reachableCells = ReachableUnexplored(hero);

            for (int x = 0; x < localMap.GetLength(0); x++)
            {
                for (int y = 0; y < localMap.GetLength(1); y++)
                {
                    if ((localMap[x, y] == Cell.Empty || localMap[x, y] == Cell.Hero) // to account for possibly not cleaning Hero right
                        && (x != hero.X && y != hero.Y))
                    {
                        reachableCells.Add(new Tuple<int, int>(x, y));
                    }
                }
            }

            Random r = new Random();
            return reachableCells.ElementAt(r.Next(reachableCells.Count));
        }

        public Tuple<int, int> UnexploredNode(GridSquare hero)
        {
            if (unexplored == null)
            {
                SetUnexploredNode(hero); // potential for infinite loop if called with no unexplored nodes remaining
            }

            if (localMap[unexplored.Item1, unexplored.Item2] != Cell.Unexplored)
            {
                SetUnexploredNode(hero);
            }

            return unexplored;
        }

        /// <summary>
        /// Find a random unexplored node that is reachable (i.e. 'pathfindable' from our
        /// current position) and set it as the current "unexplored" target.
        /// </summary>
        /// <param name="hero">The player's position</param>
        private void SetUnexploredNode(GridSquare hero)
        {
            List<Tuple<int, int>> unexplored = ReachableUnexplored(hero);

            if (unexplored.Count == 0)
            {
                this.unexplored = null;
            }

            Random r = new Random();
            this.unexplored = unexplored.ElementAt(r.Next(unexplored.Count));
        }

        private List<Tuple<int, int>> ReachableUnexplored(GridSquare hero)
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

            return unexplored;
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

            //if (localMap[node.Item1, node.Item2] != Cell.Unexplored
            // && localMap[node.Item1, node.Item2] != Cell.Rock)
            if(!potentialWall(node))
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
        /// Determine whether a block is likely to belong to a "wall" that we can't pathfind around.
        /// </summary>
        /// <param name="coord">Coodrinates of a node to check</param>
        /// <returns>True if coord is a potential element in the middle of a wall, false if it is a passable node or possibly a wall "edge".</returns>
        private bool potentialWall(Tuple<int, int> coord)
        {
            if (passable(coord))
            {
                return false;
            }

            Stack<Tuple<int, int>> leftRight = new Stack<Tuple<int, int>>();
            List<Tuple<int, int>> leftRightValid = new List<Tuple<int, int>>();
            Stack<Tuple<int, int>> upDown = new Stack<Tuple<int, int>>();
            List<Tuple<int, int>> upDownValid = new List<Tuple<int, int>>();

            int x = coord.Item1;
            int y = coord.Item2;

            leftRight.Push(new Tuple<int, int>(x - 1, y));
            leftRight.Push(new Tuple<int, int>(x + 1, y));
            upDown.Push(new Tuple<int, int>(x, y + 1));
            upDown.Push(new Tuple<int, int>(x, y - 1));

            bool result = true;

            foreach (var coor in leftRight)
            {
                if (IsValidCoordinate(coor))
                {
                    result &= nonObsticle(coor);
                }
            }

            if (result)
            {
                return true;
            }

            result = true;

            foreach (Tuple<int, int> coor in upDown)
            {
                if (IsValidCoordinate(coor))
                {
                    result &= nonObsticle(coor);
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if a given node is traversable in terms of pathfinding
        /// </summary>
        /// <param name="coord">Coordinates of a node to check</param>
        /// <returns>True if pathfinding considers the node passable, false otherwise.</returns>
        private bool passable(Tuple<int, int> coord)
        {
            return (localMap[coord.Item1, coord.Item2] == Cell.Empty)
                || (localMap[coord.Item1, coord.Item2] == Cell.Hero);
        }

        /// <summary>
        /// Check if a map is not an obsticle. Since this is used for exploring the map, we consider Unexplored
        /// cells to not be obsticles. Please keep in mind that this should ONLY be used when analyzing a cell
        /// as belonging to a wall or not. A rock with an unexplored cell above it is considered to be at the
        /// "edge", since we don't know if the Unexplored cell is passable or not. However, neighbours of such
        /// cells should not be generated, as that would lead to the whole map being considered reachable.
        /// </summary>
        /// <param name="coord">Coordinates of a node to check</param>
        /// <returns>See method description</returns>
        private bool nonObsticle(Tuple<int, int> coord)
        {
            return passable(coord) || (localMap[coord.Item1, coord.Item2] == Cell.Unexplored);
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
