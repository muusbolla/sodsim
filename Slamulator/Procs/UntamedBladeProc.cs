using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class CrusaderProc : Proc
    {
        public CrusaderProc(PlayerState p) : base(p)
        {
        }
        public override void RollProc(bool isMainHand)
        {
            if (isMainHand)
            { //TODO implement ppm -> proc chance in outer set function
                if (myPlayer.MyContext.RollChance(myPlayer.mhSpeedBase / 60)) //Crusader 1 ppm - https://vanilla-twinhead.twinstar.cz/?issue=718
                {
                    myPlayer.Buffs.MHCrusader.Start();
                    if (myPlayer.logging) myPlayer.Log.Add("MH CRUSADER");
                }
            }
            else
            {
                if (myPlayer.MyContext.RollChance(myPlayer.ohSpeedBase / 60))
                {
                    myPlayer.Buffs.OHCrusader.Start();
                    if (myPlayer.logging) myPlayer.Log.Add("OH CRUSADER");
                }
            }
        }
    }
}
