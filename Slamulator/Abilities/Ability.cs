using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class Ability
    {
        public LinkedListNode<TimedAction> ServerSideNode;
        protected PlayerState myPlayer;
        protected Ability(PlayerState p)
        {
            myPlayer = p;
        }
        public virtual void Do()
        {
        }

        public virtual void DoWhen(double time)
        {
            ServerSideNode.Value.Time = time;
            myPlayer.MyContext.Server.RequeueNode(ServerSideNode);
        }

        public virtual double AvailableWhen()
        {
            return myPlayer.MyContext.Server.Time;
        }

        public virtual Outcome RollWhiteHitMH()
        {
            double roll = myPlayer.MyContext.RNG.NextDouble();
            if (roll >= myPlayer.mhWhiteCritThreshold)
            {
                return Outcome.Crit;
            }
            else if (roll >= myPlayer.mhWhiteHitThreshold)
            {
                return Outcome.Hit;
            }
            else if (roll >= myPlayer.mhWhiteMissThreshold)
            {
                return Outcome.Miss;
            }
            else if (roll >= myPlayer.mhWhiteGlanceThreshold)
            {
                return Outcome.Glance;
            }
            else
            {
                return Outcome.Dodge;
            }
        }

        public virtual Outcome RollWhiteHitOH()
        {
            double roll = myPlayer.MyContext.RNG.NextDouble();
            if (roll >= myPlayer.ohWhiteCritThreshold)
            {
                return Outcome.Crit;
            }
            else if (roll >= myPlayer.ohWhiteHitThreshold)
            {
                return Outcome.Hit;
            }
            else if (roll >= myPlayer.ohWhiteMissThreshold)
            {
                return Outcome.Miss;
            }
            else if (roll >= myPlayer.ohWhiteGlanceThreshold)
            {
                return Outcome.Glance;
            }
            else
            {
                return Outcome.Dodge;
            }
        }

        public virtual Outcome RollYellow()
        {
            double roll = myPlayer.MyContext.RNG.NextDouble();
            if (roll >= myPlayer.yellowCritThreshold)
            {
                return Outcome.Crit;
            }
            else if (roll >= myPlayer.yellowHitThreshold)
            {
                return Outcome.Hit;
            }
            else if (roll >= myPlayer.yellowMissThreshold)
            {
                return Outcome.Miss;
            }
            else
            {
                return Outcome.Dodge;
            }
        }

        public double RollDamage(double min, double max, double mult)
        {
            double roll = myPlayer.MyContext.RNG.NextDouble();
            return (roll * (max - min) + min) * myPlayer.damageMultiplier * mult;
        }

        public virtual double RollWhiteDamageMH(Outcome oc)
        {
            double dmg = 0;
            switch (oc)
            {
                case Outcome.Crit:
                    dmg = RollDamage(myPlayer.mhWpnDmgMin, myPlayer.mhWpnDmgMax, 2);
                    break;
                case Outcome.Hit:
                    dmg = RollDamage(myPlayer.mhWpnDmgMin, myPlayer.mhWpnDmgMax, 1);
                    break;
                case Outcome.Dodge:
                    break;
                case Outcome.Glance:
                    double glancePercent = myPlayer.MyContext.RollRange(myPlayer.mhGlanceMin, myPlayer.mhGlanceMax);
                    dmg = RollDamage(myPlayer.mhWpnDmgMin, myPlayer.mhWpnDmgMax, glancePercent);
                    break;
                case Outcome.Miss:
                    break;
                default:
                    break;
            }
            return dmg;
        }

        public virtual double RollWhiteDamageOH(Outcome oc)
        {
            double dmg = 0;
            switch (oc)
            {
                case Outcome.Crit:
                    dmg = RollDamage(myPlayer.ohWpnDmgMin, myPlayer.ohWpnDmgMax, 2);
                    break;
                case Outcome.Hit:
                    dmg = RollDamage(myPlayer.ohWpnDmgMin, myPlayer.ohWpnDmgMax, 1);
                    break;
                case Outcome.Dodge:
                    break;
                case Outcome.Glance:
                    double glancePercent = myPlayer.MyContext.RollRange(myPlayer.ohGlanceMin, myPlayer.ohGlanceMax);
                    dmg = RollDamage(myPlayer.ohWpnDmgMin, myPlayer.ohWpnDmgMax, glancePercent);
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
