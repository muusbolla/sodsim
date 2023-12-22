using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class Slam : Ability
    {
        public double lastUsed = -1;
        public Slam(PlayerState p) : base(p)
        {
            ServerSideNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Do));
        }
        public override void Do()
        {
            double now = myPlayer.MyContext.Server.Time;
            lastUsed = now;
            Outcome oc = RollYellow();
            double dmg = 0;
            switch (oc)
            {
                case Outcome.Crit:
                    myPlayer.rage -= 15;
                    dmg = RollDamage(myPlayer.mhWpnDmgMin + 87, myPlayer.mhWpnDmgMax + 87, 2.2); //TODO implement Rank 9 HS option
                    dmg = myPlayer.ArmorMitigate(dmg);
                    myPlayer.totalDamage += dmg;
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("SL{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    myPlayer.Buffs.Flurry.Start();
                    myPlayer.RollProcsMH(true);
                    break;
                case Outcome.Hit:
                    myPlayer.rage -= 15;
                    dmg = RollDamage(myPlayer.mhWpnDmgMin + 87, myPlayer.mhWpnDmgMax + 87, 1);
                    dmg = myPlayer.ArmorMitigate(dmg);
                    myPlayer.totalDamage += dmg;
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("SL{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    myPlayer.RollProcsMH(true);
                    break;
                case Outcome.Dodge:
                    myPlayer.rage -= myPlayer.MyContext.RollRange(0, 3.75); //See "Bloodthirst and Whirlwind Rage Costs.avi"
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("SL{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    myPlayer.LastDodge = now;
                    break;
                case Outcome.Miss:
                    myPlayer.rage -= myPlayer.MyContext.RollRange(0, 3.75); //See "Bloodthirst and Whirlwind Rage Costs.avi"
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("SL{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    break;
                default:
                    break;
            }
            ((WhiteHitMH)myPlayer.Abilities.WhiteHitMH).ResetSwingTimer();
        }
        public override double AvailableWhen()
        {
            return lastUsed + 6;
        }
    }
}
