using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    static class Globals
    {
        public static double BOSS_DEFENSE = 315;
        public const double DAMAGE_PER_RAGE = 30.75; //http://www.lurkerlounge.com/forums/thread-3656.html
        public const double GCD = 1.5;

        public static DWForm DWForm;
    }

    public enum Stance { Berserker, Battle, Defensive};
    public enum Outcome { Hit, Crit, Miss, Glance, Dodge, None };
    public delegate void Action();
    //public delegate void Proc(bool isMainHand);

    public class TimedAction
    {
        public TimedAction(double t, Action a)
        {
            this.Time = t;
            this.Action = a;
        }
        public double Time;
        public Action Action;
    }

    public class ItemProc
    {
        public ItemProc(string i, double p)
        {
            this.Item = i;
            this.ProcChance = p;
        }
        public string Item;
        public double ProcChance;
    }
}
