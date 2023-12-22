using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class EmpyreanDemolisherProc : Proc
    {
        public EmpyreanDemolisherProc(PlayerState p) : base(p)
        {
        }
        public override void RollProc(bool isMainHand)
        {
            if (myPlayer.MyContext.RollChance(procChance))
            {
                myPlayer.Buffs.EmpyreanDemolisher.Start();
                if (myPlayer.logging) myPlayer.Log.Add("EMPYREAN DEMOLISHER");
            }
        }
    }
}
