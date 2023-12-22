using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class Overpower : Ability
    {
        public double lastUsed = -5;
        public Overpower(PlayerState p) : base(p)
        {
            ServerSideNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Do));
        }
        public override void Do()
        {
            double now = myPlayer.MyContext.Server.Time;
            lastUsed = now;
            myPlayer.GCDEnds = now + Globals.GCD;
            myPlayer.ThinkWhen(myPlayer.GCDEnds); //think after GCD
            myPlayer.ThinkWhen(now + 5); //think after ability cooldown
            double dmg = 0;
            double roll = myPlayer.MyContext.RNG.NextDouble();
            if (roll >= myPlayer.overpowerCritThreshold)
            {
                myPlayer.rage -= 5;
                dmg = RollDamage(myPlayer.normalizedMhWpnDmgMin + 35, myPlayer.normalizedMhWpnDmgMax + 35, 2.2);
                dmg = myPlayer.ArmorMitigate(dmg);
                myPlayer.totalDamage += dmg;
                if (myPlayer.logging) myPlayer.Log.Add(String.Format("OP{0:F2} | {1:F2} | {2:F2} *CRIT*", dmg, myPlayer.rage, now));
                myPlayer.Buffs.Flurry.Start();
                myPlayer.RollProcsMH(true);
            }
            else if (roll >= myPlayer.overpowerHitThreshold)
            {
                myPlayer.rage -= 5;
                dmg = RollDamage(myPlayer.normalizedMhWpnDmgMin + 35, myPlayer.normalizedMhWpnDmgMax + 35, 1);
                dmg = myPlayer.ArmorMitigate(dmg);
                myPlayer.totalDamage += dmg;
                if (myPlayer.logging) myPlayer.Log.Add(String.Format("OP{0:F2} | {1:F2} | {2:F2}", dmg, myPlayer.rage, now));
                myPlayer.RollProcsMH(true);
            }
            else
            {
                myPlayer.rage -= myPlayer.MyContext.RollRange(0, 1.25); //See "Bloodthirst and Whirlwind Rage Costs.avi"
                if (myPlayer.logging) myPlayer.Log.Add(String.Format("OP{0:F2} | {1:F2} | {2:F2}", dmg, myPlayer.rage, now));
            }
            myPlayer.Think();
        }
        public override double AvailableWhen()
        {
            double now = myPlayer.MyContext.Server.Time;
            if (myPlayer.MyContext.Server.Time - myPlayer.LastDodge < 5)
            {
                return lastUsed + 5;
            }
            else
            {
                return now + 5;
            }
        }
    }
}
