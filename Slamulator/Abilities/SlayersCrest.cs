using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class SlayersCrest : Ability
    {
        public double lastUsed = -120;
        public SlayersCrest(PlayerState p) : base(p)
        {
            ServerSideNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Do));
        }
        public override void Do()
        {
            double now = myPlayer.MyContext.Server.Time;
            lastUsed = now;
            //myPlayer.ThinkWhen(now); //trinket doesn't cause cooldown, think again
            myPlayer.Buffs.SlayersCrest.Start();
            if (myPlayer.logging) myPlayer.Log.Add("*SLAYER'S CREST*");
            myPlayer.Think();
        }
        public override double AvailableWhen()
        {
            return lastUsed + 120;
        }
    }
}
