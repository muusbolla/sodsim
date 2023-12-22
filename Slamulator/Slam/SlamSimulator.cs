using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    //TODO add overpower to rotation
    //TODO factor in mid-swing flurry gain
    class SlamSimulator
    {
        const double BASE_MISS = 0.08;
        const double DAMAGE_PER_RAGE = 30.75; //http://www.lurkerlounge.com/forums/thread-3656.html
        const double GCD = 1.5;
        Random rand;
        
        double whiteGlanceThreshold, whiteMissThreshold, whiteHitThreshold, whiteCritThreshold;
        double yellowMissThreshold, yellowHitThreshold, yellowCritThreshold;

        //public enum Action { WhiteHit, Slam, Bloodthirst, Whirlwind, Execute, RageGain };
        public enum Outcome { Hit, Crit, Miss, Glance, Dodge };
        double wpnDmgMin, wpnDmgMax, wpnSpeed, baseAP;
        double haste, armor;
        double tooltipHit, tooltipCrit, wpnSkill;
        double slamCastTime, slamDelay, maxWhiteHitDelay;
        bool am, hoj, bre, rage_dump;

        double rage, AP, damageMult;
        public double totalDamage;
        double time, nextBT, nextWW, nextGCD, nextWhiteHit;

        int flurryCharges;
        int breStacks;
        double breApplyTime;
        double breExpirationTime;
        public double breWeightedUptime;

        Action Rotation;

        public delegate void Action();
        public class Pair
        {
            public Pair(double t, Action a)
            {
                this.Time = t;
                this.Action = a;
            }
            public double Time;
            public Action Action;
        }
        LinkedList<Pair> actionList;
        LinkedListNode<Pair> WhiteHitNode;
        LinkedListNode<Pair> SlamNode;
        LinkedListNode<Pair> BTNode;
        LinkedListNode<Pair> WWNode;
        LinkedListNode<Pair> ExpireBRENode;
        List<string> log;

        bool didSlam; //lazy hack
        bool logging;

        public void InitSimulator(double i_min, double i_max, double i_speed, double i_AP,
            double i_hit, double i_crit, double i_skill, double i_haste,
            double i_armor,
            bool i_am, bool i_hoj, bool i_bre,
            bool i_rage_dump, bool i_logging)
        {
            wpnDmgMin = i_min;
            wpnDmgMax = i_max;
            wpnSpeed = i_speed;
            baseAP = i_AP;
            tooltipHit = i_hit;
            tooltipCrit = i_crit;
            wpnSkill = i_skill;
            haste = i_haste;
            armor = i_armor;
            rand = new Random();
            InitAttackTable(tooltipHit, tooltipCrit, wpnSkill);
            am = i_am;
            hoj = i_hoj;
            bre = i_bre;
            rage_dump = i_rage_dump;
            logging = i_logging;
            slamCastTime = 1.0;     //TODO add slam cast time to UI
            slamDelay = 0.0;        //TODO add slam delay (lag) to UI
            maxWhiteHitDelay = 0.1; //TODO add option to potentially delay white swings to UI

            actionList = new LinkedList<Pair>();

            log = new List<string>();
        }

        public string[] GetLog()
        {
            return log.ToArray();
        }

        public void Run(double duration)
        {
            time = 0.0;
            nextBT = 0.0;
            nextWW = 0.0;
            nextGCD = 0.0;
            nextWhiteHit = 0.0;

            rage = 0.0;
            totalDamage = 0.0;
            flurryCharges = 0;
            breStacks = 0;
            breApplyTime = 0.0;
            damageMult = 1.0;
            AP = baseAP;

            didSlam = false;

            if(logging && duration > 1000)
            {
                System.Windows.Forms.DialogResult dlg =
                    System.Windows.Forms.MessageBox.Show("Warning: You are about to produce a very large log file. Are you sure you want to do this?",
                    "Warning", System.Windows.Forms.MessageBoxButtons.OKCancel);
                if(dlg != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
            }

            WhiteHitNode = new LinkedListNode<Pair>(new Pair(0.0,DoWhiteHit));
            SlamNode = new LinkedListNode<Pair>(new Pair(0.0, DoSlam));
            BTNode = new LinkedListNode<Pair>(new Pair(0.0, DoBloodthirst));
            WWNode = new LinkedListNode<Pair>(new Pair(0.0, DoWhirlwind));
            ExpireBRENode = new LinkedListNode<Pair>(new Pair(0.0, DoExpireBRE));
            EnqueueNode(WhiteHitNode);
            LinkedListNode<Pair> curr = actionList.First;
            Rotation = PrepareNextAction;

            while (time < duration)
            {
                curr.Value.Action();
                actionList.RemoveFirst();
                Rotation();
                curr = actionList.First;
                time = curr.Value.Time;
            }
            if(bre)
            {
                breWeightedUptime = breWeightedUptime / duration;
            }
            if (logging)
            {
                string[] log = GetLog();
                bool exists = System.IO.Directory.Exists(@"C:\SlamLogs\");

                if (!exists)
                    System.IO.Directory.CreateDirectory(@"C:\SlamLogs\");
                System.IO.File.WriteAllLines(String.Format(@"C:\SlamLogs\{0}.txt", DateTime.Now.ToFileTimeUtc()), log);
            }
        }
        
        void PrepareNextAction()
        {
            double next_gcd = NextFreeGCD();
            double swing_time = SwingTime();
            //if(rage_dump && rage == 100)
            //{
            //    SlamNode.Value.Time = next_gcd + slamCastTime + slamDelay;
            //    RequeueNode(SlamNode);
            //    Rotation = RageDump;
            //}
            if (!didSlam && nextWhiteHit - next_gcd > swing_time - maxWhiteHitDelay && rage >= 15)
            {
                SlamNode.Value.Time = next_gcd + slamCastTime + slamDelay;
                RequeueNode(SlamNode);
            }
            else if (rage >= 30 && time > nextBT && next_gcd + GCD < nextWhiteHit + maxWhiteHitDelay)
            {
                BTNode.Value.Time = next_gcd;
                RequeueNode(BTNode);
            }
            else if (rage >= 25 && time > nextWW && next_gcd + GCD < nextWhiteHit + maxWhiteHitDelay)
            {
                WWNode.Value.Time = next_gcd;
                RequeueNode(WWNode);
            }
            else if (rage_dump && rage >= 40 && time < nextBT && time < nextWW && nextWhiteHit - next_gcd > swing_time - maxWhiteHitDelay - 0.5)
            {
                SlamNode.Value.Time = next_gcd + slamCastTime + slamDelay;
                RequeueNode(SlamNode);
            }
            else
            {
                WhiteHitNode.Value.Time = nextWhiteHit;
                RequeueNode(WhiteHitNode);
            }
        }

        void RageDump()
        {
            double next_gcd = NextFreeGCD();
            if (rage > 50)
            {
                SlamNode.Value.Time = next_gcd + slamCastTime + slamDelay;
                RequeueNode(SlamNode);
            }
            else
            {
                Rotation = PrepareNextAction;
                if (rage >= 30 && time > nextBT && next_gcd + GCD < nextWhiteHit + maxWhiteHitDelay)
                {
                    BTNode.Value.Time = next_gcd;
                    RequeueNode(BTNode);
                }
                else if (rage >= 25 && time > nextWW && next_gcd + GCD < nextWhiteHit + maxWhiteHitDelay)
                {
                    WWNode.Value.Time = next_gcd;
                    RequeueNode(WWNode);
                }
                else
                {
                    WhiteHitNode.Value.Time = nextWhiteHit;
                    RequeueNode(WhiteHitNode);
                }
            }
        }

        void RequeueNode(LinkedListNode<Pair> newNode)
        {
            if (newNode.List != null)
            {
                actionList.Remove(newNode);
            }
            EnqueueNode(newNode);
        }

        void EnqueueNode(LinkedListNode<Pair> newNode)
        {
            double t = newNode.Value.Time;
            LinkedListNode<Pair> insertNode = actionList.Last;
            while (insertNode != null && insertNode.Value.Time > t)
            {
                insertNode = insertNode.Previous;
            }
            if (insertNode == null)
            {
                actionList.AddFirst(newNode);
            }
            else
            {
                actionList.AddAfter(insertNode, newNode);
            }
        }

        void EnqueueAction(double t, Action a)
        {
            EnqueueNode(new LinkedListNode<Pair> (new Pair(t, a)));
        }

        void RemoveAllActionsOfTypeFromQueue(Action a)
        {
            LinkedListNode<Pair> removeNode = actionList.First;
            LinkedListNode<Pair> next;
            while(removeNode != null)
            {
                next = removeNode.Next;
                if(removeNode.Value.Action == a)
                {
                    actionList.Remove(removeNode);
                }
                removeNode = next;
            }
        }

        void DoWhiteHit()
        {
            Outcome oc = RollWhite();
            double dmg = 0;
            switch (oc)
            {
                case Outcome.Crit:
                    dmg = RollDamage(wpnDmgMin + AP * wpnSpeed / 14, wpnDmgMax + AP * wpnSpeed / 14, damageMult * 2.0);
                    flurryCharges = 3;
                    RollProcs();
                    break;
                case Outcome.Hit:
                    dmg = RollDamage(wpnDmgMin + AP * wpnSpeed / 14, wpnDmgMax + AP * wpnSpeed / 14, damageMult);
                    --flurryCharges;
                    RollProcs();
                    break;
                case Outcome.Dodge:
                    break;
                case Outcome.Glance: //TODO integrate weapon skill affecting glance damage
                    dmg = RollDamage(wpnDmgMin + AP * wpnSpeed / 14, wpnDmgMax + AP * wpnSpeed / 14, damageMult * 0.6);
                    RollProcs();
                    break;
                case Outcome.Miss:
                    break;
            }
            dmg = ArmorMitigate(dmg);
            rage += dmg / DAMAGE_PER_RAGE;
            if (rage > 100.0) rage = 100.0;
            totalDamage += dmg;
            nextWhiteHit = time + SwingTime();
            didSlam = false;
            if(logging) log.Add(String.Format("-{0:F2} | {1:F2} | {2:F2} {3}", dmg, rage, time, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
        }

        void DoSlam()
        {
            Outcome oc = RollYellow();
            double dmg = 0;
            switch (oc)
            {
                case Outcome.Crit:
                    dmg = RollDamage(87 + wpnDmgMin + AP * wpnSpeed / 14, 87 + wpnDmgMax + AP * wpnSpeed / 14, damageMult * 2.2);
                    flurryCharges = 3;
                    RollProcs();
                    break;
                case Outcome.Hit:
                    dmg = RollDamage(87 + wpnDmgMin + AP * wpnSpeed / 14, 87 + wpnDmgMax + AP * wpnSpeed / 14, damageMult);
                    RollProcs();
                    break;
                case Outcome.Dodge:
                    break;
                case Outcome.Miss:
                    break;
            }
            dmg = ArmorMitigate(dmg);
            rage -= 15; //TODO what happens to rage on a dodged/missed yellow swing?
            totalDamage += dmg;
            nextWhiteHit = time + SwingTime();
            nextGCD = time + GCD - slamCastTime;
            didSlam = true;
            if (logging) log.Add(String.Format("S{0:F2} | {1:F2} | {2:F2} {3}", dmg, rage, time, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
        }

        void DoBloodthirst()
        {
            Outcome oc = RollYellow();
            double dmg = 0;
            switch (oc)
            {
                case Outcome.Crit:
                    dmg = AP * 0.45 * damageMult * 2.2;
                    flurryCharges = 3;
                    RollProcs();
                    break;
                case Outcome.Hit:
                    dmg = AP * 0.45 * damageMult;
                    RollProcs();
                    break;
                case Outcome.Dodge:
                    break;
                case Outcome.Miss:
                    break;
            }
            dmg = ArmorMitigate(dmg);
            rage -= 30; //TODO what happens to rage on a dodged/missed yellow swing?
            totalDamage += dmg;
            nextBT = time + 6.0;
            nextGCD = time + GCD;
            if (logging) log.Add(String.Format("B{0:F2} | {1:F2} | {2:F2} {3}", dmg, rage, time, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
        }

        void DoWhirlwind()  //TODO in-game tooltip shows Whirlwind doing ~97% of weapon damage instead of 100%, verify
        {
            Outcome oc = RollYellow();
            double dmg = 0;
            switch (oc)
            {
                case Outcome.Crit:
                    dmg = RollDamage(wpnDmgMin + AP * wpnSpeed / 14, wpnDmgMax + AP * wpnSpeed / 14, damageMult * 2.2);
                    flurryCharges = 3;
                    //RollProcs();
                    break;
                case Outcome.Hit:
                    dmg = RollDamage(wpnDmgMin + AP * wpnSpeed / 14, wpnDmgMax + AP * wpnSpeed / 14, damageMult);
                    //RollProcs();
                    break;
                case Outcome.Dodge:
                    break;
                case Outcome.Miss:
                    break;
            }
            dmg = ArmorMitigate(dmg);
            rage -= 25; //TODO what happens to rage on a dodged/missed yellow swing?
            totalDamage += dmg;
            nextWW = time + 12.0;
            nextGCD = time + GCD;
            if (logging) log.Add(String.Format("W{0:F2} | {1:F2} | {2:F2} {3}", dmg, rage, time, oc == Outcome.Crit ? "*CRIT*" : String.Empty));
        }

        void DoExpireBRE()
        {
            SumBRETime();
            breStacks = 0;
            if (logging) log.Add(String.Format("BRE expired."));
        }

        void RollProcs()
        {
            if(bre && RollBRE()) //gain a stack of Bonereaver's Edge buff
            {
                SumBRETime();
                breStacks = Math.Min(3, breStacks + 1);
                breApplyTime = time;
                breExpirationTime = time + 10.0;
                if (logging) log.Add(String.Format("BRE {0}", breStacks, breExpirationTime));
                ExpireBRENode.Value.Time = breExpirationTime;
                RequeueNode(ExpireBRENode);
            }
            if(hoj && RollHOJ()) //gain an extra attack from HOJ
            {

            }
        }

        void SumBRETime()
        {
            double bre_time = time - breApplyTime;
            breWeightedUptime += bre_time * breStacks;
        }

        double SwingTime()
        {
            if (flurryCharges > 0)
            {
                return wpnSpeed / haste / 1.3;
            }
            else
            {
                return wpnSpeed / haste;
            }
        }

        double NextFreeGCD()
        {
            return Math.Max(time,nextGCD);
        }

        double ArmorMitigate(double rawDamage)
        {
            //https://forum.nostalrius.org/viewtopic.php?f=24&t=17968
            //DR% = Armor / (Armor + 400 + 85 * (AttackerLevel + 4.5 * (AttackerLevel - 60))) matches my character tooltip
            //for an attacker of level 60 (the player) this simplifies to Armor / (Armor + 5500)
            double finalArmor = Math.Max(0.0, armor - breStacks * 700);
            return rawDamage * (1- (finalArmor / (finalArmor + 5500)));
        }

        
        void InitAttackTable(double tooltipHit, double tooltipCrit, double wpnSkill)
        {
            //TODO integrate weapon skill

            whiteGlanceThreshold = 0.05;
            whiteMissThreshold = whiteGlanceThreshold + 0.40;
            whiteHitThreshold = whiteMissThreshold + ((BASE_MISS - tooltipHit) < 0 ? 0 : (BASE_MISS - tooltipHit));
            whiteCritThreshold = (1.0 - tooltipCrit) < whiteHitThreshold ? whiteHitThreshold : (1.0 - tooltipCrit);

            yellowMissThreshold = 0.05;
            yellowHitThreshold = yellowMissThreshold + ((BASE_MISS - tooltipHit) < 0 ? 0 : (BASE_MISS - tooltipHit));
            yellowCritThreshold = (1.0 - tooltipCrit) < yellowHitThreshold ? yellowHitThreshold : (1.0 - tooltipCrit);
        }

        public Outcome RollWhite()
        {
            double roll = rand.NextDouble();
            if (roll >= whiteCritThreshold)
            {
                return Outcome.Crit;
            }
            else if (roll >= whiteHitThreshold)
            {
                return Outcome.Hit;
            }
            else if (roll >= whiteMissThreshold)
            {
                return Outcome.Miss;
            }
            else if (roll >= whiteGlanceThreshold)
            {
                return Outcome.Glance;
            }
            else
            {
                return Outcome.Dodge;
            }
        }

        public Outcome RollYellow()
        {
            double roll = rand.NextDouble();
            if (roll >= yellowCritThreshold)
            {
                return Outcome.Crit;
            }
            else if (roll >= yellowHitThreshold)
            {
                return Outcome.Hit;
            }
            else if (roll >= yellowMissThreshold)
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
            double roll = rand.NextDouble();
            return (roll * (max - min) + min) * mult;
        }

        public bool RollHOJ() //Hand of Justice 2% proc chance
        {
            double roll = rand.NextDouble();
            if (roll >= 0.98)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RollBRE() //Bonereaver's Edge 15% proc chance - https://vanilla-twinhead.twinstar.cz/?issue=718
        {
            double roll = rand.NextDouble();
            if (roll >= 0.85)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
