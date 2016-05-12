using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridWorld
{
    public enum Cell { Empty, Rock, Hero, Enemy1, Enemy2, Enemy3, Destroyed, Unexplored };
    public class dtorguno : BasePlayer
    {
        /// <summary>
        /// The last known state of the world.
        /// </summary>
        private PlayerWorldState worldState;
        /// <summary>
        /// The local map, keeping track of cells seen and last
        /// known enemy locations.
        /// </summary>
        private Cell[,] localMap;
        /// <summary>
        /// Which player are we?
        /// </summary>
        private int playerNumber;
        /// <summary>
        /// The subsumption dispatcher responsible for
        /// taking the right action in the right situation.
        /// </summary>
        private SubsumptionDispatch dispatcher;

        /// <summary>
        /// The pathfinder module.
        /// </summary>
        private MightyPathFinder pathFinder;
        /// <summary>
        /// The Actions module.
        /// </summary>
        private LocationLocator locationLocator;

        public dtorguno()
            : base()
        {
            this.Name = "Subsumptive AI";
            this.localMap = null;
            this.dispatcher = chooseBehaviour();
        }

        /// <summary>
        /// Initialise the map on the first turn.
        /// </summary>
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
            this.locationLocator = new LocationLocator(localMap, this.ID);
        }

        /// <summary>
        /// Randomy choose a behaviour profile.
        /// </summary>
        /// <returns>The subsumption dispatcher corresponding to the chosen profile.</returns>
        private SubsumptionDispatch chooseBehaviour()
        {
            Random r = new Random();
            int choise = r.Next(1);
            if (choise == 0)
            {
                WriteTrace("Playing hunter");
                return hunter();
            }
            else
            {
                WriteTrace("Playing explorer");
                return explorer();
            }
        }

        /// <summary>
        /// The main game loop.
        /// </summary>
        /// <param name="igrid">The latest world state.</param>
        /// <returns>A command to execute.</returns>
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

        // Behavious Profiles

        /// <summary>
        /// The Explorer behaviour profile. Prioratises discovering new squares to confronting enemies,
        /// allowing them to fight amongst themselves instead. Should be used when destroying enemies
        /// is less valuable (in terms of points) than exploring the map.
        /// </summary>
        /// <returns>A subsumption dispatcher corresponding to the Explorer profile.</returns>
        private SubsumptionDispatch explorer()
        {
            SubsumptionDispatch dispatcher = new SubsumptionDispatch();

            dispatcher.add(new Tuple<SubsumptionDispatch.Situation, SubsumptionDispatch.Action>
                (lastEnemySighted, engageEnemy));
            dispatcher.add(new Tuple<SubsumptionDispatch.Situation, SubsumptionDispatch.Action>
                (seenByEnemy, runAway));
            dispatcher.add(new Tuple<SubsumptionDispatch.Situation, SubsumptionDispatch.Action>
                (unexploredExists, goToUnexplored));
            dispatcher.add(new Tuple<SubsumptionDispatch.Situation, SubsumptionDispatch.Action>
                (singleEnemy, seekEnemy));
            dispatcher.add(new Tuple<SubsumptionDispatch.Situation, SubsumptionDispatch.Action>
                (SubsumptionDispatch.defaultAction, stay));
            
            return dispatcher;
        }

        /// <summary>
        /// The Hunter behaviour profile. Prioratises destroying enemies to exploring the map.
        /// Should be used when the gain in points for killing an enemy is larger than point gain
        /// for exploration, in order to stop the enemies from getting those points. Risk if the
        /// reward is high enough.
        /// </summary>
        /// <returns>A subsumption dispatcher corresponding to the Hunter profile.</returns>
        private SubsumptionDispatch hunter()
        {
            SubsumptionDispatch dispatcher = new SubsumptionDispatch();

            dispatcher.add(new Tuple<SubsumptionDispatch.Situation, SubsumptionDispatch.Action>
                (enemySighted, engageEnemy));
            dispatcher.add(new Tuple<SubsumptionDispatch.Situation, SubsumptionDispatch.Action>
                (enemiesExist, seekEnemy));
            dispatcher.add(new Tuple<SubsumptionDispatch.Situation, SubsumptionDispatch.Action>
                (unexploredExists, goToUnexplored));
            dispatcher.add(new Tuple<SubsumptionDispatch.Situation, SubsumptionDispatch.Action>
                (SubsumptionDispatch.defaultAction, stay));

            return dispatcher;
        }

        // Predicates (Situations)

        /// <summary>
        /// Check if there are unexplored cells on the local map.
        /// </summary>
        /// <returns>True if there is at least one unexplored cell, false otherwise.</returns>
        public bool unexploredExists()
        {
            for (int x = 0; x < localMap.GetLength(0); x++)
            {
                for (int y = 0; y < localMap.GetLength(1); y++)
                {
                    if (localMap[x, y] == Cell.Unexplored)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Check if there is only one living enemy.
        /// </summary>
        /// <returns>True if we know exactly one enemy is still alive, false otherwise.</returns>
        public bool singleEnemy()
        {
            return enemyCount() == 1;
        }

        /// <summary>
        /// Check if we are visible to an enemy tank.
        /// </summary>
        /// <returns>True if we can confirm an enemy can see us, false otherwise.</returns>
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

        /// <summary>
        /// Checks if there is at least one enemy still alive.
        /// </summary>
        /// <returns>True if we know there is more than one living enemy, false otherwise.</returns>
        public bool enemiesExist()
        {
            return enemyCount() > 1;
        }

        /// <summary>
        /// There is (at least one) visible enemy.
        /// </summary>
        /// <returns>True if we can see at least one enemy tank, false otherwise.</returns>
        public bool enemySighted()
        {
            GridSquare closestEnemy = getClosestEnemy();
            // closest enemy refers to the closest VISIBLE enemy
            return !(closestEnemy == null);
        }

        /// <summary>
        /// We can see (what we know to be) the last enemy tank.
        /// </summary>
        /// <returns>True if the last surviving enemy is visible, false otherwise.</returns>
        public bool lastEnemySighted()
        {
            GridSquare closestEnemy = getClosestEnemy();
            return (!(closestEnemy == null)) && (enemyCount() == 1);
        }

        // Actions

        /// <summary>
        /// Stay still.
        /// </summary>
        /// <returns>The command to execute.</returns>
        public ICommand stay()
        {
            return new Command(Command.Move.Stay, false);
        }

        /// <summary>
        /// Run for cover.
        /// </summary>
        /// <returns>The command to execute.</returns>
        public ICommand runAway()
        {
            GridSquare enemy = getClosestEnemy();
            Tuple<int, int> goHere = locationLocator.Retreat(enemy, worldState.MyGridSquare);
            return urgentMove(goHere);
        }

        /// <summary>
        /// Attempt to attack a visible enemy.
        /// </summary>
        /// <returns>The command to execute.</returns>
        public ICommand engageEnemy()
        {
            return locationLocator.Attack(getClosestEnemy(),
               getFacing(worldState.MyGridSquare), pathFinder, worldState.MyGridSquare);
        }

        /// <summary>
        /// Head in the direction of an unexplored node, using pathfinding to
        /// determine the direction to move in.
        /// </summary>
        /// <returns>The command to execute.</returns>
        public ICommand goToUnexplored()
        {
            Tuple<int, int> dest = locationLocator.UnexploredNode(worldState.MyGridSquare);
            return explorationMove(dest);
        }

        /// <summary>
        /// Move to the last known enemy location, or to a random reachable (explored or not)
        /// cell, hoping to find an enemy.
        /// </summary>
        /// <returns>The command to execute.</returns>
        public ICommand seekEnemy()
        {
            List<Tuple<int, int>> Enemies = new List<Tuple<int, int>>();

            for (int x = 0; x < localMap.GetLength(0); x++)
            {
                for (int y = 0; y < localMap.GetLength(1); y++)
                {

                    if (Enemies.Count == enemyCount())
                    {
                        break;
                    }

                    // if the cell contaions is an enemy tank place in enemy list
                    if (localMap[x, y] == Cell.Enemy1 || localMap[x, y] == Cell.Enemy2 || localMap[x, y] == Cell.Enemy3)
                    {
                        Enemies.Add(new Tuple<int, int>(x, y));
                    }
                }
            }
            
            int cost = int.MaxValue;
            Tuple<int, int> ApproxClosest = null;
            Tuple<int, int> HeroTank = new Tuple<int, int>(worldState.MyGridSquare.X, worldState.MyGridSquare.Y);

            foreach (var tank in Enemies)
            {
                if (squaredDistance(tank, HeroTank) < cost)
                {
                    ApproxClosest = tank;
                    cost = squaredDistance(tank, HeroTank);
                }
            }
            
            if (ApproxClosest != null)
            {
                return explorationMove(ApproxClosest);
            }

            // no known enemy locations, move to a random reachable cell
            return explorationMove(locationLocator.RandomReachable(worldState.MyGridSquare));
        }

        

        // Helper methods

        /// <summary>
        /// Update the internal map as necessary, based on the information from 
        /// currently visible cells.
        /// </summary>
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

            cleanTanks(e1, e2, e3);
         }

        /// <summary>
        /// A movement command that tries to maximise the area explored
        /// by turning in the direction of movement before executing a
        /// move.
        /// </summary>
        /// <param name="destination">A location we're trying to move towards.</param>
        /// <returns>The command to execute.</returns>
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

        /// <summary>
        /// A movement command mainly used for dodging, that forgoes
        /// turning to get out of the enemy's line of fire quickly.
        /// </summary>
        /// <param name="destination">A location we're moving towards.</param>
        /// <returns>The command to execute.</returns>
        private Command urgentMove(Tuple<int, int> destination)
        {
            return moveInDirection(destination);
        }

        /// <summary>
        /// Move in the direction of a given coordinate.
        /// </summary>
        /// <param name="destination">Coordinate to move towards.</param>
        /// <returns>The command to execute.</returns>
        private Command moveInDirection(Tuple<int, int> destination)
        {
            return new Command(directionToMove(destination), false);
        }

        /// <summary>
        /// Turn to face a specific node. Note that if the note is behind
        /// us, the command will need to be executed twice, as no 180 turn
        /// is available.
        /// </summary>
        /// <param name="destination">Coordinate to face.</param>
        /// <returns>The command to execute.</returns>
        private Command turnToFace(Tuple<int, int> destination)
        {
            return new Command(directionToTurn(destination), false);
        }

        /// <summary>
        /// Check whether we are facing the direction we're about to move
        /// towards.
        /// </summary>
        /// <param name="bearing">The direction we're facing.</param>
        /// <param name="expectedBearing">The direction we're about to move in.</param>
        /// <returns>True if the two coincide, false otherwise.</returns>
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

        /// <summary>
        /// The direction we need to move in in order to get closer to a given
        /// coordinate.
        /// </summary>
        /// <param name="destination">The coordinates of the node to move towards.</param>
        /// <returns>The direction of movement.</returns>
        private Command.Move directionToMove(Tuple<int, int> destination)
        {
            List<GridNode> path = pathFinder.GetPathToTarget(destination, worldState.MyGridSquare);
            GridSquare hero = worldState.MyGridSquare;

            GridNode nextMove = path.ElementAt(1);
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

        /// <summary>
        /// The direction we need to turn in in order to face a given coordinate.
        /// </summary>
        /// <param name="destination">The coordinates of the node to face.</param>
        /// <returns>The direction to turn in.</returns>
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

        /// <summary>
        /// Clean duplicate enemies off the map, if the enemy has been sighted again. Also make sure there is only one hero
        /// tank present.
        /// </summary>
        /// <param name="e1">The current known coordinates of the first enemy (or (-1, -1) if not see this turn).</param>
        /// <param name="e2">The current known coordinates of the second enemy (or (-1, -1) if not see this turn).</param>
        /// <param name="e3">The current known coordinates of the third enemy (or (-1, -1) if not see this turn).</param>
        private void cleanTanks(Tuple<int, int> e1, Tuple<int, int> e2, Tuple<int, int> e3)
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

                    if (localMap[x, y] == Cell.Hero)
                    {
                        if (worldState.MyGridSquare.X != x || worldState.MyGridSquare.Y != y)
                        {
                            localMap[x, y] = Cell.Empty;
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Convert a grid square to a corresponding cell.
        /// </summary>
        /// <param name="s">The grid square to convert.</param>
        /// <returns>The equivalent cell.</returns>
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

        /// <summary>
        /// Undo the transformation done by convertToCell
        /// </summary>
        /// <param name="c">The cell to convert.</param>
        /// <param name="x">Its x coordinate.</param>
        /// <param name="y">Its y coordinate.</param>
        /// <returns>The equivalent GridSquare.</returns>
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

        /// <summary>
        /// Convert a GridSquare representing an enemy tank to
        /// the right Enemy Cell.
        /// </summary>
        /// <param name="s">A GridSquare representing an enemy.</param>
        /// <returns>The Cell corresponding to the Enemy (does not take bearing into account, but differs per player ID)</returns>
        private Cell resolveTank(GridSquare s)
        {
            // assuming it's not us, or we wouldn't be here
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

        /// <summary>
        /// Undo the transformation done by resolveTank. Only works on tanks we can see
        /// (or we wouldn't know the direction they are facing).
        /// </summary>
        /// <param name="x">The x coordinate of the tank.</param>
        /// <param name="y">The y coordinate of the tank.</param>
        /// <returns>The equivalent GridSquare.</returns>
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

        /// <summary>
        /// Find the closest enemy tank that we can see.
        /// </summary>
        /// <returns>The closest visible enemy tank, or null if no enemy tanks are visible.</returns>
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

        /// <summary>
        /// Determine which of the two coordinate pairs is closest to a given point.
        /// </summary>
        /// <param name="h">Reference point.</param>
        /// <param name="p">First coordinate pair.</param>
        /// <param name="q">Second coordinate pair.</param>
        /// <returns>p or q, depending on which is closest to h.</returns>
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

        /// <summary>
        /// Compute the squared Euclidean distance between two points.
        /// </summary>
        /// <param name="p">The first point.</param>
        /// <param name="q">The second point.</param>
        /// <returns>The squared Euclidean distance between p and q.</returns>
        private int squaredDistance(Tuple<int, int> p, Tuple<int, int> q)
        {
            int deltaX = q.Item1 - p.Item1;
            int deltaY = q.Item2 - p.Item2;
            return deltaX * deltaX + deltaY * deltaY;
        }

        /// <summary>
        /// Check whether a GridSquare is occupied by a tank.
        /// </summary>
        /// <param name="s">The GridSquare to consider.</param>
        /// <returns>True if s is a tank, false otherwise.</returns>
        private bool isTank(GridSquare s)
        {
            return (s.Contents == GridSquare.ContentType.TankDown)
                || (s.Contents == GridSquare.ContentType.TankUp)
                || (s.Contents == GridSquare.ContentType.TankLeft)
                || (s.Contents == GridSquare.ContentType.TankRight);
        }

        /// <summary>
        /// Undo the conversion to localMap done by updateMap(). Note that there is some loss
        /// of information: namely, the tanks that are not visible are not taken into account.
        /// </summary>
        /// <returns>A (semi)equivalent GridSquare map, corresponding to our internal map.</returns>
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

        /// <summary>
        /// Determine the facing of a tank.
        /// </summary>
        /// <param name="tank">A GridSquare containing a tank.</param>
        /// <returns>The direction that tank is facing.</returns>
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

        /// <summary>
        /// The amount of known enemies. Only destroyed tanks that have been observed
        /// are subtracted from this count.
        /// </summary>
        /// <returns>The (estimated) number of enemies alive, based on map data.</returns>
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
