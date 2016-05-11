﻿using System;
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

        public MightyPathFinder(Cell[,] ICM)
        {
            this.InternalCellMap = ICM;
            ConvertToGridNodeArray(InternalCellMap);
            
        }

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

        private List<GridNode> GetNeighbours(GridNode node)
        {

            // to account for impassable cells, we say that an unwalkable cell has no neighbours
            // therefore it will never be part of a path, other than as the goal
            if (!node.walkable)
            {
                return new List<GridNode>();
            }

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
                    neighbours.Add(InternalNodeMap[coor.Item1, coor.Item2]);
                }
            }
            return neighbours;
        }

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

        private int heuristic(GridNode node, GridNode goal)
        {
            int dx = Math.Abs(node.x - goal.x);
            int dy = Math.Abs(node.y - goal.y);

            return dx + dy;
        }


        public List<GridNode> GetPathToTarget(Tuple<int, int> TupleNode){

            ConvertToGridNodeArray(InternalCellMap);

            List<GridNode> open = new List<GridNode>();
            List<GridNode> closed = new List<GridNode>();

            GridNode Target = InternalNodeMap[TupleNode.Item1, TupleNode.Item2];

            open.Add(Hero);

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
                    List<GridNode> properPath = new List<GridNode>();
                    foreach (var node in path)
                    {
                        properPath.Add(node);
                    }

                    return properPath;
                }

                 List<GridNode> neighbours = GetNeighbours(current);
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
