using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class HeartOfWyrmthalakProc : Proc
    {
        public HeartOfWyrmthalakProc(PlayerState p) : base(p)
        {
        }
        public override void RollProc(bool isMainHand)
        {
            //TODO implement ppm -> proc chance in outer set function
            if (isMainHand)
            {
                if (myPlayer.MyContext.RollChance(3.4 * myPlayer.mhSpeedBase / 60)) //Heart of Wyrmthalak 4 ppm?
                {
                    myPlayer.Abilities.HeartOfWyrmthalak.Do();
                }
            }
            else
            {
                if (myPlayer.MyContext.RollChance(3.4 * myPlayer.ohSpeedBase / 60))
                {
                    myPlayer.Abilities.HeartOfWyrmthalak.Do();
                }
            }
        }
    }
}
