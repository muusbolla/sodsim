using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class IronfoeProc : Proc
    {
        public IronfoeProc(PlayerState p) : base(p)
        {
        }
        public override void RollProc(bool isMainHand)
        {
            if (myPlayer.MyContext.RollChance(procChance))
            {
                myPlayer.Abilities.IronfoeHitMH.Do();
                myPlayer.Abilities.IronfoeHitMH.Do();
            }
        }
    }
}
