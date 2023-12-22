using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class WindfuryTotemHitMH : Ability
    {
        public WindfuryTotemHitMH(PlayerState p) : base(p)
        {
            //ServerSideNode = new LinkedListNode<Pair>(new Pair(0.0, Do));
        }
        public override void Do()
        {
            Outcome oc = RollWhiteHitMH();
            double dmg = RollWhiteDamageMH(oc); //TODO add windfury totem bonus AP
            dmg = myPlayer.ArmorMitigate(dmg);
            myPlayer.RageGain(dmg);
            myPlayer.totalDamage += dmg;
            if (myPlayer.logging) myPlayer.Log.Add(String.Format("HJ{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, myPlayer.MyContext.Server.Time, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
            //Extra attacks don't reset the swing timer or remove flurry charges - see Ironfoe Mechanics At End.avi
            //TODO confirm this is true on Elysium / for Windfury Totem
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
        public override double RollWhiteDamageMH(Outcome oc)
        {
            double dmg = 0;
            double dmgMin = myPlayer.mhWpnDmgMin + 22.5 * myPlayer.mhSpeedBase; //22.5 DPS = 315 bonus AP from WF totem / 14
            double dmgMax = myPlayer.mhWpnDmgMax + 22.5 * myPlayer.mhSpeedBase;
            switch (oc)
            {
                case Outcome.Crit:
                    dmg = RollDamage(dmgMin, dmgMax, 2);
                    myPlayer.Buffs.Flurry.Start();
                    myPlayer.RollProcsMH(false);
                    break;
                case Outcome.Hit:
                    dmg = RollDamage(dmgMin, dmgMax, 1);
                    myPlayer.RollProcsMH(false);
                    break;
                case Outcome.Dodge:
                    break;
                case Outcome.Glance:
                    double glancePercent = myPlayer.MyContext.RollRange(myPlayer.mhGlanceMin, myPlayer.mhGlanceMax);
                    dmg = RollDamage(dmgMin, dmgMax, glancePercent);
                    myPlayer.RollProcsMH(false);
                    break;
                case Outcome.Miss:
                    break;
                default:
                    break;
            }
            return dmg;
        }
    }
}
