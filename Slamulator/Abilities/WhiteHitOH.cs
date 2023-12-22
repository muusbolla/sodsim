using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class WhiteHitOH : Ability
    {
        public WhiteHitOH(PlayerState p) : base(p)
        {
            ServerSideNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Do));
        }
        private double lastUpdate, timerPercentRemaining;
        public override void Do()
        {
            Outcome oc = RollWhiteHitOH();
            double dmg = RollWhiteDamageOH(oc);
            dmg = myPlayer.ArmorMitigate(dmg);
            myPlayer.RageGain(dmg);
            myPlayer.totalDamage += dmg;
            if (myPlayer.logging) myPlayer.Log.Add(String.Format("OH{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, myPlayer.MyContext.Server.Time, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
            lastUpdate = myPlayer.MyContext.Server.Time;
            timerPercentRemaining = 1;
            switch (oc)
            {
                case Outcome.Crit:
                    myPlayer.Buffs.Flurry.Start();
                    myPlayer.RollProcsOH(true);
                    break;
                case Outcome.Hit:
                    ((Flurry)myPlayer.Buffs.Flurry).RemoveCharge();
                    myPlayer.RollProcsOH(true);
                    break;
                case Outcome.Glance:
                    ((Flurry)myPlayer.Buffs.Flurry).RemoveCharge();
                    myPlayer.RollProcsOH(true);
                    break;
                case Outcome.Dodge:
                    ((Flurry)myPlayer.Buffs.Flurry).RemoveCharge();
                    myPlayer.LastDodge = myPlayer.MyContext.Server.Time;
                    break;
                case Outcome.Miss:
                    ((Flurry)myPlayer.Buffs.Flurry).RemoveCharge();
                    break;
                default:
                    break;
            }
            DoWhen(lastUpdate + timerPercentRemaining * myPlayer.ohSpeed);
            myPlayer.Think();
        }
        public override double AvailableWhen()
        {
            timerPercentRemaining -= (myPlayer.MyContext.Server.Time - lastUpdate) / myPlayer.ohSpeed;
            lastUpdate = myPlayer.MyContext.Server.Time;
            DoWhen(lastUpdate + timerPercentRemaining * myPlayer.ohSpeed);
            return lastUpdate + timerPercentRemaining * myPlayer.ohSpeed;
        }
    }
}
