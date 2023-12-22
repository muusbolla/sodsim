using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class Earthstrike : Ability
    {
        public double lastUsed = -120;
        public Earthstrike(PlayerState p) : base(p)
        {
            ServerSideNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Do));
        }
        public override void Do()
        {
            double now = myPlayer.MyContext.Server.Time;
            lastUsed = now;
            //myPlayer.ThinkWhen(now); //trinket doesn't cause cooldown, think again
            myPlayer.Buffs.Earthstrike.Start();
            if (myPlayer.logging) myPlayer.Log.Add("*EARTHSTRIKE*");
            myPlayer.Think();
        }
        public override double AvailableWhen()
        {
            return lastUsed + 120;
        }
    }
}
