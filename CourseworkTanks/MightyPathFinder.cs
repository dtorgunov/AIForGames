using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridWorld
{
    class MightyPathFinder
    {
        GridNode[,] InternalNodeMap;
        Cell[,] InternalCellMap;
        GridNode Hero;


        /// <summary>
        /// Construct class and initialise internal map representatives.
        /// </summary>
        /// <param name="ICM">Local Cell Map.</param>
        public MightyPathFinder(Cell[,] ICM)
        {
            this.InternalCellMap = ICM;
            ConvertToGridNodeArray(InternalCellMap);
            
        }

       
        /// <summary>
        /// Convert from Cell[,] to GridNode[,] and intialises internal node map.
        /// </summary>
        /// <param name="ICM">Local Cell Map.</param>
        public void ConvertToGridNodeArray(Cell[,] ICM)
        {
            InternalNodeMap = new GridNode[InternalCellMap.GetLength(0),InternalCellMap.GetLength(1)];

            for (int x = 0; x < InternalCellMap.GetLength(0); x++)
            {
                for (int y = 0; y < InternalCellMap.GetLength(1); y++)
                {
                    InternalNodeMap[x, y] = new GridNode(x, y, InternalCellMap[x, y]);

                    if (InternalCellMap[x, y] == Cell.Hero)
                    {
                        Hero = InternalNodeMap[x, y];
                    }
                }
            }
        }

        /// <summary>
        /// Return the horizontal and vertical neighbouring nodes of a given node. 
        /// </summary>
        /// <param name="node"> The current node.</param>
        /// <param name="target">The target node.</param>
        /// <returns>Return a list of all found neigbours(if any).</returns>

        private List<GridNode> GetNeighbours(GridNode node, GridNode target)
        {

            List<GridNode> neighbours = new List<GridNode>();

            Stack<Tuple<int, int>> potentialNeighbours
                    = new Stack<Tuple<int, int>>();

            int x = node.x;
            int y = node.y;

            potentialNeighbours.Push(new Tuple<int, int>(x - 1, y));
            potentialNeighbours.Push(new Tuple<int, int>(x + 1, y));
            potentialNeighbours.Push(new Tuple<int, int>(x, y - 1));
            potentialNeighbours.Push(new Tuple<int, int>(x, y + 1));

            foreach (Tuple<int, int> coor in potentialNeighbours)
            {
                if (IsValidCoordinate(coor))
                {
                    GridNode n = InternalNodeMap[coor.Item1, coor.Item2];
                    if (n.walkable || n.Equals(target))
                    {
                        neighbours.Add(n);
                    }
                }
            }
            return neighbours;
        }
        /// <summary>
        /// Check if an unexplored node is traversable.
        /// </summary>
        /// <param name="node"> Node to be checked.</param>
        /// <returns>'True' if Traversable, 'False' otherwise.</returns>

        private bool reachableUnexplored(GridNode node)
        {
            if (node.cell == Cell.Unexplored)
            {
                Stack<Tuple<int, int>> potentialNeighbours
                   = new Stack<Tuple<int, int>>();

                int x = node.x;
                int y = node.y;

                potentialNeighbours.Push(new Tuple<int, int>(x - 1, y));
                potentialNeighbours.Push(new Tuple<int, int>(x + 1, y));
                potentialNeighbours.Push(new Tuple<int, int>(x, y - 1));
                potentialNeighbours.Push(new Tuple<int, int>(x, y + 1));

                foreach (Tuple<int, int> coor in potentialNeighbours)
                {
                    if (IsValidCoordinate(coor))
                    {
                        GridNode n = InternalNodeMap[coor.Item1, coor.Item2];
                        if (n.walkable)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check if vector position is contained on the map.
        /// </summary>
        /// <param name="coor">Position.</param>
        /// <returns></returns>

        private bool IsValidCoordinate(Tuple<int, int> coor)
        {
            if (coor.Item1 < 0 || coor.Item2 < 0)
            {
                return false;
            }

            if (coor.Item1 > InternalNodeMap.GetLength(0) - 1
              || coor.Item2 > InternalNodeMap.GetLength(1) - 1)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Return Matthatten heuristic value of cost for moving from one space to an adjacent space.
        /// </summary>
        /// <param name="node">The Current Node</param>
        /// <param name="goal">The Target Node</param>
        /// <returns>Computed cost</returns>
        private int heuristic(GridNode node, GridNode goal)
        {
            int dx = Math.Abs(node.x - goal.x);
            int dy = Math.Abs(node.y - goal.y);

            return dx + dy;
        }
        /// <summary>
        /// Gets the shortest path between two given node positions. 
        /// </summary>
        /// <param name="TupleNode">Target node position</param>
        /// <param name="heroPos"> Starting node position</param>
        /// <returns> List of path nodes</returns>

        public List<GridNode> GetPathToTarget(Tuple<int, int> TupleNode, GridSquare heroPos){

            ConvertToGridNodeArray(InternalCellMap);

            List<GridNode> open = new List<GridNode>();
            List<GridNode> closed = new List<GridNode>();

            GridNode Target = InternalNodeMap[TupleNode.Item1, TupleNode.Item2];
            GridNode heroNode = new GridNode(heroPos.X, heroPos.Y, Cell.Hero);

            open.Add(heroNode);

            while (open.Count > 0)
            {

                open = open.OrderBy(n => n.fCost).ToList(); // treat as priority queue
                GridNode current = open[0];
                open.Remove(current);
                closed.Add(current);

                if (current == Target)
                {
                    List<GridNode> path = new List<GridNode>();

                    while (current != null)
                    {
                        path.Add(current);
                        current = current.parent;
                    }

                    // reverse the path
                    path.Reverse();

                    return path;
                }

                 List<GridNode> neighbours = GetNeighbours(current, Target);
                    foreach (var n in neighbours)
                    {
                        int cost = current.gCost + 1; // assume movement cost is always 1 (even terrain)

                        // .Contains() should be fine, as we're getting all nodes from the array above,
                        // so the node at the same position should have the same address. If this causes
                        // issues, override .Equals()
                        if (open.Contains(n) && cost < n.gCost)
                        {
                            open.Remove(n);
                        }

                        if (!open.Contains(n) && !closed.Contains(n)) {
                            n.gCost = cost;
                            n.hCost = heuristic(n, Target);
                            n.parent = current;
                            open.Add(n);
                        }
                    }
                }
            
            // no path available
            return new List<GridNode>();
        }
    }
}
