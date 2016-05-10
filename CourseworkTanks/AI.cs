using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridWorld
{
    public class dtorguno : BasePlayer
    {
        private PlayerWorldState worldState;

        public dtorguno()
            : base()
        {
            this.Name = "Subsumptive AI";
        }

        public override ICommand GetTurnCommands(IPlayerWorldState igrid)
        {
            return new Command(Command.Move.Up, true);
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
