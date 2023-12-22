using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class Buff
    {
        protected PlayerState myPlayer;
        protected bool isActive = false;
        protected double startTime = 0;
        protected double uptime = 0;
        protected Buff(PlayerState p)
        {
            myPlayer = p;
        }
        public virtual void Start() { }
        public virtual void ApplyBuff() { }
        public virtual void Expire() { }
        public bool IsActive { get { return isActive; } }
        public double Uptime { get { return uptime; } }
    }
}
