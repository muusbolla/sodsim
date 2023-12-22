using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class Whirlwind : Ability
    {
        public double lastUsed = -10;
        public Whirlwind(PlayerState p) : base(p)
        {
            ServerSideNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Do));
        }
        public override void Do()
        {
            double now = myPlayer.MyContext.Server.Time;
            lastUsed = now;
            myPlayer.GCDEnds = now + Globals.GCD;
            myPlayer.ThinkWhen(myPlayer.GCDEnds); //think after GCD
            myPlayer.ThinkWhen(now + 10); //think after ability cooldown
            Outcome oc = RollYellow();
            double dmg = 0;
            switch (oc)
            {
                case Outcome.Crit:
                    myPlayer.rage -= 25;
                    dmg = RollDamage(myPlayer.normalizedMhWpnDmgMin, myPlayer.normalizedMhWpnDmgMax, 2.2);
                    dmg = myPlayer.ArmorMitigate(dmg);
                    myPlayer.totalDamage += dmg;
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("WW{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    myPlayer.Buffs.Flurry.Start();
                    break;
                case Outcome.Hit:
                    myPlayer.rage -= 25;
                    dmg = RollDamage(myPlayer.normalizedMhWpnDmgMin, myPlayer.normalizedMhWpnDmgMax, 1);
                    dmg = myPlayer.ArmorMitigate(dmg);
                    myPlayer.totalDamage += dmg;
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("WW{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    break;
                case Outcome.Dodge:
                    myPlayer.rage -= 25;
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("WW{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    myPlayer.LastDodge = now;
                    break;
                case Outcome.Miss:
                    myPlayer.rage -= 25;
                    if (myPlayer.logging) myPlayer.Log.Add(String.Format("WW{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, now, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                    break;
                default:
                    break;
            }
            myPlayer.Think();
        }
        public override double AvailableWhen()
        {
            return lastUsed + 10;
        }
    }
}
