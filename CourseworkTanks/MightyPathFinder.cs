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
        GridNode Target;

     

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
                   
                }//End of Y 
            }// End of X 

        }

        private List<GridNode> GetNeighbours(GridNode node)
        {
            List<GridNode> neighbours = new List<GridNode>();

            for (int x = 0; x < InternalNodeMap.GetLength(0); x++)
            {
                for (int y = 0; y < InternalNodeMap.GetLength(1); y++)
                {
                    if (InternalNodeMap[x, y].Y == node.Y && InternalNodeMap[x, y].X == node.X)
                    {
                        if (x >= 1)
                            neighbours.Add(InternalNodeMap[x - 1, y]);
                        if (y >= 1)
                            neighbours.Add(InternalNodeMap[x, y - 1]);
                        if (x <= InternalNodeMap.GetLength(0) - 2)
                            neighbours.Add(InternalNodeMap[x + 1, y]);
                        if (y <= InternalNodeMap.GetLength(0) - 2)
                            neighbours.Add(InternalNodeMap[x, y + 1]);
                    }
                }
            }
            return neighbours;
        }

        int Heuristic(GridNode current, GridNode target)
        {
            int dx = Math.Abs(current.X - target.X);
            int dy = Math.Abs(current.Y - target.Y);

                return 10 * (dx + dy);
        }



        public List<GridNode> GetPathToTarget(Tuple<int, int> TupleNode){

            List<GridNode> open = new List<GridNode>();
            List<GridNode> closed = new List<GridNode>();

            Target = InternalNodeMap[TupleNode.Item1, TupleNode.Item2];

            open.Add(Hero);

            while (open.Count > 0)
            {

                GridNode current = open[0];

                for (int i = 1; i < open.Count; i++)
                {

                    if (open[i].GetFCost() < current.GetFCost() || open[i].GetFCost() == current.GetFCost() && open[i].GetHCost() < current.GetHCost())
                    {
                        current = open[i];
                    }

                 }

                open.Remove(current);
                closed.Add(current);

                if (current == Target)
                {
                    //found target

                    List<GridNode> path = new List<GridNode>();
                    GridNode retrace = Hero;

                    while (retrace != current && retrace != null)
                    {
                        if (retrace.GetParent() == null)
                             break;
                        path.Add(retrace);
                        retrace = retrace.GetParent();
                    }

                    return path;
                }

                 List<GridNode> neighbours = GetNeighbours(current);
                    foreach (GridNode n in neighbours)
                    {
                        if (n.GetWalkable() && !closed.Contains(n))
                        {
                            int newMovementCost = current.GetGCost() + Heuristic(current, n);
                            if (newMovementCost < n.GetGCost() || !open.Contains(n))
                            {
                                n.SetGCost(newMovementCost);
                                n.SetHCost(Heuristic(n, Target));
                                n.SetParent(current);
                                if (!open.Contains(n))
                                    open.Add(n);
                            }
                        }
                    }
                }


            
            
            return new List<GridNode>();

        }

    }
}
