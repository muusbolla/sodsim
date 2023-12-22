using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class RecklessnessBuff : Buff
    {
        public LinkedListNode<Buff> BuffNode;
        public LinkedListNode<TimedAction> ExpireNode;
        public RecklessnessBuff(PlayerState p) : base(p)
        {
            BuffNode = new LinkedListNode<Buff>(this);
            ExpireNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Expire));
        }
        public override void Start()
        {
            startTime = myPlayer.MyContext.Server.Time;
            if (!isActive)
            {
                myPlayer.AddFinalBuff(BuffNode);
                isActive = true;
            }
            ExpireNode.Value.Time = startTime + 15;
            myPlayer.MyContext.Server.RequeueNode(ExpireNode);
        }

        public override void ApplyBuff() //called in PlayerState.AddBuff || UpdateStats
        {
            //TODO fix - you can glance and miss while recklessness is on
            double mhWpnSkillMod = (Globals.BOSS_DEFENSE - myPlayer.mhSkill) * 0.0004;
            double mhDodgeChance = 0.05 + mhWpnSkillMod;

            double ohWpnSkillMod = (Globals.BOSS_DEFENSE - myPlayer.ohSkill) * 0.0004;
            double ohDodgeChance = 0.05 + ohWpnSkillMod;
            myPlayer.mhWhiteGlanceThreshold = mhDodgeChance;
            myPlayer.mhWhiteMissThreshold = mhDodgeChance;
            myPlayer.mhWhiteHitThreshold = mhDodgeChance;
            myPlayer.mhWhiteCritThreshold = mhDodgeChance;

            myPlayer.yellowMissThreshold = mhDodgeChance;
            myPlayer.yellowHitThreshold = mhDodgeChance;
            myPlayer.yellowCritThreshold = mhDodgeChance;

            myPlayer.ohWhiteGlanceThreshold = ohDodgeChance;
            myPlayer.ohWhiteMissThreshold = ohDodgeChance;
            myPlayer.ohWhiteHitThreshold = ohDodgeChance;
            myPlayer.ohWhiteCritThreshold = ohDodgeChance;
        }

        public override void Expire() //called from server, therefore don't need to dequeue
        {
            myPlayer.RemoveFinalBuff(BuffNode);
            isActive = false;
        }
    }
}
