using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridWorld
{
    class MightyPathFinder
    {
        GridNode[,] gridNodeBoardArray;
        PlayerWorldState myworld;

        enum tiles {Empty,Rock,Hero,Enemy,Rubble,Unexplored};

        public MightyPathFinder(PlayerWorldState p)
        {
            this.myworld = p;
        }

        public void ConvertToGridNodeArray(PlayerWorldState p)
        {



        }



        public List<GridNode> GetPathToTarget(GridNode Target){



            return new List<GridNode>();

        }

    }
}
