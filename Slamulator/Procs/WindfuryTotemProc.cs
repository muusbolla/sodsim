using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class WindfuryTotemProc : Proc
    {
        public WindfuryTotemProc(PlayerState p) : base(p)
        {
        }
        public override void RollProc(bool isMainHand)
        {
            if (myPlayer.MyContext.RollChance(procChance))
            {
                myPlayer.Abilities.WindfuryTotemHitMH.Do();
            }
        }
    }
}
