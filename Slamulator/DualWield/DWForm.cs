using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Slamulator
{
    public partial class DWForm : Form
    {
        double  _str, _agi, _AP;
        double str, agi, AP, crit, hit, haste;
        double mhWpnDmgMin, mhWpnDmgMax, mhWpnSpeed, mhWpnSkill;
        double ohWpnDmgMin, ohWpnDmgMax, ohWpnSpeed, ohWpnSkill;

        DataSet GearDataSet;
        Dictionary<string, ComboBox> gearBoxDict1, enchantBoxDict1;
        Dictionary<string, CheckBox> checkboxDict1;
        public DWForm()
        {
            InitializeComponent();
            GearDataSet = new DataSet();
            gearBoxDict1 = new Dictionary<string,ComboBox>
            {
                { "Head", cmbHead1 },
                { "Neck", cmbNeck1 },
                { "Shoulder", cmbShoulder1 },
                { "Back", cmbBack1 },
                { "Chest", cmbChest1 },
                { "Wrist", cmbWrist1 },
                { "Hand", cmbHand1 },
                { "Waist", cmbWaist1 },
                { "Legs", cmbLegs1 },
                { "Feet", cmbFeet1 },
                { "Ring1", cmbRing11 },
                { "Ring2", cmbRing21 },
                { "Trinket1", cmbTrinket11 },
                { "Trinket2", cmbTrinket21 },
                { "MeleeMH", cmbMeleeMH1 },
                { "MeleeOH", cmbMeleeOH1 },
                { "Ranged", cmbRanged1 }
            };
            enchantBoxDict1 = new Dictionary<string, ComboBox>
            {
                { "HeadEnchant", cmbHeadEnchant1 },
                { "ShoulderEnchant", cmbShoulderEnchant1 },
                { "BackEnchant", cmbBackEnchant1 },
                { "ChestEnchant", cmbChestEnchant1 },
                { "WristEnchant", cmbWristEnchant1 },
                { "HandEnchant", cmbHandEnchant1 },
                { "LegsEnchant", cmbLegsEnchant1 },
                { "FeetEnchant", cmbFeetEnchant1 },
                { "MeleeMHEnchant", cmbMeleeMHEnchant1 },
                { "MeleeOHEnchant", cmbMeleeOHEnchant1 }
            };


            cmbSimulationMode.SelectedIndex = 0;
            cmbRotation1.SelectedIndex = 0;


            _str = 120; _agi = 80; _AP = 160; //Human Warrior
            //double str = 117, agi = 85, ap = 160, crit = 0, hit = 0; //Nelf Warrior
            //double str = 80, agi = 130, ap = 100, crit = 0, hit = 0; //Human Rogue
            //double str = 75, agi = 133, ap = 100, crit = 0, hit = 0; //Gnome Rogue
            //double str = 77, agi = 135, ap = 100, crit = 0, hit = 0; //Nelf Rogue
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            btnGo.Enabled = false;
            double armor, armorReduction, level;
            double duration, executeDuration, numSimulations;

            bool option_dual_wield = cbDualWield.Checked;
            bool option_combat_log = cbLog.Checked;
            bool option_buff_uptime = cbBuffUptime.Checked;
            bool option_compare_ironfoe = false; //TODO contextual "compare ironfoe" button

            double.TryParse(tbArmor.Text, out armor);
            double.TryParse(tbArmorReduction.Text, out armorReduction);
            armor = armor - armorReduction;

            double.TryParse(tbLevel.Text, out level);
            double.TryParse(tbFightDuration.Text, out duration);
            double.TryParse(tbExecuteDuration.Text, out executeDuration);
            double.TryParse(tbNumRuns.Text, out numSimulations);

            if (option_combat_log && duration > 1000)
            {
                System.Windows.Forms.DialogResult dlg =
                    System.Windows.Forms.MessageBox.Show("Warning: You are about to produce a very large log file. Are you sure you want to do this?",
                    "Warning", System.Windows.Forms.MessageBoxButtons.OKCancel);
                if (dlg != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
            }

            List<ItemProc> itemListMH = new List<ItemProc>();
            //Future proofing a bit here, in case there are procs added to other slots (TBC meta gem maybe?)
            itemListMH.Add(new ItemProc((string)((DataRowView)gearBoxDict1["Head"].SelectedItem).Row["Name"], 0));
            itemListMH.Add(new ItemProc((string)((DataRowView)gearBoxDict1["Neck"].SelectedItem).Row["Name"], 0));
            itemListMH.Add(new ItemProc((string)((DataRowView)gearBoxDict1["Shoulder"].SelectedItem).Row["Name"], 0));
            itemListMH.Add(new ItemProc((string)((DataRowView)gearBoxDict1["Back"].SelectedItem).Row["Name"], 0));
            itemListMH.Add(new ItemProc((string)((DataRowView)gearBoxDict1["Chest"].SelectedItem).Row["Name"], 0));
            itemListMH.Add(new ItemProc((string)((DataRowView)gearBoxDict1["Wrist"].SelectedItem).Row["Name"], 0));
            itemListMH.Add(new ItemProc((string)((DataRowView)gearBoxDict1["Hand"].SelectedItem).Row["Name"], 0));
            itemListMH.Add(new ItemProc((string)((DataRowView)gearBoxDict1["Waist"].SelectedItem).Row["Name"], 0));
            itemListMH.Add(new ItemProc((string)((DataRowView)gearBoxDict1["Legs"].SelectedItem).Row["Name"], 0));
            itemListMH.Add(new ItemProc((string)((DataRowView)gearBoxDict1["Feet"].SelectedItem).Row["Name"], 0));
            itemListMH.Add(new ItemProc((string)((DataRowView)gearBoxDict1["Ring1"].SelectedItem).Row["Name"], 0));
            itemListMH.Add(new ItemProc((string)((DataRowView)gearBoxDict1["Ring2"].SelectedItem).Row["Name"], 0));
            //TODO add PPM or % parsing here
            double t1p = 0; if(tbTrinket1ProcPercent.Visible) { double.TryParse(tbTrinket1ProcPercent.Text, out t1p); }
            itemListMH.Add(new ItemProc((string)((DataRowView)gearBoxDict1["Trinket1"].SelectedItem).Row["Name"], t1p));
            double t2p = 0; if (tbTrinket2ProcPercent.Visible) { double.TryParse(tbTrinket2ProcPercent.Text, out t2p); }
            itemListMH.Add(new ItemProc((string)((DataRowView)gearBoxDict1["Trinket2"].SelectedItem).Row["Name"], t2p));
            //itemListMH.Add((string)((DataRowView)gearBoxDict1["Ranged"].SelectedItem).Row["Name"]);
            List<ItemProc> itemListOH = new List<ItemProc>(itemListMH);
            double w1p = 0; if (tbWeapon1ProcPercent.Visible) { double.TryParse(tbWeapon1ProcPercent.Text, out w1p); }
            itemListMH.Add(new ItemProc((string)((DataRowView)gearBoxDict1["MeleeMH"].SelectedItem).Row["Name"], w1p));
            //TODO add PPM for weapon enchant here
            itemListMH.Add(new ItemProc((string)((DataRowView)enchantBoxDict1["MeleeMHEnchant"].SelectedItem).Row["Name"], 0));
            if(cbWindfuryTotemMH1.Checked) { itemListMH.Add(new ItemProc("Windfury Totem", 0.2)); } //TODO hardcoded 20% wf totem?
            double w2p = 0; if (tbWeapon2ProcPercent.Visible) { double.TryParse(tbWeapon2ProcPercent.Text, out w2p); }
            itemListOH.Add(new ItemProc((string)((DataRowView)gearBoxDict1["MeleeOH"].SelectedItem).Row["Name"], w2p));
            itemListOH.Add(new ItemProc((string)((DataRowView)enchantBoxDict1["MeleeOHEnchant"].SelectedItem).Row["Name"], 0));
            
            if(cmbSimulationMode.Text == "Stat Weights")
            {
                double totalDamageBase = 0;
                double totalDamageAP = 0;
                double totalDamageHit = 0;
                double totalDamageCrit = 0;
                double totalDamageHaste = 0;
                double bonusAP = 28;
                double bonusHit = 0.01;
                double bonusCrit = 0.01;
                double bonusHaste = 0.01;

                int numThreads = 6;
                int iterations = (int)Math.Ceiling(numSimulations / numThreads);

                ThreadedSimulationContext[] contexts = new ThreadedSimulationContext[numThreads];
                for(int i = 0; i < numThreads; i++)
                {
                    contexts[i] = new ThreadedSimulationContext();
                }
                //Baseline damage
                for (int i = 0; i < iterations; i++)
                {
                    for(int j = 0; j < numThreads; j++)
                    {
                        InitSimWithBuffs(contexts[j], mhWpnDmgMin, mhWpnDmgMax, mhWpnSpeed, mhWpnSkill,
                            ohWpnDmgMin, ohWpnDmgMax, ohWpnSpeed, ohWpnSkill,
                            str, agi, AP,
                            hit, crit, haste, armor, level, 
                            option_dual_wield, option_combat_log,
                            duration, executeDuration,
                            itemListMH, itemListOH);
                        contexts[j].ThreadedSimulate(duration);
                    }
                    for (int j = 0; j < numThreads; j++)
                    {
                        contexts[j].Join();
                        totalDamageBase += contexts[j].Simulator.PlayerOne.totalDamage;
                    }
                }

                //Bonus AP
                for (int i = 0; i < iterations; i++)
                {
                    for (int j = 0; j < numThreads; j++)
                    {
                        InitSimWithBuffs(contexts[j], mhWpnDmgMin, mhWpnDmgMax, mhWpnSpeed, mhWpnSkill,
                        ohWpnDmgMin, ohWpnDmgMax, ohWpnSpeed, ohWpnSkill,
                        str, agi, AP + bonusAP,
                        hit, crit, haste, armor, level,
                        option_dual_wield, option_combat_log,
                        duration, executeDuration,
                        itemListMH, itemListOH);
                        contexts[j].ThreadedSimulate(duration);
                    }
                    for (int j = 0; j < numThreads; j++)
                    {
                        contexts[j].Join();
                        totalDamageAP += contexts[j].Simulator.PlayerOne.totalDamage;
                    }
                }

                //Bonus Hit
                for (int i = 0; i < iterations; i++)
                {
                    for (int j = 0; j < numThreads; j++)
                    {
                        InitSimWithBuffs(contexts[j], mhWpnDmgMin, mhWpnDmgMax, mhWpnSpeed, mhWpnSkill,
                        ohWpnDmgMin, ohWpnDmgMax, ohWpnSpeed, ohWpnSkill,
                        str, agi, AP,
                        hit + bonusHit, crit, haste, armor, level,
                        option_dual_wield, option_combat_log,
                        duration, executeDuration,
                        itemListMH, itemListOH);
                        contexts[j].ThreadedSimulate(duration);
                    }
                    for (int j = 0; j < numThreads; j++)
                    {
                        contexts[j].Join();
                        totalDamageHit += contexts[j].Simulator.PlayerOne.totalDamage;
                    }
                }

                //Bonus Crit
                for (int i = 0; i < iterations; i++)
                {
                    for (int j = 0; j < numThreads; j++)
                    {
                        InitSimWithBuffs(contexts[j], mhWpnDmgMin, mhWpnDmgMax, mhWpnSpeed, mhWpnSkill,
                        ohWpnDmgMin, ohWpnDmgMax, ohWpnSpeed, ohWpnSkill,
                        str, agi, AP,
                        hit, crit + bonusCrit, haste, armor, level,
                        option_dual_wield, option_combat_log,
                        duration, executeDuration,
                        itemListMH, itemListOH);
                        contexts[j].ThreadedSimulate(duration);
                    }
                    for (int j = 0; j < numThreads; j++)
                    {
                        contexts[j].Join();
                        totalDamageCrit += contexts[j].Simulator.PlayerOne.totalDamage;
                    }
                }

                //Bonus Haste
                for (int i = 0; i < iterations; i++)
                {
                    for (int j = 0; j < numThreads; j++)
                    {
                        InitSimWithBuffs(contexts[j], mhWpnDmgMin, mhWpnDmgMax, mhWpnSpeed, mhWpnSkill,
                        ohWpnDmgMin, ohWpnDmgMax, ohWpnSpeed, ohWpnSkill,
                        str, agi, AP,
                        hit, crit, haste + bonusHaste, armor, level,
                        option_dual_wield, option_combat_log,
                        duration, executeDuration,
                        itemListMH, itemListOH);
                        contexts[j].ThreadedSimulate(duration);
                    }
                    for (int j = 0; j < numThreads; j++)
                    {
                        contexts[j].Join();
                        totalDamageHaste += contexts[j].Simulator.PlayerOne.totalDamage;
                    }
                }
                
                double totalDuration = duration * iterations * numThreads;
                double baseDPS = totalDamageBase / totalDuration;
                double apDPS = totalDamageAP / totalDuration;
                double hitDPS = totalDamageHit / totalDuration;
                double critDPS = totalDamageCrit / totalDuration;
                double hasteDPS = totalDamageHaste / totalDuration;
                tbOutput.AppendText(String.Format("Base = {0:F2}\n", baseDPS));
                tbOutput.AppendText(String.Format("AP = {0:F2}\n", apDPS));
                tbOutput.AppendText(String.Format("Hit = {0:F2}\n", hitDPS));
                tbOutput.AppendText(String.Format("Crit = {0:F2}\n", critDPS));
                tbOutput.AppendText(String.Format("Haste = {0:F2}\n", hasteDPS));

                double apBonusDPS = apDPS - baseDPS;
                double hitBonusDPS = hitDPS - baseDPS;
                double critBonusDPS = critDPS - baseDPS;
                double hasteBonusDPS = hasteDPS - baseDPS;

                double hitWeight = (hitBonusDPS * bonusAP) / (apBonusDPS * bonusHit * 100); //(critBonusDPS / bonusCrit) / (apBonusDPS / bonusAP)
                double critWeight = (critBonusDPS * bonusAP) / (apBonusDPS * bonusCrit * 100); //(critBonusDPS / bonusCrit) / (apBonusDPS / bonusAP)
                double hasteWeight = (hasteBonusDPS * bonusAP) / (apBonusDPS * bonusHaste * 100); //(critBonusDPS / bonusCrit) / (apBonusDPS / bonusAP)
                tbOutput.AppendText(String.Format("1 Hit = {0:F2} AP\n", hitWeight));
                tbOutput.AppendText(String.Format("1 Crit = {0:F2} AP\n", critWeight));
                tbOutput.AppendText(String.Format("1 Haste = {0:F2} AP\n", hasteWeight));

            }
            else if (cmbSimulationMode.Text == "Accurate DPS")
            {
                int numThreads = 6; //TODO add config for number of threads
                int iterations = (int)Math.Ceiling(numSimulations / numThreads);
                double totalDamage = 0, totalRage = 0, wastedRage = 0;

                ThreadedSimulationContext context = new ThreadedSimulationContext();
                InitSimWithBuffs(context, mhWpnDmgMin, mhWpnDmgMax, mhWpnSpeed, mhWpnSkill,
                    ohWpnDmgMin, ohWpnDmgMax, ohWpnSpeed, ohWpnSkill,
                    str, agi, AP,
                    hit, crit, haste, armor, level,
                    option_dual_wield, option_combat_log,
                    duration, executeDuration,
                    itemListMH, itemListOH);
                PlayerState player = context.Simulator.PlayerOne;
                tbOutput.AppendText("MH Attack Table:\n");
                tbOutput.AppendText(String.Format("Crit: {0:P2}\n", 1 - player.mhWhiteCritThreshold));
                tbOutput.AppendText(String.Format("Hit: {0:P2}\n", player.mhWhiteCritThreshold - player.mhWhiteHitThreshold));
                tbOutput.AppendText(String.Format("Miss: {0:P2}\n", player.mhWhiteHitThreshold - player.mhWhiteMissThreshold));
                tbOutput.AppendText(String.Format("Glance: {0:P2}\n", player.mhWhiteMissThreshold - player.mhWhiteGlanceThreshold));
                tbOutput.AppendText(String.Format("Dodge: {0:P2}\n", player.mhWhiteGlanceThreshold));
                tbOutput.AppendText("OH Attack Table:\n");
                tbOutput.AppendText(String.Format("Crit: {0:P2}\n", 1 - player.ohWhiteCritThreshold));
                tbOutput.AppendText(String.Format("Hit: {0:P2}\n", player.ohWhiteCritThreshold - player.ohWhiteHitThreshold));
                tbOutput.AppendText(String.Format("Miss: {0:P2}\n", player.ohWhiteHitThreshold - player.ohWhiteMissThreshold));
                tbOutput.AppendText(String.Format("Glance: {0:P2}\n", player.ohWhiteMissThreshold - player.ohWhiteGlanceThreshold));
                tbOutput.AppendText(String.Format("Dodge: {0:P2}\n", player.ohWhiteGlanceThreshold));
                tbOutput.AppendText("Yellow Attack Table:\n");
                tbOutput.AppendText(String.Format("Crit: {0:P2}\n", 1 - player.yellowCritThreshold));
                tbOutput.AppendText(String.Format("Hit: {0:P2}\n", player.yellowCritThreshold - player.yellowHitThreshold));
                tbOutput.AppendText(String.Format("Miss: {0:P2}\n", player.yellowHitThreshold - player.yellowMissThreshold));
                tbOutput.AppendText(String.Format("Dodge: {0:P2}\n", player.yellowMissThreshold));

                ThreadedSimulationContext[] contexts = new ThreadedSimulationContext[numThreads];
                for (int i = 0; i < numThreads; i++)
                {
                    contexts[i] = new ThreadedSimulationContext();
                }
                for (int i = 0; i < iterations; i++)
                {
                    for (int j = 0; j < numThreads; j++)
                    {
                        InitSimWithBuffs(contexts[j], mhWpnDmgMin, mhWpnDmgMax, mhWpnSpeed, mhWpnSkill,
                            ohWpnDmgMin, ohWpnDmgMax, ohWpnSpeed, ohWpnSkill,
                            str, agi, AP,
                            hit, crit, haste, armor, level,
                            option_dual_wield, option_combat_log,
                            duration, executeDuration,
                            itemListMH, itemListOH);
                        contexts[j].ThreadedSimulate(duration);
                    }
                    for (int j = 0; j < numThreads; j++)
                    {
                        contexts[j].Join();
                        totalDamage += contexts[j].Simulator.PlayerOne.totalDamage;
                        totalRage += contexts[j].Simulator.PlayerOne.totalRage;
                        wastedRage += contexts[j].Simulator.PlayerOne.wastedRage;
                    }
                }

                double totalDuration = duration * iterations * numThreads;
                double DPS = totalDamage / totalDuration;
                tbOutput.AppendText(String.Format("DPS: {0:F2}\n", DPS));
                tbOutput.AppendText(String.Format("Effec. RPS: {0:F2}\n", (totalRage - wastedRage) / totalDuration));
                tbOutput.AppendText(String.Format("Wasted RPS: {0:F2}\n", wastedRage / totalDuration));
                tbOutput.AppendText(String.Format("Total  RPS: {0:F2}\n", totalRage / totalDuration));
            }
            else
            {
                ThreadedSimulationContext context = new ThreadedSimulationContext();
                InitSimWithBuffs(context, mhWpnDmgMin, mhWpnDmgMax, mhWpnSpeed, mhWpnSkill,
                    ohWpnDmgMin, ohWpnDmgMax, ohWpnSpeed, ohWpnSkill,
                    str, agi, AP,
                    hit, crit, haste, armor, level,
                    option_dual_wield, option_combat_log,
                    duration, executeDuration,
                    itemListMH, itemListOH);
                PlayerState player = context.Simulator.PlayerOne;
                tbOutput.AppendText("MH Attack Table:\n");
                tbOutput.AppendText(String.Format("Crit: {0:P2}\n", 1 - player.mhWhiteCritThreshold));
                tbOutput.AppendText(String.Format("Hit: {0:P2}\n", player.mhWhiteCritThreshold - player.mhWhiteHitThreshold));
                tbOutput.AppendText(String.Format("Miss: {0:P2}\n", player.mhWhiteHitThreshold - player.mhWhiteMissThreshold));
                tbOutput.AppendText(String.Format("Glance: {0:P2}\n", player.mhWhiteMissThreshold - player.mhWhiteGlanceThreshold));
                tbOutput.AppendText(String.Format("Dodge: {0:P2}\n", player.mhWhiteGlanceThreshold));
                tbOutput.AppendText("OH Attack Table:\n");
                tbOutput.AppendText(String.Format("Crit: {0:P2}\n", 1 - player.ohWhiteCritThreshold));
                tbOutput.AppendText(String.Format("Hit: {0:P2}\n", player.ohWhiteCritThreshold - player.ohWhiteHitThreshold));
                tbOutput.AppendText(String.Format("Miss: {0:P2}\n", player.ohWhiteHitThreshold - player.ohWhiteMissThreshold));
                tbOutput.AppendText(String.Format("Glance: {0:P2}\n", player.ohWhiteMissThreshold - player.ohWhiteGlanceThreshold));
                tbOutput.AppendText(String.Format("Dodge: {0:P2}\n", player.ohWhiteGlanceThreshold));
                tbOutput.AppendText("Yellow Attack Table:\n");
                tbOutput.AppendText(String.Format("Crit: {0:P2}\n", 1 - player.yellowCritThreshold));
                tbOutput.AppendText(String.Format("Hit: {0:P2}\n", player.yellowCritThreshold - player.yellowHitThreshold));
                tbOutput.AppendText(String.Format("Miss: {0:P2}\n", player.yellowHitThreshold - player.yellowMissThreshold));
                tbOutput.AppendText(String.Format("Dodge: {0:P2}\n", player.yellowMissThreshold));

                context.ThreadedSimulate(duration);
                context.Join();
                tbOutput.AppendText(String.Format("Duration: {0:F2}\n", duration));
                tbOutput.AppendText(String.Format("Damage: {0:F2}\n", player.totalDamage));
                tbOutput.AppendText(String.Format("DPS: {0:F2}\n", player.totalDamage / duration));
                tbOutput.AppendText(String.Format("Effec. RPS: {0:F2}\n", (player.totalRage - player.wastedRage) / duration));
                tbOutput.AppendText(String.Format("Wasted RPS: {0:F2}\n", player.wastedRage / duration));
                tbOutput.AppendText(String.Format("Total  RPS: {0:F2}\n", player.totalRage / duration));
                if (option_compare_ironfoe)
                {
                    double ironfoeDPS = player.totalDamage / duration;
                    double currentDPStry = 150;
                    double lastHighWeaponDPSTried = 150;
                    double lastLowWeaponDPSTried = 50;
                    double iterations = 1;
                    while (true)
                    {
                        double baseDPS = currentDPStry - 55 / 2.8;
                        InitSimWithBuffs(context, baseDPS * 2.8, baseDPS * 2.8 + 110, 2.8, mhWpnSkill,
                        ohWpnDmgMin, ohWpnDmgMax, ohWpnSpeed, ohWpnSkill,
                        str, agi, AP,
                        hit, crit, haste, armor, level,
                        option_dual_wield, option_combat_log,
                        duration, executeDuration,
                        itemListMH, itemListOH);
                        context.ThreadedSimulate(duration);
                        context.Join();
                        double tryDPS = (player.totalDamage / duration);
                        if (tryDPS - ironfoeDPS > 1)
                        {
                            lastHighWeaponDPSTried = currentDPStry;
                            currentDPStry = (lastHighWeaponDPSTried + lastLowWeaponDPSTried) / 2;
                        }
                        else if (tryDPS - ironfoeDPS < -1)
                        {
                            lastLowWeaponDPSTried = currentDPStry;
                            currentDPStry = (lastHighWeaponDPSTried + lastLowWeaponDPSTried) / 2;
                        }
                        else
                        {
                            break;
                        }
                        iterations++;
                    }
                    tbOutput.AppendText(String.Format("Ironfoe equivalent to {0:F2} DPS MSA\n", currentDPStry));
                }
                if(option_buff_uptime)
                {
                    tbOutput.AppendText("Buff Uptimes:\n");
                    if ((string)((DataRowView)gearBoxDict1["MeleeMH"].SelectedItem).Row["Name"] == "Bonereaver's Edge")
                    {
                        tbOutput.AppendText(String.Format("BRE Avg Stacks: {0:F2}\n", ((BonereaversEdgeBuff)player.Buffs.BonereaversEdge).breWeightedUptime / duration));
                    }
                    tbOutput.AppendText(String.Format("MH Crusader: {0:F2}%\n", player.Buffs.MHCrusader.Uptime * 100 / duration));
                    tbOutput.AppendText(String.Format("OH Crusader: {0:F2}%\n", player.Buffs.OHCrusader.Uptime * 100 / duration));
                    tbOutput.AppendText(String.Format("Flurry: {0:F2}%\n", player.Buffs.Flurry.Uptime * 100 / duration));
                    tbOutput.AppendText(String.Format("Empyrean Demolisher: {0:F2}%\n", player.Buffs.EmpyreanDemolisher.Uptime * 100 / duration));
                    tbOutput.AppendText(String.Format("Eskhandar's Right Claw: {0:F2}%\n", player.Buffs.EskhandarsRightClaw.Uptime * 100 / duration));
                    tbOutput.AppendText(String.Format("The Untamed Blade: {0:F2}%\n", player.Buffs.UntamedFury.Uptime * 100 / duration));
                }
            }
            tbOutput.AppendText("===========================");
            btnGo.Enabled = true;
        }

        private void cbDualWield_CheckedChanged(object sender, EventArgs e)
        {
            if(cbDualWield.Checked)
            {
                lblMeleeOH.Visible = true;
                cmbMeleeOH1.Visible = true;
                cmbMeleeMH1.BindingContext = new BindingContext();
                cmbMeleeMH1.DataSource = GearDataSet.Tables["DW Weapons"].DefaultView;
                cmbMeleeMH1.DisplayMember = "Name";
            }
            else
            {
                lblMeleeOH.Visible = false;
                cmbMeleeOH1.Visible = false;
                cmbMeleeMH1.BindingContext = new BindingContext();
                cmbMeleeMH1.DataSource = GearDataSet.Tables["2H Weapons"].DefaultView;
                cmbMeleeMH1.DisplayMember = "Name";
            }
        }

        private void cmbGear1_SelectedChanged(object sender, EventArgs e)
        {
            RecalculateTooltips1();
        }

        private void RecalculateTooltips1()
        {
            //TODO get all race + class specific base values
            haste = 1;
            crit = 0; hit = 0;
            str = _str;
            agi = _agi;
            AP = _AP;
            mhWpnSkill = 300;
            ohWpnSkill = 300;
            DataRow row;

            row = ((DataRowView)gearBoxDict1["MeleeMH"].SelectedItem).Row;
            double meleeMhMin = mhWpnDmgMin = RowDouble(row, "Min Hit");
            double meleeMhMax = mhWpnDmgMax = RowDouble(row, "Max Hit");
            double meleeMhSpeed = mhWpnSpeed = RowDouble(row, "Speed");
            string meleeMhType = RowString(row, "Weapon Type");
            if (cmbRace.Text == "Human")
            {
                if (meleeMhType == "Mace" || meleeMhType == "Sword" || meleeMhType == "2H Mace" || meleeMhType == "2H Sword")
                {
                    mhWpnSkill += 5;
                }
            }
            else if (cmbRace.Text == "Orc")
            {
                if (meleeMhType == "Axe" || meleeMhType == "2H Axe")
                {
                    mhWpnSkill += 5;
                }
            }
            double mhProcPercent = RowDouble(row, "Proc%");
            if (mhProcPercent != 0)
            {
                lblWeapon1ProcPercent.Visible = true;
                tbWeapon1ProcPercent.Visible = true;
                tbWeapon1ProcPercent.Text = mhProcPercent.ToString();
            }
            else
            {
                lblWeapon1ProcPercent.Visible = false;
                tbWeapon1ProcPercent.Visible = false;
            }

            row = ((DataRowView)gearBoxDict1["MeleeOH"].SelectedItem).Row;
            double meleeOhMin = ohWpnDmgMin = RowDouble(row, "Min Hit");
            double meleeOhMax = ohWpnDmgMax = RowDouble(row, "Max Hit");
            double meleeOhSpeed = ohWpnSpeed = RowDouble(row, "Speed");
            string meleeOhType = RowString(row, "Weapon Type");
            if (cmbRace.Text == "Human")
            {
                if (meleeOhType == "Mace" || meleeOhType == "Sword" || meleeOhType == "2H Mace" || meleeOhType == "2H Sword")
                {
                    ohWpnSkill += 5;
                }
            }
            else if(cmbRace.Text == "Orc")
            {
                if (meleeOhType == "Axe" || meleeOhType == "2H Axe")
                {
                    ohWpnSkill += 5;
                }
            }
            double ohProcPercent = RowDouble(row, "Proc%");
            if (ohProcPercent != 0)
            {
                lblWeapon2ProcPercent.Visible = true;
                tbWeapon2ProcPercent.Visible = true;
                tbWeapon2ProcPercent.Text = ohProcPercent.ToString();
            }
            else
            {
                lblWeapon2ProcPercent.Visible = false;
                tbWeapon2ProcPercent.Visible = false;
            }

            row = ((DataRowView)gearBoxDict1["Trinket1"].SelectedItem).Row;
            double t1ProcPercent = RowDouble(row, "Proc%");
            if (t1ProcPercent != 0)
            {
                lblTrinket1ProcPercent.Visible = true;
                tbTrinket1ProcPercent.Visible = true;
                tbTrinket1ProcPercent.Text = t1ProcPercent.ToString();
            }
            else
            {
                lblTrinket1ProcPercent.Visible = false;
                tbTrinket1ProcPercent.Visible = false;
            }

            row = ((DataRowView)gearBoxDict1["Trinket2"].SelectedItem).Row;
            double t2ProcPercent = RowDouble(row, "Proc%");
            if (t2ProcPercent != 0)
            {
                lblTrinket2ProcPercent.Visible = true;
                tbTrinket2ProcPercent.Visible = true;
                tbTrinket2ProcPercent.Text = t2ProcPercent.ToString();
            }
            else
            {
                lblTrinket2ProcPercent.Visible = false;
                tbTrinket2ProcPercent.Visible = false;
            }

            foreach (KeyValuePair<string, ComboBox> kvp in gearBoxDict1)
            {
                row = ((DataRowView)kvp.Value.SelectedItem).Row;
                crit += RowDouble(row, "Crit") / 100;
                hit += RowDouble(row, "Hit") / 100;
                str += RowDouble(row, "Str");
                agi += RowDouble(row, "Agi");
                AP += RowDouble(row, "AP");
                double skill = RowDouble(row, "Skill");
                if (skill != 0)
                {
                    string[] rowType = RowString(row, "Weapon Type").Split('/');
                    if (rowType != null)
                    {
                        for (int i = 0; i < rowType.Length; i++)
                        {
                            if (rowType[i] == meleeMhType)
                            {
                                mhWpnSkill += skill;
                            }
                            if (rowType[i] == meleeOhType)
                            {
                                ohWpnSkill += skill;
                            }
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, ComboBox> kvp in enchantBoxDict1)
            {
                row = ((DataRowView)kvp.Value.SelectedItem).Row;
                crit += RowDouble(row, "Crit") / 100;
                str += RowDouble(row, "Str");
                agi += RowDouble(row, "Agi");
                AP += RowDouble(row, "AP");
                haste += RowDouble(row, "Haste") / 100;
                
            }
            
            if (cbRallyingCry1.Checked) { crit += 0.05; AP += 140; }
            if (cbSongflower1.Checked) { crit += 0.05; str += 15; agi += 15; }
            if (cbFengusFerocity1.Checked) { AP += 200; }
            if (cbLeaderOfThePack1.Checked) { crit += 0.03; }
            if (cbMarkOfTheWild1.Checked) { str += 16; agi += 16; } //TODO implement MOTW talent
            if (cbTrueshotAura1.Checked) { AP += 100; }
            if (cbBattleShout1.Checked) { AP += 290; }//241; } //TODO implement bshout talent, is this bugged on kronos? base is 193 instead of 185
            if (cbBlessingOfMight1.Checked) { AP += 186; } //TODO implement might talent?

            if (cbMongoose1.Checked) { crit += 0.02; agi += 25; }
            if (cbJujuPower1.Checked) { str += 30; }
            if (cbJujuMight1.Checked) { AP += 40; }
            if (cbDumplings1.Checked) { str += 20; }
            if (cbGrilledSquid1.Checked) { agi += 10; }
            if (cbROIDS1.Checked) { str += 25; }
            if (cbScorpok1.Checked) { agi += 25; }


            //Implemented as buffs in the simulation, implemented more simply here for display purposes
            double tooltipStr = str;
            double tooltipAgi = agi;
            if (cbBlessingOfKings1.Checked) { tooltipStr += str * 0.1; tooltipAgi += agi * 0.1; }
            if (cbZandalar1.Checked) { tooltipStr += str * 0.15; tooltipAgi += agi * 0.15; }

            double tooltipCritMH = crit + tooltipAgi / 2000 + (cbEleStoneMH1.Checked ? 0.02 : 0);
            tooltipCritMH += 0.08; //TODO correctly implement crit talent (assume zerker stance for tooltip)
            double tooltipCritOH = crit + tooltipAgi / 2000 + (cbEleStoneOH1.Checked ? 0.02 : 0);
            tooltipCritOH += 0.08; //TODO correctly implement crit talent (assume zerker stance for tooltip)
            double tooltipAP = AP + tooltipStr * 2;

            meleeMhMin += tooltipAP * meleeMhSpeed / 14;
            meleeMhMax += tooltipAP * meleeMhSpeed / 14;
            meleeOhMin += tooltipAP * meleeOhSpeed / 14;
            meleeOhMax += tooltipAP * meleeOhSpeed / 14;
            
            meleeOhMin *= 0.625; //TODO implement dw spec talent
            meleeOhMax *= 0.625;

            //sharpening stone not affected by offhand damage penalty
            if (cbDenseStoneMH1.Checked) { meleeMhMin += 8; meleeMhMax += 8; }
            if (cbDenseStoneOH1.Checked) { meleeOhMin += 8; meleeOhMax += 8; }

            lblStr1.Text = tooltipStr.ToString();
            lblAgi1.Text = tooltipAgi.ToString();
            lblAP1.Text = tooltipAP.ToString();
            lblMhCrit1.Text = tooltipCritMH.ToString();
            lblOhCrit1.Text = tooltipCritOH.ToString();
            lblHit1.Text = hit.ToString();
            lblHaste1.Text = (haste - 1).ToString();

            lblMhWpnDmg.Text = String.Format("{0:F2} - {1:F2}", meleeMhMin, meleeMhMax);
            lblOhWpnDmg.Text = String.Format("{0:F2} - {1:F2}", meleeOhMin, meleeOhMax);
            lblMhSpeed.Text = String.Format("{0:F2}", meleeMhSpeed);
            lblOhSpeed.Text = String.Format("{0:F2}", meleeOhSpeed);
            lblMhWpnType.Text = meleeMhType;
            lblOhWpnType.Text = meleeOhType;
            lblMhSkill.Text = mhWpnSkill.ToString();
            lblOhSkill.Text = ohWpnSkill.ToString();
        }

        void InitSimWithBuffs(ThreadedSimulationContext context, double i_mh_min, double i_mh_max, double i_mh_speed, double i_mh_skill,
            double i_oh_min, double i_oh_max, double i_oh_speed, double i_oh_skill,
            double i_str, double i_agi, double i_AP,
            double i_hit, double i_crit, double i_haste, double i_armor, double i_level, 
            bool i_dual_wield, bool i_logging,
            double i_duration, double i_executeDuration,
            List<ItemProc> i_mh_procs, List<ItemProc> i_oh_procs)
        {
            i_oh_min *= 0.625; //TODO correctly implement DW weapon spec
            i_oh_max *= 0.625;
            if (cbDenseStoneMH1.Checked) { i_mh_min += 8; i_mh_max += 8; }
            if (cbDenseStoneOH1.Checked) { i_oh_min += 8; i_oh_max += 8; }
            double critMH = i_crit;
            if (cbEleStoneMH1.Checked) { critMH += 0.02; }
            double critOH = i_crit;
            if (cbEleStoneOH1.Checked) { critOH += 0.02; }
            Globals.BOSS_DEFENSE = i_level * 5;
            context.Simulator.PlayerOne.Init(i_mh_min, i_mh_max, i_mh_speed, i_mh_skill,
                i_oh_min, i_oh_max, i_oh_speed, i_oh_skill,
                i_str, i_agi, i_AP,
                i_hit, critMH, critOH, i_haste,
                i_armor, i_dual_wield, i_logging,
                i_duration, i_executeDuration,
                i_mh_procs, i_oh_procs, cmbRotation1.Text);

            if (cbBlessingOfKings1.Checked) { context.Simulator.PlayerOne.Buffs.BlessingOfKings.Start(); }
            if (cbZandalar1.Checked) { context.Simulator.PlayerOne.Buffs.SpiritOfZandalar.Start(); }
        }

        double RowDouble(DataRow row, string column)
        {
            if (row.IsNull(column))
            {
                return 0;
            }
            else
            {
                return (double)row[column];
            }
        }

        string RowString(DataRow row, string column)
        {
            if (row.IsNull(column))
            {
                return String.Empty;
            }
            else
            {
                return (string)row[column];
            }
        }

        void FillGearTables(string fileName)
        {
            List<string> slots = new List<String>
                {"Head","Neck","Shoulder","Back","Chest",
                "Wrist","Hand","Waist","Legs","Feet","Ring",
                "Trinket","DW Weapons","2H Weapons","Ranged",
                "HeadEnchant","ShoulderEnchant","BackEnchant",
                "ChestEnchant","WristEnchant","HandEnchant",
                "LegsEnchant","FeetEnchant","MeleeEnchant"};
            using (OleDbConnection conn = this.returnConnection(fileName))
            {
                conn.Open();
                DataTable sheetData;
                // retrieve the data using data adapter
                foreach (string slot in slots)
                {
                    sheetData = new DataTable(slot);
                    OleDbDataAdapter sheetAdapter = new OleDbDataAdapter("select * from ["+slot+"$]", conn);
                    sheetAdapter.Fill(sheetData);
                    GearDataSet.Tables.Add(sheetData);
                }
            }
        }

        void PopulateGearComboBox(ComboBox box, string tableName)
        {
            box.BindingContext = new BindingContext();
            box.DataSource = GearDataSet.Tables[tableName].DefaultView;
            box.DisplayMember = "Name";
            box.SelectedIndexChanged += cmbGear1_SelectedChanged;
        }

        private OleDbConnection returnConnection(string fileName)
        {
            return new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + "; Jet OLEDB:Engine Type=5;Extended Properties=\"Excel 8.0; HDR=Yes\"");
        }

        private void DWForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                SaveSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Failed to Save Settings\n{0}", ex.Message));
            }
        }

        private void DWForm_Load(object sender, EventArgs e)
        {
            //To prevent annoying bug where the OLEDB Provider fails to correctly guess the column type,
            //The first row on each table should be completely filled with the correct types
            //See https://www.connectionstrings.com/microsoft-jet-ole-db-4-0/ , Ctrl-F TypeGuessRows
            try
            {
                FillGearTables("GearData.xls");
                PopulateGearComboBox(cmbHead1, "Head");
                PopulateGearComboBox(cmbNeck1, "Neck");
                PopulateGearComboBox(cmbShoulder1, "Shoulder");
                PopulateGearComboBox(cmbBack1, "Back");
                PopulateGearComboBox(cmbChest1, "Chest");
                PopulateGearComboBox(cmbWrist1, "Wrist");
                PopulateGearComboBox(cmbHand1, "Hand");
                PopulateGearComboBox(cmbWaist1, "Waist");
                PopulateGearComboBox(cmbLegs1, "Legs");
                PopulateGearComboBox(cmbFeet1, "Feet");
                PopulateGearComboBox(cmbRing11, "Ring");
                PopulateGearComboBox(cmbRing21, "Ring");
                PopulateGearComboBox(cmbTrinket11, "Trinket");
                PopulateGearComboBox(cmbTrinket21, "Trinket");
                PopulateGearComboBox(cmbMeleeMH1, "DW Weapons");
                PopulateGearComboBox(cmbMeleeOH1, "DW Weapons");
                PopulateGearComboBox(cmbRanged1, "Ranged");
                PopulateGearComboBox(cmbHeadEnchant1, "HeadEnchant");
                PopulateGearComboBox(cmbShoulderEnchant1, "ShoulderEnchant");
                PopulateGearComboBox(cmbBackEnchant1, "BackEnchant");
                PopulateGearComboBox(cmbChestEnchant1, "ChestEnchant");
                PopulateGearComboBox(cmbWristEnchant1, "WristEnchant");
                PopulateGearComboBox(cmbHandEnchant1, "HandEnchant");
                PopulateGearComboBox(cmbLegsEnchant1, "LegsEnchant");
                PopulateGearComboBox(cmbFeetEnchant1, "FeetEnchant");
                PopulateGearComboBox(cmbMeleeMHEnchant1, "MeleeEnchant");
                PopulateGearComboBox(cmbMeleeOHEnchant1, "MeleeEnchant");
            }
            catch(Exception ex)
            {
                MessageBox.Show(String.Format("Failed to load GearData.xls\n{0}", ex.Message));
            }
            cbRallyingCry1.CheckedChanged += cmbGear1_SelectedChanged;
            cbSongflower1.CheckedChanged += cmbGear1_SelectedChanged;
            cbZandalar1.CheckedChanged += cmbGear1_SelectedChanged;
            cbFengusFerocity1.CheckedChanged += cmbGear1_SelectedChanged;
            cbLeaderOfThePack1.CheckedChanged += cmbGear1_SelectedChanged;
            cbMarkOfTheWild1.CheckedChanged += cmbGear1_SelectedChanged;
            cbTrueshotAura1.CheckedChanged += cmbGear1_SelectedChanged;
            cbBattleShout1.CheckedChanged += cmbGear1_SelectedChanged;
            cbBlessingOfMight1.CheckedChanged += cmbGear1_SelectedChanged;
            cbBlessingOfKings1.CheckedChanged += cmbGear1_SelectedChanged;
            cbEleStoneMH1.CheckedChanged += cmbGear1_SelectedChanged;
            cbEleStoneOH1.CheckedChanged += cmbGear1_SelectedChanged;
            cbDenseStoneMH1.CheckedChanged += cmbGear1_SelectedChanged;
            cbDenseStoneOH1.CheckedChanged += cmbGear1_SelectedChanged;
            cbMongoose1.CheckedChanged += cmbGear1_SelectedChanged;
            cbJujuPower1.CheckedChanged += cmbGear1_SelectedChanged;
            cbJujuMight1.CheckedChanged += cmbGear1_SelectedChanged;
            cbDumplings1.CheckedChanged += cmbGear1_SelectedChanged;
            cbGrilledSquid1.CheckedChanged += cmbGear1_SelectedChanged;
            cbROIDS1.CheckedChanged += cmbGear1_SelectedChanged;
            cbScorpok1.CheckedChanged += cmbGear1_SelectedChanged;
            cmbRace.SelectedIndexChanged += cmbGear1_SelectedChanged;
            cbDualWield.CheckedChanged += cbDualWield_CheckedChanged;

            //PopulateGearComboBox(cmbHead2, "Head");
            //PopulateGearComboBox(cmbNeck2, "Neck");
            //PopulateGearComboBox(cmbShoulder2, "Shoulder");
            //PopulateGearComboBox(cmbBack2, "Back");
            //PopulateGearComboBox(cmbChest2, "Chest");
            //PopulateGearComboBox(cmbWrist2, "Wrist");
            //PopulateGearComboBox(cmbHand2, "Hand");
            //PopulateGearComboBox(cmbWaist2, "Waist");
            //PopulateGearComboBox(cmbLegs2, "Legs");
            //PopulateGearComboBox(cmbFeet2, "Feet");
            //PopulateGearComboBox(cmbRing12, "Ring");
            //PopulateGearComboBox(cmbRing22, "Ring");
            //PopulateGearComboBox(cmbTrinket12, "Trinket");
            //PopulateGearComboBox(cmbTrinket22, "Trinket");
            //PopulateGearComboBox(cmbMeleeMH2, "DW Weapons");
            //PopulateGearComboBox(cmbMeleeOH2, "DW Weapons");
            //PopulateGearComboBox(cmbRanged2, "Ranged");

            try
            {
                LoadSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Failed to Load Settings\n{0}", ex.Message));
            }
        }

        private void SaveSettings()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, ComboBox> kvp in gearBoxDict1)
            {
                ComboBox box = kvp.Value;
                sb.AppendFormat("{0},", kvp.Value.SelectedIndex);
            }
            foreach (KeyValuePair<string, ComboBox> kvp in enchantBoxDict1)
            {
                ComboBox box = kvp.Value;
                sb.AppendFormat("{0},", kvp.Value.SelectedIndex);
            }
            Properties.Settings.Default.ComboBoxIndexes = sb.ToString();
            Properties.Settings.Default.Save();
        }

        private void LoadSettings()
        {
            if (Properties.Settings.Default.NeedsUpgrade)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.NeedsUpgrade = false;
                Properties.Settings.Default.Save();
            }
            string[] indexes = Properties.Settings.Default.ComboBoxIndexes.Split(new char[]{ ',' }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            int count = indexes.Count();
            int idx;
            foreach (KeyValuePair<string, ComboBox> kvp in gearBoxDict1)
            {
                ComboBox box = kvp.Value;
                if (i > count || !int.TryParse(indexes[i], out idx))
                {
                    System.Windows.Forms.MessageBox.Show("Failed to load settings.");
                    return;
                }
                kvp.Value.SelectedIndex = idx;
                i++;
            }
            foreach (KeyValuePair<string, ComboBox> kvp in enchantBoxDict1)
            {
                ComboBox box = kvp.Value;
                if (i > count || !int.TryParse(indexes[i], out idx))
                {
                    System.Windows.Forms.MessageBox.Show("Failed to load settings.");
                    return;
                }
                kvp.Value.SelectedIndex = idx;
                i++;
            }
        }
    }
}
