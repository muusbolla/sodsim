using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class DMCMaelstrom : Ability
    {
        public DMCMaelstrom(PlayerState p) : base(p)
        {
            //ServerSideNode = new LinkedListNode<Pair>(new Pair(0.0, Do));
        }
        public override void Do()
        {
            double dmg = myPlayer.MyContext.RollRange(200, 300); //TODO implement resist?
            myPlayer.totalDamage += dmg;
            if (myPlayer.logging) myPlayer.Log.Add(String.Format("DMC{0:F2} | {1:F2} | {2:F2}", dmg, myPlayer.rage, myPlayer.MyContext.Server.Time));
        }
    }
}
