﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class HOJHitMH : Ability
    {
        public HOJHitMH(PlayerState p) : base(p)
        {
            //ServerSideNode = new LinkedListNode<Pair>(new Pair(0.0, Do));
        }
        public override void Do()
        {
            Outcome oc = RollWhiteHitMH();
            double dmg = RollWhiteDamageMH(oc);
            dmg = myPlayer.ArmorMitigate(dmg);
            myPlayer.RageGain(dmg);
            myPlayer.totalDamage += dmg;
            if (myPlayer.logging) myPlayer.Log.Add(String.Format("HJ{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, myPlayer.MyContext.Server.Time, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
            //Extra attacks don't reset the swing timer or remove flurry charges ON KRONOS - see Ironfoe Mechanics At End.avi
            //TODO confirm this for Anathema
            switch (oc)
            {
                case Outcome.Crit:
                    myPlayer.Buffs.Flurry.Start();
                    myPlayer.RollProcsMH(false);
                    break;
                case Outcome.Hit:
                    myPlayer.RollProcsMH(false);
                    break;
                case Outcome.Glance:
                    myPlayer.RollProcsMH(false);
                    break;
                case Outcome.Dodge:
                    myPlayer.LastDodge = myPlayer.MyContext.Server.Time;
                    break;
                case Outcome.Miss:
                    break;
                default:
                    break;
            }
        }
    }
}
