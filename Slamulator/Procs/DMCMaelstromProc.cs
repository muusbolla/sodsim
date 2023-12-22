using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class DMCMaelstromProc : Proc
    {
        public DMCMaelstromProc(PlayerState p) : base(p)
        {
        }
        public override void RollProc(bool isMainHand)
        {
            //TODO implement ppm -> proc chance in outer set function
            if (isMainHand)
            {
                if (myPlayer.MyContext.RollChance(2 * myPlayer.mhSpeedBase / 60)) //DMC: Maelstrom 2 ppm?
                {
                    myPlayer.Abilities.DMCMaelstrom.Do();
                }
            }
            else
            {
                if (myPlayer.MyContext.RollChance(2 * myPlayer.ohSpeedBase / 60))
                {
                    myPlayer.Abilities.DMCMaelstrom.Do();
                }
            }
        }
    }
}
