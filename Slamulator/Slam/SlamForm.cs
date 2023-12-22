using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Slamulator
{
    public partial class SlamForm : Form
    {
        public SlamForm()
        {
            InitializeComponent();
        }

        private void btnGo_Click(object sender, EventArgs e)
        {

            double wpnDmgMin, wpnDmgMax, wpnSpeed, baseAP;
            double tooltipHit, tooltipCrit, wpnSkill;
            double haste, armor, armorReduction;
            double duration;

            bool option_anger_management = cbAM.Checked;
            bool option_bonereavers_edge = cbBRE.Checked;
            bool option_hand_of_justice = cbHOJ.Checked;
            bool option_rage_dump = cbRageDump.Checked;
            bool option_combat_log = cbLog.Checked;
            bool option_calc_stat_weight = cbStatWeight.Checked;

            double.TryParse(tbWpnDmgMin.Text, out wpnDmgMin);
            double.TryParse(tbWpnDmgMax.Text, out wpnDmgMax);
            double.TryParse(tbWpnSpeed.Text, out wpnSpeed);
            double.TryParse(tbAP.Text, out baseAP);
            double.TryParse(tbHit.Text, out tooltipHit);
            tooltipHit /= 100;
            double.TryParse(tbCrit.Text, out tooltipCrit);
            tooltipCrit /= 100;
            wpnSkill = 0;
            double.TryParse(tbHaste.Text, out haste);
            haste = 1 + (haste / 100);

            double.TryParse(tbArmor.Text, out armor);
            double.TryParse(tbArmorReduction.Text, out armorReduction);
            armor = armor - armorReduction;

            double.TryParse(tbDuration.Text, out duration);

            SlamSimulator test = new SlamSimulator();
            test.InitSimulator(wpnDmgMin, wpnDmgMax, wpnSpeed, baseAP,
                tooltipHit, tooltipCrit, wpnSkill,
                haste, armor,
                option_anger_management, option_hand_of_justice, option_bonereavers_edge,
                option_rage_dump, option_combat_log);
            if(!option_calc_stat_weight)
            {
                test.Run(duration);
                tbOutput.AppendText(String.Format("Duration: {0:F2}\n", duration));
                tbOutput.AppendText(String.Format("Damage: {0:F2}\n", test.totalDamage));
                tbOutput.AppendText(String.Format("DPS: {0:F2}\n", test.totalDamage / duration));
                if (option_bonereavers_edge)
                {
                    tbOutput.AppendText(String.Format("BRE Avg Stacks: {0:F2}\n", test.breWeightedUptime));
                }
            }
            else
            {
                double totalDamageBase = 0;
                double totalDamageAP = 0;
                double totalDamageCrit = 0;
                double totalDamageHaste = 0;
                double bonusAP = 28;
                double bonusCrit = 0.01;
                double bonusHaste = 0.01;
                double iterations = 10;
                for(int i = 0; i < iterations; i++)
                {
                    test.Run(duration);
                    totalDamageBase += test.totalDamage;
                }

                test.InitSimulator(wpnDmgMin, wpnDmgMax, wpnSpeed, baseAP + bonusAP,
                tooltipHit, tooltipCrit, wpnSkill,
                haste, armor,
                option_anger_management, option_hand_of_justice, option_bonereavers_edge,
                option_rage_dump, option_combat_log);
                for (int i = 0; i < iterations; i++)
                {
                    test.Run(duration);
                    totalDamageAP += test.totalDamage;
                }

                test.InitSimulator(wpnDmgMin, wpnDmgMax, wpnSpeed, baseAP,
                tooltipHit, tooltipCrit + bonusCrit, wpnSkill,
                haste, armor,
                option_anger_management, option_hand_of_justice, option_bonereavers_edge,
                option_rage_dump, option_combat_log);
                for (int i = 0; i < iterations; i++)
                {
                    test.Run(duration);
                    totalDamageCrit += test.totalDamage;
                }

                test.InitSimulator(wpnDmgMin, wpnDmgMax, wpnSpeed, baseAP,
                tooltipHit, tooltipCrit, wpnSkill,
                haste + bonusHaste, armor,
                option_anger_management, option_hand_of_justice, option_bonereavers_edge,
                option_rage_dump, option_combat_log);
                for (int i = 0; i < iterations; i++)
                {
                    test.Run(duration);
                    totalDamageHaste += test.totalDamage;
                }
                double totalDuration = duration * iterations;
                double baseDPS = totalDamageBase / totalDuration;
                double apDPS = totalDamageAP / totalDuration;
                double critDPS = totalDamageCrit / totalDuration;
                double hasteDPS = totalDamageHaste / totalDuration;

                double apBonusDPS = apDPS - baseDPS;
                double critBonusDPS = critDPS - baseDPS;
                double hasteBonusDPS = hasteDPS - baseDPS;

                double critWeight = (critBonusDPS * bonusAP) / (apBonusDPS * bonusCrit * 100); //(critBonusDPS / bonusCrit) / (apBonusDPS / bonusAP)
                double hasteWeight = (hasteBonusDPS * bonusAP) / (apBonusDPS * bonusHaste * 100); //(critBonusDPS / bonusCrit) / (apBonusDPS / bonusAP)
                tbOutput.AppendText(String.Format("1 Crit = {0:F2} AP\n", critWeight));
                tbOutput.AppendText(String.Format("1 Haste = {0:F2} AP\n", hasteWeight));

            }
        }
    }
}
