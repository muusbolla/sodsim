using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class BerserkerStance : Ability
    {
        public BerserkerStance(PlayerState p) : base(p)
        {
            ServerSideNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Do));
        }
        public override void Do()
        {
            double now = myPlayer.MyContext.Server.Time;
            myPlayer.rage = Math.Min(myPlayer.rage, 25); //TODO implement tactical mastery?
            myPlayer.stanceGCDEnds = now + Globals.GCD;
            myPlayer.ThinkWhen(myPlayer.stanceGCDEnds); //think after GCD
            myPlayer.CurrentStance = Stance.Berserker;
            myPlayer.UpdateStats();
            myPlayer.Think();
        }
        public override double AvailableWhen()
        {
            return myPlayer.stanceGCDEnds;
        }
    }
}
