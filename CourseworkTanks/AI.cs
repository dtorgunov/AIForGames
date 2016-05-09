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
    }
}
