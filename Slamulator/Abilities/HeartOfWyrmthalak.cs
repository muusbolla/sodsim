using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class HeartOfWyrmthalak : Ability
    {
        public HeartOfWyrmthalak(PlayerState p) : base(p)
        {
            //ServerSideNode = new LinkedListNode<Pair>(new Pair(0.0, Do));
        }
        public override void Do()
        {
            double dmg = myPlayer.MyContext.RollRange(120, 180); //TODO implement resist?
            myPlayer.totalDamage += dmg;
            if (myPlayer.logging) myPlayer.Log.Add(String.Format("HOW{0:F2} | {1:F2} | {2:F2}", dmg, myPlayer.rage, myPlayer.MyContext.Server.Time));
        }
    }
}
