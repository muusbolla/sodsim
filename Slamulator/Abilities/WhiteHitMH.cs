using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class WhiteHitMH : Ability
    {
        public WhiteHitMH(PlayerState p) : base(p)
        {
            ServerSideNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Do));
        }
        public double lastUpdate, timerPercentRemaining;
        public bool isHeroicStrike = false;
        public bool isCleave = false;
        public override void Do()
        {
            lastUpdate = myPlayer.MyContext.Server.Time;
            timerPercentRemaining = 1;
            if (isHeroicStrike && myPlayer.rage >= 12)
            {
                Outcome oc = RollYellow();
                double dmg = 0;
                switch (oc)
                {
                    case Outcome.Crit:
                        myPlayer.rage -= 12; //TODO implement HS cost reduction talent
                        dmg = RollDamage(myPlayer.mhWpnDmgMin + 138, myPlayer.mhWpnDmgMax + 138, 2.2); //TODO implement Rank 9 HS option
                        dmg = myPlayer.ArmorMitigate(dmg);
                        myPlayer.totalDamage += dmg;
                        if (myPlayer.logging) myPlayer.Log.Add(String.Format("HS{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, myPlayer.MyContext.Server.Time, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                        myPlayer.Buffs.Flurry.Start();
                        myPlayer.RollProcsMH(true);
                        break;
                    case Outcome.Hit:
                        myPlayer.rage -= 12; //TODO implement HS cost reduction talent
                        dmg = RollDamage(myPlayer.mhWpnDmgMin + 138, myPlayer.mhWpnDmgMax + 138, 1);
                        dmg = myPlayer.ArmorMitigate(dmg);
                        myPlayer.totalDamage += dmg;
                        if (myPlayer.logging) myPlayer.Log.Add(String.Format("HS{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, myPlayer.MyContext.Server.Time, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                        ((Flurry)myPlayer.Buffs.Flurry).RemoveCharge();
                        myPlayer.RollProcsMH(true);
                        break;
                    case Outcome.Dodge:
                        myPlayer.rage -= myPlayer.MyContext.RollRange(0, 3.75); //TODO implement HS cost reduction talent
                        if (myPlayer.logging) myPlayer.Log.Add(String.Format("HS{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, myPlayer.MyContext.Server.Time, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                        ((Flurry)myPlayer.Buffs.Flurry).RemoveCharge();
                        myPlayer.LastDodge = myPlayer.MyContext.Server.Time;
                        break;
                    case Outcome.Miss:
                        myPlayer.rage -= myPlayer.MyContext.RollRange(0, 3.75); //TODO implement HS cost reduction talent
                        if (myPlayer.logging) myPlayer.Log.Add(String.Format("HS{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, myPlayer.MyContext.Server.Time, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                        ((Flurry)myPlayer.Buffs.Flurry).RemoveCharge();
                        break;
                    default:
                        break;
                }
            }
            else if (isCleave && myPlayer.rage >= 20)
            {
                Outcome oc = RollYellow();
                double dmg = 0;
                switch (oc)
                {
                    case Outcome.Crit:
                        myPlayer.rage -= 20; 
                        dmg = RollDamage(myPlayer.mhWpnDmgMin + 110, myPlayer.mhWpnDmgMax + 110, 2.2); //TODO implement Cleave damage talent
                        dmg = myPlayer.ArmorMitigate(dmg);
                        myPlayer.totalDamage += dmg;
                        if (myPlayer.logging) myPlayer.Log.Add(String.Format("CL{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, myPlayer.MyContext.Server.Time, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                        myPlayer.Buffs.Flurry.Start();
                        myPlayer.RollProcsMH(true);
                        break;
                    case Outcome.Hit:
                        myPlayer.rage -= 20;
                        dmg = RollDamage(myPlayer.mhWpnDmgMin + 110, myPlayer.mhWpnDmgMax + 110, 1); //TODO implement Cleave damage talent
                        dmg = myPlayer.ArmorMitigate(dmg);
                        myPlayer.totalDamage += dmg;
                        if (myPlayer.logging) myPlayer.Log.Add(String.Format("CL{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, myPlayer.MyContext.Server.Time, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                        ((Flurry)myPlayer.Buffs.Flurry).RemoveCharge();
                        myPlayer.RollProcsMH(true);
                        break;
                    case Outcome.Dodge:
                        myPlayer.rage -= myPlayer.MyContext.RollRange(0, 5);
                        if (myPlayer.logging) myPlayer.Log.Add(String.Format("CL{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, myPlayer.MyContext.Server.Time, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                        ((Flurry)myPlayer.Buffs.Flurry).RemoveCharge();
                        myPlayer.LastDodge = myPlayer.MyContext.Server.Time;
                        break;
                    case Outcome.Miss:
                        myPlayer.rage -= myPlayer.MyContext.RollRange(0, 5);
                        if (myPlayer.logging) myPlayer.Log.Add(String.Format("CL{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, myPlayer.MyContext.Server.Time, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                        ((Flurry)myPlayer.Buffs.Flurry).RemoveCharge();
                        break;
                    default:
                        break;
                }
            }
            else //regular white swing
            {
                Outcome oc = RollWhiteHitMH();
                double dmg = RollWhiteDamageMH(oc);
                dmg = myPlayer.ArmorMitigate(dmg);
                myPlayer.RageGain(dmg);
                myPlayer.totalDamage += dmg;
                if (myPlayer.logging) myPlayer.Log.Add(String.Format("MH{0:F2} | {1:F2} | {2:F2} {3}", dmg, myPlayer.rage, myPlayer.MyContext.Server.Time, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
                switch (oc)
                {
                    case Outcome.Crit:
                        myPlayer.Buffs.Flurry.Start();
                        myPlayer.RollProcsMH(true);
                        break;
                    case Outcome.Hit:
                        ((Flurry)myPlayer.Buffs.Flurry).RemoveCharge();
                        myPlayer.RollProcsMH(true);
                        break;
                    case Outcome.Glance:
                        ((Flurry)myPlayer.Buffs.Flurry).RemoveCharge();
                        myPlayer.RollProcsMH(true);
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
            }
            isHeroicStrike = false;
            isCleave = false;
            DoWhen(lastUpdate + timerPercentRemaining * myPlayer.mhSpeed);
            myPlayer.Think();
        }
        public override double AvailableWhen()
        {
            timerPercentRemaining -= (myPlayer.MyContext.Server.Time - lastUpdate) / myPlayer.mhSpeed;
            lastUpdate = myPlayer.MyContext.Server.Time;
            DoWhen(lastUpdate + timerPercentRemaining * myPlayer.mhSpeed);
            return lastUpdate + timerPercentRemaining * myPlayer.mhSpeed;
        }
        public void ResetSwingTimer()
        {
            lastUpdate = myPlayer.MyContext.Server.Time;
            timerPercentRemaining = 1;
            DoWhen(lastUpdate + myPlayer.mhSpeed);
        }
    }
}
