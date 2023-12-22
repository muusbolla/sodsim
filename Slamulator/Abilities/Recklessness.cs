using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class Recklessness : Ability
    {
        public double lastUsed = -180;
        public Recklessness(PlayerState p) : base(p)
        {
            ServerSideNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Do));
        }
        public override void Do()
        {
            double now = myPlayer.MyContext.Server.Time;
            lastUsed = now;
            myPlayer.GCDEnds = now + Globals.GCD;
            myPlayer.ThinkWhen(myPlayer.GCDEnds); //think after GCD
            //myPlayer.ThinkWhen(now + 1800); //think after ability cooldown
            myPlayer.Buffs.Recklessness.Start();
            if (myPlayer.logging) myPlayer.Log.Add("*RECKLESSNESS*");
            myPlayer.Think();
        }
        public override double AvailableWhen()
        {
            return lastUsed + 1800;
        }
    }
}
