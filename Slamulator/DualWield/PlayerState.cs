using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class PlayerState
    {

        //STATISTICS
        //base values
        private double _mhWpnDmgMin, _mhWpnDmgMax, _mhSpeed, _mhSkill;
        private double _ohWpnDmgMin, _ohWpnDmgMax, _ohSpeed, _ohSkill;
        private double _tooltipHit, _tooltipCritMH, _tooltipCritOH, _tooltipHaste;
        private double _str, _agi, _AP;
        private double _bossArmor, _armorPen;
        public bool _dualWield;
        private double _duration, _executeDuration;
        private double _ohWpnDmgFactor = 0.625;

        public double mhSpeedBase { get { return _mhSpeed; } }
        public double ohSpeedBase { get { return _ohSpeed; } }

        //in-fight / modified / buffed values
        public double mhWpnDmgMin, mhWpnDmgMax, mhSpeed, mhSkill, mhGlanceMin, mhGlanceMax, normalizedMhWpnDmgMin, normalizedMhWpnDmgMax;
        public double ohWpnDmgMin, ohWpnDmgMax, ohSpeed, ohSkill, ohGlanceMin, ohGlanceMax;
        public double baseStr, baseAgi, AP, critMH, critOH, hit;
        public double str, agi;
        public double damageMultiplier, bossArmor, armorPen;
        public double rage;

        public double mhWhiteGlanceThreshold, mhWhiteMissThreshold, mhWhiteHitThreshold, mhWhiteCritThreshold;
        public double ohWhiteGlanceThreshold, ohWhiteMissThreshold, ohWhiteHitThreshold, ohWhiteCritThreshold;
        public double yellowMissThreshold, yellowHitThreshold, yellowCritThreshold;
        public double overpowerHitThreshold, overpowerCritThreshold;

        private double executeStart, dwStart, reckStart;

        public Dictionary<string, Proc> ProcDict;
        public Dictionary<string, Proc> ExtraAttackProcDict;
        public Dictionary<string, Ability> AbilityDict;
        public AbilityCollection Abilities;
        public BuffCollection Buffs;
        public ProcCollection Procs;

        List<Proc> ProcListMH;
        List<Proc> ExtraAttackProcListMH;
        List<Proc> ProcListOH;
        List<Proc> ExtraAttackProcListOH;
        List<Ability> ItemAbilityList;
        LinkedList<Buff> PrimaryBuffList;
        LinkedList<Buff> SecondaryBuffList;
        LinkedList<Buff> FinalBuffList;
        public List<string> Log;

        double latency = 0; //TODO implement latency?
        public Stance CurrentStance;
        public double LastDodge = -10;
        public double GCDEnds = 0;
        public double stanceGCDEnds = 0;
        public double totalDamage = 0;
        public double totalRage = 0;
        public double wastedRage = 0;
        public bool logging = false;
        double breApplyTime = 0.0;
        public Action Think;
        public ThreadedSimulationContext MyContext;

        public PlayerState(ThreadedSimulationContext _myContext)
        {
            MyContext = _myContext;
            PrimaryBuffList = new LinkedList<Buff>();
            SecondaryBuffList = new LinkedList<Buff>();
            FinalBuffList = new LinkedList<Buff>();
            Log = new List<string>();
            ProcListMH = new List<Proc>();
            ExtraAttackProcListMH = new List<Proc>();
            ProcListOH = new List<Proc>();
            ExtraAttackProcListOH = new List<Proc>();

            //Use proper item names - must match character sheet exactly
            Procs = new ProcCollection(this);
            Abilities = new AbilityCollection(this);
            Buffs = new BuffCollection(this);
            ProcDict = new Dictionary<string, Proc>();
            ProcDict.Add("Bonereaver's Edge", Procs.BonereaversEdge);
            ProcDict.Add("Empyrean Demolisher", Procs.EmpyreanDemolisher);
            ProcDict.Add("Eskhandar's Right Claw", Procs.EskhandarsRightClaw);
            ProcDict.Add("Felstriker", Procs.Felstriker);
            ProcDict.Add("Crusader", Procs.Crusader);
            ProcDict.Add("Darkmoon Card: Maelstrom", Procs.DMCMaelstrom);
            ProcDict.Add("Heart of Wyrmthalak", Procs.HeartOfWyrmthalak);
            ProcDict.Add("The Untamed Blade", Procs.UntamedBlade);

            ExtraAttackProcDict = new Dictionary<string, Proc>();
            ExtraAttackProcDict.Add("Hand of Justice", Procs.HandOfJustice);
            ExtraAttackProcDict.Add("Ironfoe", Procs.Ironfoe);
            ExtraAttackProcDict.Add("Flurry Axe", Procs.FlurryAxe);
            ExtraAttackProcDict.Add("Windfury Totem", Procs.WindfuryTotem);

        }

        public void BeginCombat()
        {
            stanceGCDEnds = 0;
            GCDEnds = 0;
            totalDamage = 0;
            totalRage = 0;
            wastedRage = 0;

            //TODO figure out synchronization of attacks at the start
            Abilities.WhiteHitMH.DoWhen(0.0);
            if (_dualWield)
            {
                Abilities.WhiteHitOH.DoWhen(0.2); //TODO more testing with random swing timer delay
            }
        }

        public void SetRotation(string rotString)
        {
            switch(rotString)
            {
                case "BT > WW > HS":
                    Think = BasicRotation;
                    break;
                case "BT > WW > Ham > HS":
                    Think = HamstringRotation;
                    break;
                case "BT > Ham > WW > HS":
                    Think = SpamstringRotation;
                    break;
                case "Slam > BT > WW > Ham > HS":
                    Think = SlamRotation;
                    break;
                default:
                    Think = BasicRotation;
                    break;
            }
            return;
        }

        public void BasicRotation()
        {
            double now = MyContext.Server.Time;
            if (now >= dwStart && Abilities.DeathWish.AvailableWhen() <= now)
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 10)
                    {
                        for(int i = 0; i < ItemAbilityList.Count; i++) //TODO untether trinket cooldown from DW cooldown
                        {
                            ItemAbilityList[i].DoWhen(now + latency);
                        }
                        Abilities.DeathWish.DoWhen(now + latency);
                        dwStart += 180;
                        if(_duration - dwStart > 30 && _duration - dwStart < 210)
                        {
                            dwStart = _duration - 30;
                        }
                        return;
                    }
                }
            }
            else if (now >= executeStart)
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 10) //TODO fix hardcoded rage cost of execute
                    {
                        Abilities.Execute.DoWhen(now + latency);
                        return;
                    }
                }
            }
            else if (Abilities.Bloodthirst.AvailableWhen() <= now)
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 30)
                    {
                        Abilities.Bloodthirst.DoWhen(now + latency);
                        return;
                    }
                }
            }
            else if (Abilities.Overpower.AvailableWhen() <= now)
            {
                if(CurrentStance == Stance.Battle)
                {
                    if (GCDEnds <= now)
                    {
                        if (rage >= 5)
                        {
                            Abilities.Overpower.DoWhen(now + latency);
                            return;
                        }
                    }
                }
                else
                {
                    if (stanceGCDEnds <= now)
                    {
                        Abilities.BattleStance.DoWhen(now + latency);
                    }
                }
            }
            else if (CurrentStance == Stance.Battle && stanceGCDEnds <= now)
            {
                Abilities.BerserkerStance.DoWhen(now + latency);
            }
            else if (Abilities.Whirlwind.AvailableWhen() <= now && CurrentStance == Stance.Berserker)
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 25)
                    {
                        Abilities.Whirlwind.DoWhen(now + latency);
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                if (rage >= 42) //TODO allow use of HS while spamming other abilities
                {
                    ((WhiteHitMH)Abilities.WhiteHitMH).isHeroicStrike = true; //TODO implement cleave and HS switching
                    return;
                }
                else
                {
                    return;
                }
            }
        }

        public void HamstringRotation()
        {
            double now = MyContext.Server.Time;
            if (now >= dwStart && Abilities.DeathWish.AvailableWhen() <= now)
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 10)
                    {
                        for (int i = 0; i < ItemAbilityList.Count; i++) //TODO untether trinket cooldown from DW cooldown
                        {
                            ItemAbilityList[i].DoWhen(now + latency);
                        }
                        Abilities.DeathWish.DoWhen(now + latency);
                        dwStart += 180;
                        if (_duration - dwStart > 30 && _duration - dwStart < 210)
                        {
                            dwStart = _duration - 30;
                        }
                        return;
                    }
                }
            }
            else if (now >= executeStart)
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 10) //TODO fix hardcoded rage cost of execute
                    {
                        Abilities.Execute.DoWhen(now + latency);
                        return;
                    }
                }
            }
            else if (Abilities.Bloodthirst.AvailableWhen() <= now)
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 30)
                    {
                        Abilities.Bloodthirst.DoWhen(now + latency);
                        return;
                    }
                }
            }
            else if (Abilities.Overpower.AvailableWhen() <= now)
            {
                if (CurrentStance == Stance.Battle)
                {
                    if (GCDEnds <= now)
                    {
                        if (rage >= 5)
                        {
                            Abilities.Overpower.DoWhen(now + latency);
                            return;
                        }
                    }
                }
                else
                {
                    if (stanceGCDEnds <= now)
                    {
                        Abilities.BattleStance.DoWhen(now + latency);
                    }
                }
            }
            else if (CurrentStance == Stance.Battle && stanceGCDEnds <= now)
            {
                Abilities.BerserkerStance.DoWhen(now + latency);
            }
            else if (Abilities.Whirlwind.AvailableWhen() <= now && CurrentStance == Stance.Berserker)
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 25)
                    {
                        Abilities.Whirlwind.DoWhen(now + latency);
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 10)
                    {
                        Abilities.Hamstring.DoWhen(now + latency);
                        if (rage >= 52)
                        {
                            ((WhiteHitMH)Abilities.WhiteHitMH).isHeroicStrike = true; //TODO implement cleave and HS switching
                        }
                        return;
                    }

                }
                else if (rage >= 42)
                {
                    ((WhiteHitMH)Abilities.WhiteHitMH).isHeroicStrike = true; //TODO implement cleave and HS switching
                    return;
                }
                else
                {
                    return;
                }
            }
        }

        public void SpamstringRotation()
        {
            double now = MyContext.Server.Time;
            if (now >= dwStart && Abilities.DeathWish.AvailableWhen() <= now)
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 10)
                    {
                        for (int i = 0; i < ItemAbilityList.Count; i++) //TODO untether trinket cooldown from DW cooldown
                        {
                            ItemAbilityList[i].DoWhen(now + latency);
                        }
                        Abilities.DeathWish.DoWhen(now + latency);
                        dwStart += 180;
                        if (_duration - dwStart > 30 && _duration - dwStart < 210)
                        {
                            dwStart = _duration - 30;
                        }
                        return;
                    }
                }
            }
            else if (now >= executeStart)
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 10) //TODO fix hardcoded rage cost of execute
                    {
                        Abilities.Execute.DoWhen(now + latency);
                        return;
                    }
                }
            }
            else if (Abilities.Bloodthirst.AvailableWhen() <= now)
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 30)
                    {
                        Abilities.Bloodthirst.DoWhen(now + latency);
                        return;
                    }
                }
            }
            else if (Abilities.Overpower.AvailableWhen() <= now)
            {
                if (CurrentStance == Stance.Battle)
                {
                    if (GCDEnds <= now)
                    {
                        if (rage >= 5)
                        {
                            Abilities.Overpower.DoWhen(now + latency);
                            return;
                        }
                    }
                }
                else
                {
                    if (stanceGCDEnds <= now)
                    {
                        Abilities.BattleStance.DoWhen(now + latency);
                    }
                }
            }
            else if (CurrentStance == Stance.Battle && stanceGCDEnds <= now)
            {
                Abilities.BerserkerStance.DoWhen(now + latency);
            }
            //else if (Abilities.Whirlwind.AvailableWhen() <= now && CurrentStance == Stance.Berserker)
            //{
            //    if (GCDEnds <= now)
            //    {
            //        if (rage >= 25)
            //        {
            //            Abilities.Whirlwind.DoWhen(now + latency);
            //            return;
            //        }
            //        else
            //        {
            //            return;
            //        }
            //    }
            //}
            else
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 10)
                    {
                        Abilities.Hamstring.DoWhen(now + latency);
                        if (rage >= 52)
                        {
                            ((WhiteHitMH)Abilities.WhiteHitMH).isHeroicStrike = true; //TODO implement cleave and HS switching
                        }
                        return;
                    }

                }
                else if (rage >= 42)
                {
                    ((WhiteHitMH)Abilities.WhiteHitMH).isHeroicStrike = true; //TODO implement cleave and HS switching
                    return;
                }
                else
                {
                    return;
                }
            }
        }

        public void SlamRotation()
        {
            double now = MyContext.Server.Time;
            Abilities.WhiteHitMH.AvailableWhen();
            if (now >= dwStart && Abilities.DeathWish.AvailableWhen() <= now)
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 10)
                    {
                        for (int i = 0; i < ItemAbilityList.Count; i++) //TODO untether trinket cooldown from DW cooldown
                        {
                            ItemAbilityList[i].DoWhen(now + latency);
                        }
                        Abilities.DeathWish.DoWhen(now + latency);
                        dwStart += 180;
                        if (_duration - dwStart > 30 && _duration - dwStart < 210)
                        {
                            dwStart = _duration - 30;
                        }
                        return;
                    }
                }
            }
            else if (((WhiteHitMH)Abilities.WhiteHitMH).timerPercentRemaining == 1)
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 15)
                    {
                        Abilities.Slam.DoWhen(now + 1.0 + latency); //TODO implement improved slam talent, for now assume 5/5
                        GCDEnds = now + Globals.GCD;
                        ThinkWhen(GCDEnds); //think after GCD
                        return;
                    }
                    return;
                }
            }
            else if (now >= executeStart)
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 10) //TODO fix hardcoded rage cost of execute
                    {
                        Abilities.Execute.DoWhen(now + latency);
                        return;
                    }
                }
            }
            else if (Abilities.Bloodthirst.AvailableWhen() <= now)
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 30)
                    {
                        Abilities.Bloodthirst.DoWhen(now + latency);
                        return;
                    }
                }
            }
            else if (Abilities.Overpower.AvailableWhen() <= now)
            {
                if (CurrentStance == Stance.Battle)
                {
                    if (GCDEnds <= now)
                    {
                        if (rage >= 5)
                        {
                            Abilities.Overpower.DoWhen(now + latency);
                            return;
                        }
                    }
                }
                else
                {
                    if (stanceGCDEnds <= now)
                    {
                        Abilities.BattleStance.DoWhen(now + latency);
                    }
                }
            }
            else if (CurrentStance == Stance.Battle && stanceGCDEnds <= now)
            {
                Abilities.BerserkerStance.DoWhen(now + latency);
            }
            else if (Abilities.Whirlwind.AvailableWhen() <= now && CurrentStance == Stance.Berserker)
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 25)
                    {
                        Abilities.Whirlwind.DoWhen(now + latency);
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                if (GCDEnds <= now)
                {
                    if (rage >= 10)
                    {
                        Abilities.Hamstring.DoWhen(now + latency);
                        if (rage >= 52)
                        {
                            ((WhiteHitMH)Abilities.WhiteHitMH).isHeroicStrike = true; //TODO implement cleave and HS switching
                        }
                        return;
                    }

                }
                else if (rage >= 80) //only use HS at very high rage with 2 hander
                {
                    ((WhiteHitMH)Abilities.WhiteHitMH).isHeroicStrike = true; //TODO implement cleave and HS switching
                    return;
                }
                else
                {
                    return;
                }
            }
        }

        public void ThinkWhen(double time)
        {
            MyContext.Server.EnqueueAction(time, Think);
        }

        public void Init(double i_mh_min, double i_mh_max, double i_mh_speed, double i_mh_skill,
            double i_oh_min, double i_oh_max, double i_oh_speed, double i_oh_skill,
            double i_str, double i_agi, double i_AP,
            double i_hit, double i_mh_crit, double i_oh_crit, double i_haste, 
            double i_armor, bool i_dual_wield, bool i_logging,
            double i_duration, double i_executeDuration,
            List<ItemProc> i_mh_procs, List<ItemProc> i_oh_procs, string rotation)
        {
            _mhWpnDmgMin = i_mh_min;
            _mhWpnDmgMax = i_mh_max;
            _mhSpeed = i_mh_speed;
            _mhSkill = i_mh_skill;
            _ohWpnDmgMin = i_oh_min;
            _ohWpnDmgMax = i_oh_max;
            _ohSpeed = i_oh_speed;
            _ohSkill = i_oh_skill;
            _str = i_str;
            _agi = i_agi;
            _AP = i_AP;
            _tooltipHit = i_hit;
            _tooltipCritMH = i_mh_crit;
            _tooltipCritOH = i_oh_crit;
            _tooltipHaste = i_haste;
            _bossArmor = i_armor;
            armorPen = _armorPen = 0;
            _dualWield = i_dual_wield;
            logging = i_logging;

            _duration = i_duration;
            _executeDuration = i_executeDuration;
            executeStart = _duration - _executeDuration;
            if (executeStart < 0) executeStart = 0;
            reckStart = _duration - 15;
            if (reckStart < 0) reckStart = 0;
            dwStart = _duration - 30;
            if (dwStart < 0) dwStart = 0;
            if (dwStart >= 190)
            {
                dwStart = 10; //TODO fix hardcoded DW start lul?
            }
            
            Abilities = new AbilityCollection(this);
            Buffs = new BuffCollection(this);
            ItemAbilityList = new List<Ability>();
            AbilityDict = new Dictionary<string, Ability>();
            AbilityDict.Add("Earthstrike", Abilities.Earthstrike);
            AbilityDict.Add("Slayer's Crest", Abilities.SlayersCrest);
            AbilityDict.Add("Kiss of the Spider", Abilities.KissOfTheSpider);

            PrimaryBuffList.Clear();
            SecondaryBuffList.Clear();
            FinalBuffList.Clear();
            ProcListMH.Clear();
            ExtraAttackProcListMH.Clear();
            ProcListOH.Clear();
            ExtraAttackProcListOH.Clear();

            for (int i = 0; i < i_mh_procs.Count; i++)
            {
                //TODO nice hack hardcoding the hamstring cost reduction here...
                if (i_mh_procs[i].Item == "Rage of Mugamba")
                {
                    ((Hamstring)Abilities.Hamstring).RageCost -= 2;
                }
                else if (i_mh_procs[i].Item.Contains("Knight-Lt. Plate Gloves") || i_mh_procs[i].Item.Contains("Marshal's Plate Gauntlets"))
                {
                    ((Hamstring)Abilities.Hamstring).RageCost -= 3;
                }
                else if (ProcDict.ContainsKey(i_mh_procs[i].Item))
                {
                    Proc p = ProcDict[i_mh_procs[i].Item];
                    p.SetProcChance(i_mh_procs[i].ProcChance);
                    ProcListMH.Add(p);
                }
                else if (ExtraAttackProcDict.ContainsKey(i_mh_procs[i].Item))
                {
                    Proc p = ExtraAttackProcDict[i_mh_procs[i].Item];
                    p.SetProcChance(i_mh_procs[i].ProcChance);
                    ExtraAttackProcListMH.Add(p);
                }
                else if (AbilityDict.ContainsKey(i_mh_procs[i].Item))
                {
                    ItemAbilityList.Add(AbilityDict[i_mh_procs[i].Item]);
                }
            }

            for (int i = 0; i < i_oh_procs.Count; i++)
            {
                if(ProcDict.ContainsKey(i_oh_procs[i].Item))
                {
                    Proc p = ProcDict[i_oh_procs[i].Item];
                    p.SetProcChance(i_oh_procs[i].ProcChance);
                    ProcListOH.Add(p);
                }
                else if (ExtraAttackProcDict.ContainsKey(i_oh_procs[i].Item))
                {
                    Proc p = ExtraAttackProcDict[i_oh_procs[i].Item];
                    p.SetProcChance(i_oh_procs[i].ProcChance);
                    ExtraAttackProcListOH.Add(p);
                }
            }
            damageMultiplier = 1;
            CurrentStance = Stance.Berserker;
            UpdateStats();
            Log.Clear();
            SetRotation(rotation);
        }

        public void UpdateStats()
        {
            ResetStats();
            //TODO include tooltip haste
            LinkedList<Buff>.Enumerator e;
            e = PrimaryBuffList.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current.ApplyBuff();
            }

            str = baseStr;
            agi = baseAgi;

            e = SecondaryBuffList.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current.ApplyBuff();
            }

            AP += str * 2;
            critMH += agi / 2000;
            critMH += .05; //TODO correctly implement talents
            critOH += agi / 2000;
            critOH += .05; //TODO correctly implement talents
            if(CurrentStance == Stance.Berserker)
            {
                critMH += 0.03;
                critOH += 0.03;
            }

            mhWpnDmgMin = _mhWpnDmgMin + AP * _mhSpeed / 14;
            mhWpnDmgMax = _mhWpnDmgMax + AP * _mhSpeed / 14;
            normalizedMhWpnDmgMin = _mhWpnDmgMin + AP * (_dualWield ? 2.4 : 3.3) / 14; //yet another hack, assume 2H if not DW
            normalizedMhWpnDmgMax = _mhWpnDmgMax + AP * (_dualWield ? 2.4 : 3.3) / 14; //TODO add a checkbox for this or something
                                                                                       //TODO daggers have 1.7 normalization also

            ohWpnDmgMin = _ohWpnDmgMin + AP * _ohSpeed / 14 * _ohWpnDmgFactor;
            ohWpnDmgMax = _ohWpnDmgMax + AP * _ohSpeed / 14 * _ohWpnDmgFactor;

            MHInitAttackTable(hit, critMH, mhSkill, _dualWield);
            if (_dualWield)
            {
                OHInitAttackTable(hit, critOH, ohSkill, _dualWield);
            }

            e = FinalBuffList.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current.ApplyBuff();
            }
        }

        public void AddPrimaryBuff(LinkedListNode<Buff> b)
        {
            PrimaryBuffList.AddLast(b);
            UpdateStats();
        }

        public void RemovePrimaryBuff(LinkedListNode<Buff> b)
        {
            PrimaryBuffList.Remove(b);
            UpdateStats();
        }

        public void AddSecondaryBuff(LinkedListNode<Buff> b)
        {
            SecondaryBuffList.AddLast(b);
            UpdateStats();
        }

        public void RemoveSecondaryBuff(LinkedListNode<Buff> b)
        {
            SecondaryBuffList.Remove(b);
            UpdateStats();
        }

        public void AddFinalBuff(LinkedListNode<Buff> b)
        {
            FinalBuffList.AddLast(b);
            UpdateStats();
        }

        public void RemoveFinalBuff(LinkedListNode<Buff> b)
        {
            FinalBuffList.Remove(b);
            UpdateStats();
        }

        private void ResetStats()
        {
            mhSpeed = _mhSpeed;
            mhSkill = _mhSkill;
            ohSpeed = _ohSpeed;
            ohSkill = _ohSkill;
            baseStr = _str;
            baseAgi = _agi;
            AP = _AP;
            critMH = _tooltipCritMH;
            critOH = _tooltipCritOH;
            hit = _tooltipHit;
            bossArmor = _bossArmor;
            armorPen = _armorPen;
            damageMultiplier = 1;
            mhSpeed /= _tooltipHaste;
            ohSpeed /= _tooltipHaste;
        }

        public double ArmorMitigate(double rawDamage)
        {
            //https://forum.nostalrius.org/viewtopic.php?f=24&t=17968
            //DR% = Armor / (Armor + 400 + 85 * (AttackerLevel + 4.5 * (AttackerLevel - 60))) matches my character tooltip
            //for an attacker of level 60 (the player) this simplifies to Armor / (Armor + 5500)
            double finalArmor = Math.Max(0.0, bossArmor - armorPen);
            return rawDamage * (1 - (finalArmor / (finalArmor + 5500)));
        }

        public void RageGain(double damage)
        {
            double gain = damage / Globals.DAMAGE_PER_RAGE;
            totalRage += gain;
            rage += gain;
            if (rage > 100.0)
            {
                wastedRage += (rage - 100.0);
                rage = 100.0;
            }
        }

        public void MHInitAttackTable(double tooltipHit, double tooltipCrit, double wpnSkill, bool dualWield)
        {
            double wpnSkillMod = (Globals.BOSS_DEFENSE - wpnSkill) * 0.0004;
            double dodgeChance = 0.05 + wpnSkillMod;
            double glanceChance = 0.4;
            double missChance;
            //source: Oto's post at https://forum.nostalrius.org/viewtopic.php?f=37&t=5412&start=20
            //http://vanilla-wow.wikia.com/wiki/Hit
            //if (Globals.BOSS_DEFENSE - wpnSkill <= 10)
            //{
            //    missChance = 0.05 + (Globals.BOSS_DEFENSE - wpnSkill) * 0.001;
            //}
            //else
            //{
            //    missChance = 0.07 + (Globals.BOSS_DEFENSE - wpnSkill - 10) * 0.004;
            //}

            //KRONOS FORMULA
            //Source: personal testing on Kronos
            if (Globals.BOSS_DEFENSE - wpnSkill <= 10)
            {
                missChance = 0.05 + (Globals.BOSS_DEFENSE - wpnSkill) * 0.001;
            }
            else
            {
                //missChance = 0.06 + (Globals.BOSS_DEFENSE - wpnSkill - 10) * 0.004; //old formula
                missChance = 0.05 + (Globals.BOSS_DEFENSE - wpnSkill) * 0.002;
            }

            mhWhiteGlanceThreshold = dodgeChance;
            mhWhiteMissThreshold = mhWhiteGlanceThreshold + glanceChance;
            mhWhiteHitThreshold = Math.Max(mhWhiteMissThreshold, mhWhiteMissThreshold + missChance + (dualWield ? 0.19 : 0.0) - tooltipHit);
            mhWhiteCritThreshold = Math.Max((1.0 - tooltipCrit + wpnSkillMod), mhWhiteHitThreshold);

            //KRONOS FORMULA
            //formula from https://vanilla-twinhead.twinstar.cz/?issue=2109
            mhGlanceMin = 1.3 - 0.05 * (315 - wpnSkill);
            if (mhGlanceMin > 1.0) mhGlanceMin = 1.0;
            mhGlanceMax = 1.2 - 0.03 * (315 - wpnSkill);
            if (mhGlanceMax > 1.0) mhGlanceMax = 1.0;

            //ELYSIUM FORMULA
            //formula from https://cdn.discordapp.com/attachments/233351623173341184/265000258734522368/latest.jpg
            //https://elysium-project.org/bugtracker/issue/767
            //https://cdn.discordapp.com/attachments/233351623173341184/265000243429376001/BZuUPHD.png
            //mhGlanceMin = 1 - .05 * (Math.Pow(2, (63 - (wpnSkill / 5))) - 1);
            //if (mhGlanceMin > 1.0) mhGlanceMin = 1.0;
            //mhGlanceMax = 1 - .05 * (Math.Pow(2, (63 - (wpnSkill / 5))) - 1);
            //if (mhGlanceMax > 1.0) mhGlanceMax = 1.0;

            //yellow attack table is based off main hand, but unaffected by dual wield penalty
            yellowMissThreshold = dodgeChance;
            yellowHitThreshold = Math.Max(yellowMissThreshold, yellowMissThreshold + missChance - tooltipHit);
            yellowCritThreshold = Math.Max((1.0 - tooltipCrit + wpnSkillMod), yellowHitThreshold);
            
            overpowerHitThreshold = Math.Max(0, missChance - tooltipHit);
            overpowerCritThreshold = Math.Max((0.5 - tooltipCrit + wpnSkillMod), yellowHitThreshold);
        }

        public void OHInitAttackTable(double tooltipHit, double tooltipCrit, double wpnSkill, bool dualWield)
        {
            double wpnSkillMod = (Globals.BOSS_DEFENSE - wpnSkill) * 0.0004;
            double dodgeChance = 0.05 + wpnSkillMod;
            double glanceChance = 0.4;
            double missChance;
            //source: Oto's post at https://forum.nostalrius.org/viewtopic.php?f=37&t=5412&start=20
            //if (Globals.BOSS_DEFENSE - wpnSkill <= 10)
            //{
            //    missChance = 0.05 + (Globals.BOSS_DEFENSE - wpnSkill) * 0.001;
            //}
            //else
            //{
            //    missChance = 0.07 + (Globals.BOSS_DEFENSE - wpnSkill - 10) * 0.004;
            //}

            //KRONOS FORMULA
            //Source: personal testing on Kronos
            //Need to modify for classic
            if (Globals.BOSS_DEFENSE - wpnSkill <= 10)
            {
                missChance = 0.05 + (Globals.BOSS_DEFENSE - wpnSkill) * 0.001;
            }
            else
            {
                missChance = 0.06 + (Globals.BOSS_DEFENSE - wpnSkill - 10) * 0.004;
            }

            ohWhiteGlanceThreshold = dodgeChance;
            ohWhiteMissThreshold = ohWhiteGlanceThreshold + glanceChance;
            ohWhiteHitThreshold = Math.Max(ohWhiteMissThreshold, ohWhiteMissThreshold + missChance + (dualWield ? 0.19 : 0.0) - tooltipHit);
            ohWhiteCritThreshold = Math.Max((1.0 - tooltipCrit + wpnSkillMod), ohWhiteHitThreshold);

            //KRONOS FORMULA
            //formula from https://vanilla-twinhead.twinstar.cz/?issue=2109
            ohGlanceMin = 1.3 - 0.05 * (315 - wpnSkill);
            if (ohGlanceMin > 1.0) ohGlanceMin = 1.0;
            ohGlanceMax = 1.2 - 0.03 * (315 - wpnSkill);
            if (ohGlanceMax > 1.0) ohGlanceMax = 1.0;

            //ELYSIUM FORMULA
            //formula from https://cdn.discordapp.com/attachments/233351623173341184/265000258734522368/latest.jpg
            //https://elysium-project.org/bugtracker/issue/767
            //https://cdn.discordapp.com/attachments/233351623173341184/265000243429376001/BZuUPHD.png
            //ohGlanceMin = 1 - .05 * (Math.Pow(2, (63 - (wpnSkill / 5))) - 1);
            //if (ohGlanceMin > 1.0) ohGlanceMin = 1.0;
            //ohGlanceMax = 1 - .05 * (Math.Pow(2, (63 - (wpnSkill / 5))) - 1);
            //if (ohGlanceMax > 1.0) ohGlanceMax = 1.0;
        }

        public void HasteHack() //UGLY HACK LOL
        {
            //force white swings to recalculate time remaining when haste is gained or lost
            Abilities.WhiteHitMH.AvailableWhen();
            if (_dualWield)
            {
                Abilities.WhiteHitOH.AvailableWhen();
            }
        }

        public void RollProcsMH(bool allowExtraAttacks)
        {
            for (int i = 0; i < ProcListMH.Count; i++)
            {
                ProcListMH[i].RollProc(true);
            }
            if (allowExtraAttacks)
            {
                for (int i = 0; i < ExtraAttackProcListMH.Count; i++)
                {
                    ExtraAttackProcListMH[i].RollProc(true);
                }
            }
        }

        public void RollProcsOH(bool allowExtraAttacks)
        {
            for (int i = 0; i < ProcListOH.Count; i++)
            {
                ProcListOH[i].RollProc(false);
            }
            if (allowExtraAttacks)
            {
                for (int i = 0; i < ExtraAttackProcListOH.Count; i++)
                {
                    ExtraAttackProcListOH[i].RollProc(false);
                }
            }
        }

        public string[] GetLog()
        {
            return Log.ToArray();
        }
    }
}
