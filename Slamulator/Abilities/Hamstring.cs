using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class Hamstring : Ability
    {
        public double RageCost = 10;
        public Hamstring(PlayerState p) : base(p)
        {
            ServerSideNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Do));
        }
        public override void Do()
        {
            double now = myPlayer.MyContext.Server.Time;
            myPlayer.GCDEnds = now + Globals.GCD;
            myPlayer.ThinkWhen(myPlayer.GCDEnds); //think after GCD
            Outcome oc = RollYellow();
            double dmg = 0;
            switch (oc)
            {
                case Outcome.Crit:
                    myPlayer.rage -= RageCost;
                    dmg = 45 * myPlayer.damageMultiplier * 2.2;
                    dmg = myPlayer.ArmorMitigate(dmg);
                    myPlayer.totalDamage += dmg;
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("HM{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    myPlayer.Buffs.Flurry.Start();
                    myPlayer.RollProcsMH(true);
                    break;
                case Outcome.Hit:
                    myPlayer.rage -= RageCost;
                    dmg = 45 * myPlayer.damageMultiplier;
                    dmg = myPlayer.ArmorMitigate(dmg);
                    myPlayer.totalDamage += dmg;
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("HM{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    myPlayer.RollProcsMH(true);
                    break;
                case Outcome.Dodge:
                    myPlayer.rage -= myPlayer.MyContext.RollRange(0, RageCost * 0.25); //See "Bloodthirst and Whirlwind Rage Costs.avi"
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("HM{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    myPlayer.LastDodge = now;
                    break;
                case Outcome.Miss:
                    myPlayer.rage -= myPlayer.MyContext.RollRange(0, RageCost * 0.25); //See "Bloodthirst and Whirlwind Rage Costs.avi"
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("HM{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    break;
                default:
                    break;
            }
            myPlayer.Think();
        }
        public override double AvailableWhen()
        {
            return 0;
        }
    }
}