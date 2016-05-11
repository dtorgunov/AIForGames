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

        private MightyPathFinder pathFinder;
        private LocationLocator locationLocator;

        public dtorguno()
            : base()
        {
            this.Name = "Subsumptive AI";
            this.localMap = null;
            this.dispatcher = new SubsumptionDispatch();

            dispatcher.add(new Tuple<SubsumptionDispatch.Situation, SubsumptionDispatch.Action>
                (unexploredExists, goToUnexplored));

            /*dispatcher.add(new Tuple<SubsumptionDispatch.Situation, SubsumptionDispatch.Action>
                (seenByEnemy, moveUp));
            
            dispatcher.add(new Tuple<SubsumptionDispatch.Situation, SubsumptionDispatch.Action>
                (singleEnemy, moveUp));*/

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
            this.pathFinder = new MightyPathFinder(localMap);
            // what does the ID stand for?
            this.locationLocator = new LocationLocator(localMap, worldState.MyGridSquare, this.ID);
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

        // Predicates (Situations)

        public bool unexploredExists()
        {
            for (int x = 0; x < localMap.GetLength(0); x++)
            {
                for (int y = 0; y < localMap.GetLength(1); y++)
                {
                    if (localMap[x, y] == Cell.Unexplored)
                    {
                        WriteTrace("Unexplored node exists!");
                        return true;
                    }
                }
            }
            return false;
        }

        public bool singleEnemy()
        {
            return enemyCount() == 1;
        }

        public bool seenByEnemy()
        {
            // consider iterating through ALL visible enemies, instead?
            GridSquare enemy = getClosestEnemy();
            if (enemy == null)
            {
                return false;
            }

            GridSquare[,] gs = fromLocalMap();
            int myX = worldState.MyGridSquare.X;
            int myY = worldState.MyGridSquare.Y;
            
            PlayerWorldState.Facing enemyFacing = getFacing(enemy);

            return worldState.CanSee(enemyFacing, enemy.X, enemy.Y, myX, myY, gs);
        }

        public bool enemiesExist()
        {
            return enemyCount() > 1;
        }

        public bool enemySighted()
        {
            GridSquare closestEnemy = getClosestEnemy();
            // closest enemy refers to the closest VISIBLE enemy
            return !(closestEnemy == null);
        }

        // Actions

        public ICommand moveUp()
        {
            return new Command(Command.Move.Up, true);
        }

        public ICommand moveToAPlace()
        {
            WriteTrace("Hey, look, I'm going to a place!");
            Tuple<int, int> aPlace = new Tuple<int, int>(0, 1);
            return explorationMove(aPlace);
        }

        public ICommand runAway()
        {
            GridSquare enemy = getClosestEnemy();
            Tuple<int, int> goHere = locationLocator.Retreat(enemy);
            return urgentMove(goHere);
        }

        public ICommand engageEnemy()
        {
            return locationLocator.Attack(getClosestEnemy(),
               getFacing(worldState.MyGridSquare), pathFinder);
        }

        public ICommand goToUnexplored()
        {
            Tuple<int, int> dest = locationLocator.UnexploredNode(worldState.MyGridSquare);
            WriteTrace("Moving to " + dest.Item1 + "," + dest.Item2);
            return explorationMove(dest);
        }

        public ICommand lookForTrouble()
        {
            List<Tuple<int,int>> Enemies = new List<Tuple<int, int>>();

            for (int x = 0; x < localMap.GetLength(0); x++)
            {
                for (int y = 0; y < localMap.GetLength(1); y++)
                {
                    // if the cell contaions is an enemy tank placve in enemy list
                    if (localMap[x, y] == Cell.Enemy1 || localMap[x, y] == Cell.Enemy2 || localMap[x, y] == Cell.Enemy3)
                    {
                        Enemies.Add(new Tuple<int, int>(x,y));
                    }

                }
                int cost = int.MaxValue;
                Tuple<int, int> ApproxClosest;
                Tuple<int, int> HeroTank = new Tuple<int, int>(worldState.MyGridSquare.X, worldState.MyGridSquare.Y);

                foreach (var tank in Enemies)
                {
                    if (squaredDistance(tank,HeroTank) < cost)
                    {
                        ApproxClosest = tank;
                    }
                }

                //pathFinder.GetPathToTarget(HeroTank);
            }


            return null;
        }

        

        // Helper methods

        public void updateMap()
        {
            GridSquare hero = worldState.MyGridSquare;
            playerNumber = hero.Player;
            List<GridSquare> visibleMap = worldState.MyVisibleSquares;

            localMap[hero.X, hero.Y] = Cell.Hero; // NEED TO CLEAR OLD LOCATION

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
            // make sure this is necessary
            //locationLocator.Update(localMap, hero, 0);
        }

        // turn, then move next turn (unless subsumpted to do otherwise)
        private Command explorationMove(Tuple<int, int> destination)
        {
            Command.Move expectedBearing = directionToMove(destination);
            PlayerWorldState.Facing bearing = worldState.MyFacing;

            if (equivalentBearing(bearing, expectedBearing))
            {
                return moveInDirection(destination);
            }
            else
            {
                return turnToFace(destination);
            }
        }

        // move without turning
        private Command urgentMove(Tuple<int, int> destination)
        {
            return moveInDirection(destination);
        }

        private Command moveInDirection(Tuple<int, int> destination)
        {
            return new Command(directionToMove(destination), false);
        }

        private Command turnToFace(Tuple<int, int> destination)
        {
            return new Command(directionToTurn(destination), false);
        }

        private bool equivalentBearing(PlayerWorldState.Facing bearing,
                                        Command.Move expectedBearing)
        {
            return (bearing == PlayerWorldState.Facing.Down 
                    && expectedBearing == Command.Move.Down)
                || (bearing == PlayerWorldState.Facing.Up 
                    && expectedBearing == Command.Move.Up)
                || (bearing == PlayerWorldState.Facing.Left 
                    && expectedBearing == Command.Move.Left)
                || (bearing == PlayerWorldState.Facing.Right 
                    && expectedBearing == Command.Move.Right);
        }

        private Command.Move directionToMove(Tuple<int, int> destination)
        {
            List<GridNode> path = pathFinder.GetPathToTarget(destination, worldState.MyGridSquare);
            GridSquare hero = worldState.MyGridSquare;

            GridNode nextMove = path.ElementAt(1);
            WriteTrace("Next move: " + nextMove.x + "," + nextMove.y);
            if (nextMove.x > hero.X)
            {
                return Command.Move.Right;
            }
            else if (nextMove.x < hero.X)
            {
                return Command.Move.Left;
            }
            else if (nextMove.y > hero.Y)
            {
                return Command.Move.Up;
            }
            else
            {
                return Command.Move.Down;
            }
        }

        private Command.Move directionToTurn(Tuple<int, int> destination)
        {
            Command.Move bearing = directionToMove(destination);

            if (bearing == Command.Move.Right)
            {
                return Command.Move.RotateRight;
            }
            else if (bearing == Command.Move.Left)
            {
                return Command.Move.RotateLeft;
            }
            else
            { // will need to turn twice
                return Command.Move.RotateLeft;
            }
        }

        // potentially inefficient, think of a better way?
        // also buggy
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

        private GridSquare convertFromCell(Cell c, int x, int y)
        {
            switch (c)
            {
                case Cell.Empty:
                    return new GridSquare(x, y, GridSquare.ContentType.Empty);
                case Cell.Destroyed:
                    return new GridSquare(x, y, GridSquare.ContentType.DestroyedTank);
                case Cell.Rock:
                    return new GridSquare(x, y, GridSquare.ContentType.Rock);
                case Cell.Hero:
                case Cell.Enemy1:
                case Cell.Enemy2:
                case Cell.Enemy3:
                    return unresolveTank(x, y);
                case Cell.Unexplored:
                    return new GridSquare(x, y); // assume "empty"
                default:
                    return null;
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

        private GridSquare unresolveTank(int x, int y)
        {
            List<GridSquare> visible = worldState.MyVisibleSquares;
            foreach (GridSquare square in visible)
            {
                if (square.X == x && square.Y == y)
                {
                    return square;
                }
            }

            // tanks only matter when we can see them
            return new GridSquare(x, y, GridSquare.ContentType.Empty);
        }

        private GridSquare getClosestEnemy()
        {
            List<GridSquare> visible = worldState.MyVisibleSquares;
            GridSquare enemySquare = null;
            Tuple<int, int> enemy = null;
            GridSquare hero = worldState.MyGridSquare;
            foreach (GridSquare square in visible)
            {
                if (!isTank(square))
                {
                    continue;
                }

                if (square.X == hero.X && square.Y == hero.Y)
                {
                    continue;
                }

                // this is a tank which is not the hero
                if (enemy == null)
                {
                    // first tank found
                    enemy = new Tuple<int, int>(square.X, square.Y);
                    enemySquare = square;
                }
                else
                {
                    // another tank exists, keep the closes one
                    Tuple<int, int> enemy2 = new Tuple<int, int>(square.X, square.Y);
                    Tuple<int, int> h = new Tuple<int, int>(hero.X, hero.Y);
                    enemy = closest(h, enemy, enemy2);
                    if (enemy.Equals(enemy2))
                    {
                        enemySquare = square;
                    }
                }
            }
            return enemySquare;
        }

        private Tuple<int, int> closest(Tuple<int, int> h, Tuple<int, int> p, Tuple<int, int> q)
        {
            // Use squared Euclidean distance, as it is good enough and quicker to compute
            if (squaredDistance(h, p) >= squaredDistance(h, q))
            {
                return p;
            }
            else
            {
                return q;
            }
        }

        private int squaredDistance(Tuple<int, int> p, Tuple<int, int> q)
        {
            int deltaX = q.Item1 - p.Item1;
            int deltaY = q.Item2 - p.Item2;
            return deltaX * deltaX + deltaY * deltaY;
        }

        private bool isTank(GridSquare s)
        {
            return (s.Contents == GridSquare.ContentType.TankDown)
                || (s.Contents == GridSquare.ContentType.TankUp)
                || (s.Contents == GridSquare.ContentType.TankLeft)
                || (s.Contents == GridSquare.ContentType.TankRight);
        }
        private GridSquare[,] fromLocalMap()
        {
            GridSquare[,] gs = new GridSquare[localMap.GetLength(0), localMap.GetLength(1)];
            for (int x = 0; x < localMap.GetLength(0); x++)
            {
                for (int y = 0; y < localMap.GetLength(1); y++)
                {
                    gs[x, y] = convertFromCell(localMap[x, y], x, y);
                }
            }

            return gs;
        }

        // assumes there is a tank there
        private PlayerWorldState.Facing getFacing(GridSquare tank)
        {
            switch (tank.Contents)
            {
                case GridSquare.ContentType.TankDown:
                    return PlayerWorldState.Facing.Down;
                case GridSquare.ContentType.TankLeft:
                    return PlayerWorldState.Facing.Left;
                case GridSquare.ContentType.TankRight:
                    return PlayerWorldState.Facing.Right;
                case GridSquare.ContentType.TankUp:
                    return PlayerWorldState.Facing.Up;
                default:
                    // shouldn't happen
                    return PlayerWorldState.Facing.Down;
            }
        }

        private int enemyCount()
        {
            int totalEnemyCount = worldState.PlayerCount - 1;
            int aliveEnemyCount = totalEnemyCount;
            for (int x = 0; x < localMap.GetLength(0); x++)
            {
                for (int y = 0; y < localMap.GetLength(1); y++)
                {
                    if (localMap[x, y] == Cell.Destroyed)
                    {
                        // subtract the enemies we know are destroyed
                        aliveEnemyCount--;
                    }
                }
            }

            return aliveEnemyCount;
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
