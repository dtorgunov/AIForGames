using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridWorld
{
    class MightyPathFinder
    {
        GridNode[,] InternalNodeMap;
        PlayerWorldState InternalPlayerWorldState;
        Cell[,] InternalCellMap;
        GridNode Hero;
        GridNode Target;

     

        public MightyPathFinder(PlayerWorldState PWS, Cell[,] ICM)
        {
            this.InternalPlayerWorldState = PWS;
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
                        InternalNodeMap[x, y] = new GridNode(x, y, InternalCellMap[x, y]);

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

            

            
            
            return new List<GridNode>();

        }

    }
}
