using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridWorld
{
    public enum Cell { Empty, Rock, Hero, Enemy1, Enemy2, Enemy3, Destroyed, Unexplored };
    public class dtorguno : BasePlayer
    {
        private PlayerWorldState worldState;
        private Cell[,] localMap;
        private int playerNumber;
        private SubsumptionDispatch dispatcher;

        public dtorguno()
            : base()
        {
            this.Name = "Subsumptive AI";
            this.localMap = null;
            this.dispatcher = new SubsumptionDispatch();

            dispatcher.add(new Tuple<SubsumptionDispatch.Situation, SubsumptionDispatch.Action>
                (SubsumptionDispatch.defaultAction, moveUp));
        }

        private void initMap() {
            localMap = new Cell[worldState.GridWidthInSquares, 
                                worldState.GridHeightInSquares];

            for (int x = 0; x < worldState.GridWidthInSquares; x++)
            {
                for (int y = 0; y < worldState.GridHeightInSquares; y++)
                {
                    // Simply initialise an empty map. It will be updated
                    // on the first pass
                    localMap[x, y] = Cell.Unexplored;
                }
            }
        }

        public override ICommand GetTurnCommands(IPlayerWorldState igrid)
        {
            worldState = (PlayerWorldState) igrid;

            if (localMap == null)
            {
                initMap();
            }

            updateMap();

            return dispatcher.act();
        }

        public ICommand moveUp()
        {
            return new Command(Command.Move.Up, true);
        }

        public void updateMap()
        {
            GridSquare hero = worldState.MyGridSquare;
            playerNumber = hero.Player;
            List<GridSquare> visibleMap = worldState.MyVisibleSquares;

            localMap[hero.X, hero.Y] = Cell.Hero;

            Tuple<int, int> e1 = new Tuple<int, int>(-1, -1);
            Tuple<int, int> e2 = new Tuple<int, int>(-1, -1);
            Tuple<int, int> e3 = new Tuple<int, int>(-1, -1);
            
            foreach (GridSquare square in visibleMap)
            {
                if (hero.X == square.X && hero.Y == square.Y)
                {
                    // already set
                    continue;
                }

               localMap[square.X, square.Y] = convertToCell(square);

               if (localMap[square.X, square.Y] == Cell.Enemy1)
               {
                   e1 = new Tuple<int, int>(square.X, square.Y);
               }
               else if (localMap[square.X, square.Y] == Cell.Enemy2)
               {
                   e2 = new Tuple<int, int>(square.X, square.Y);
               }
               else if (localMap[square.X, square.Y] == Cell.Enemy3)
               {
                   e3 = new Tuple<int, int>(square.X, square.Y);
               }
            }

            cleanEnemies(e1, e2, e3);
        }

        // potentially inefficient, think of a better way?
        private void cleanEnemies(Tuple<int, int> e1, Tuple<int, int> e2, Tuple<int, int> e3)
        {
            for (int x = 0; x < worldState.GridWidthInSquares; x++)
            {
                for (int y = 0; y < worldState.GridHeightInSquares; y++)
                {
                    if (e1.Item1 != -1)
                    {
                        if (localMap[x, y] == Cell.Enemy1
                            && e1.Item1 != x && e1.Item2 != y)
                        {
                            localMap[x, y] = Cell.Empty;
                        }
                    }

                    if (e2.Item1 != -1)
                    {
                        if (localMap[x, y] == Cell.Enemy2
                            && e2.Item1 != x && e2.Item2 != y)
                        {
                            localMap[x, y] = Cell.Empty;
                        }
                    }

                    if (e3.Item1 != -1)
                    {
                        if (localMap[x, y] == Cell.Enemy3
                            && e3.Item1 != x && e3.Item2 != y)
                        {
                            localMap[x, y] = Cell.Empty;
                        }
                    }

                }
            }
        }

        private Cell convertToCell(GridSquare s)
        {
            switch (s.Contents)
            {
                case GridSquare.ContentType.Empty:
                    return Cell.Empty;
                case GridSquare.ContentType.Rock:
                    return Cell.Rock;
                case GridSquare.ContentType.DestroyedTank:
                    return Cell.Destroyed;
                case GridSquare.ContentType.TankDown:
                case GridSquare.ContentType.TankLeft:
                case GridSquare.ContentType.TankRight:
                case GridSquare.ContentType.TankUp:
                    return resolveTank(s);
                default:
                    return Cell.Unexplored;
            }
        }

        private Cell resolveTank(GridSquare s)
        {
            // assuming it's not us, or we wouldn't be here

            // MUST be a better way to do this, surely!
            if (playerNumber == 1 || playerNumber == 4)
            {
                if (s.Player == 2)
                {
                    return Cell.Enemy1;
                }
                else if (s.Player == 3)
                {
                    return Cell.Enemy2;
                }
                else
                {
                    return Cell.Enemy3;
                }
            } else if (playerNumber == 2)
            {
                if (s.Player == 1)
                {
                    return Cell.Enemy1;
                }
                else if (s.Player == 3)
                {
                    return Cell.Enemy2;
                }
                else
                {
                    return Cell.Enemy3;
                }
            } else  // (playerNumber == 3)
            {
                if (s.Player == 2)
                {
                    return Cell.Enemy1;
                }
                else if (s.Player == 1)
                {
                    return Cell.Enemy2;
                }
                else
                {
                    return Cell.Enemy3;
                }
            }            
        }

        class SubsumptionDispatch
        {
            public delegate bool Situation();
            public delegate ICommand Action();

            // the FIRST action in the table has HIGHEST priority
            private List<Tuple<Situation, Action>> dispatchTable;

            public SubsumptionDispatch()
            {
                this.dispatchTable = new List<Tuple<Situation, Action>>();
            }

            public SubsumptionDispatch(List<Tuple<Situation, Action>> dispatchTable)
            {
                this.dispatchTable = dispatchTable;
            }

            // adds as the LOWEST PRIORITY action
            public void add(Tuple<Situation, Action> entry)
            {
                dispatchTable.Add(entry);
            }

            public static bool defaultAction()
            {
                return true;
            }

            public ICommand act()
            {
                foreach (Tuple<Situation, Action> behaviour in dispatchTable)
                {
                    if (behaviour.Item1())
                    {
                        return behaviour.Item2();
                    }
                }

                // isn't reachable if the default action is given
                return null;
            }
        }
    }
}
