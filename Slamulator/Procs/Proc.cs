using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class Proc
    {
        protected PlayerState myPlayer;
        protected double procChance = 0;
        protected Proc(PlayerState p)
        {
            myPlayer = p;
        }
        public void SetProcChance(double i_procChance)
        {
            procChance = i_procChance;
        }
        public virtual void RollProc(bool isMainhand) { }
    }
}
