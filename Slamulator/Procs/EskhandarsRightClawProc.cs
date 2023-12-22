using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class EskhandarsRightClawProc : Proc
    {
        public EskhandarsRightClawProc(PlayerState p) : base(p)
        {
        }
        public override void RollProc(bool isMainHand)
        {
            if (myPlayer.MyContext.RollChance(procChance))
            {
                myPlayer.Buffs.EskhandarsRightClaw.Start();
                if (myPlayer.logging) myPlayer.Log.Add("ESKHANDARS RIGHT CLAW");
            }
        }
    }
}
