using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class Execute : Ability
    {
        public double lastUsed = -1;
        public Execute(PlayerState p) : base(p)
        {
            ServerSideNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Do));
        }
        public override void Do()
        {
            double now = myPlayer.MyContext.Server.Time;
            lastUsed = now;
            myPlayer.GCDEnds = now + Globals.GCD;
            myPlayer.ThinkWhen(myPlayer.GCDEnds); //think after GCD
            Outcome oc = RollYellow();
            double dmg = 0;
            switch (oc)
            {
                case Outcome.Crit:
                    dmg = (600 + (myPlayer.rage - 10) * 15) * myPlayer.damageMultiplier * 2.2; //TODO implement Improved Execute talent
                    myPlayer.rage = 0;
                    dmg = myPlayer.ArmorMitigate(dmg);
                    myPlayer.totalDamage += dmg;
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("EX{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    myPlayer.Buffs.Flurry.Start();
                    myPlayer.RollProcsMH(true);
                    break;
                case Outcome.Hit:
                    dmg = (600 + (myPlayer.rage - 10) * 15) * myPlayer.damageMultiplier; //TODO implement Improved Execute talent
                    myPlayer.rage = 0;
                    dmg = myPlayer.ArmorMitigate(dmg);
                    myPlayer.totalDamage += dmg;
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("EX{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    myPlayer.RollProcsMH(true);
                    break;
                case Outcome.Dodge: //TODO implement Improved Execute talent
                    myPlayer.rage -= myPlayer.MyContext.RollRange(0, myPlayer.rage * 0.25); //TODO check Execute cost on dodge
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("EX{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    myPlayer.LastDodge = now;
                    break;
                case Outcome.Miss: //TODO implement Improved Execute talent
                    myPlayer.rage -= myPlayer.MyContext.RollRange(0, myPlayer.rage * 0.25); //TODO check Execute cost on miss
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("EX{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    break;
                default:
                    break;
            }
            myPlayer.Think();
        }
        public override double AvailableWhen()
        {
            return lastUsed;
        }
    }
}
