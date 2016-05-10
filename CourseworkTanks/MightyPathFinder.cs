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
     

        public MightyPathFinder(PlayerWorldState PWS, Cell[,] ICM)
        {
            this.InternalPlayerWorldState = PWS;
            this.InternalCellMap = ICM;

            
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



        public List<GridNode> GetPathToTarget(Tuple<int, int> TupleNode){



            
            
            return new List<GridNode>();

        }

    }
}
