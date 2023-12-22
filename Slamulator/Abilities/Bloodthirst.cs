using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class Bloodthirst : Ability
    {
        public double lastUsed = -6;
        public Bloodthirst(PlayerState p) : base(p)
        {
            ServerSideNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Do));
        }
        public override void Do()
        {
            double now = myPlayer.MyContext.Server.Time;
            lastUsed = now;
            myPlayer.GCDEnds = now + Globals.GCD;
            myPlayer.ThinkWhen(myPlayer.GCDEnds); //think after GCD
            myPlayer.ThinkWhen(now + 6); //think after ability cooldown
            Outcome oc = RollYellow();
            double dmg = 0;
            switch (oc)
            {
                case Outcome.Crit:
                    myPlayer.rage -= 30;
                    dmg = myPlayer.AP * 0.45 * myPlayer.damageMultiplier * 2.2;
                    dmg = myPlayer.ArmorMitigate(dmg);
                    myPlayer.totalDamage += dmg;
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("BT{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    myPlayer.Buffs.Flurry.Start();
                    myPlayer.RollProcsMH(true);
                    break;
                case Outcome.Hit:
                    myPlayer.rage -= 30;
                    dmg = myPlayer.AP * 0.45 * myPlayer.damageMultiplier;
                    dmg = myPlayer.ArmorMitigate(dmg);
                    myPlayer.totalDamage += dmg;
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("BT{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    myPlayer.RollProcsMH(true);
                    break;
                case Outcome.Dodge:
                    myPlayer.rage -= myPlayer.MyContext.RollRange(0, 7.5); //See "Bloodthirst and Whirlwind Rage Costs.avi"
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("BT{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    myPlayer.LastDodge = now;
                    break;
                case Outcome.Miss:
                    myPlayer.rage -= myPlayer.MyContext.RollRange(0,7.5); //See "Bloodthirst and Whirlwind Rage Costs.avi"
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("BT{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    break;
                default:
                    break;
            }
            myPlayer.Think();
        }
        public override double AvailableWhen()
        {
            return lastUsed + 6;
        }
    }
}
