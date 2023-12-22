using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class BonereaversEdgeProc : Proc
    {
        public BonereaversEdgeProc(PlayerState p) : base(p)
        {
        }
        public override void RollProc(bool isMainHand)
        {
            if (myPlayer.MyContext.RollChance(procChance))
            {
                myPlayer.Buffs.BonereaversEdge.Start();
            }
        }
    }
}
