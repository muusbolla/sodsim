using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class Flurry : Buff
    {
        public LinkedListNode<Buff> BuffNode;
        public LinkedListNode<TimedAction> ExpireNode;
        public Flurry(PlayerState p) : base(p)
        {
            BuffNode = new LinkedListNode<Buff>(this);
            ExpireNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Expire));
        }
        public int Charges = 0;
        public override void Start()
        {
            double now = myPlayer.MyContext.Server.Time;
            Charges = 3;
            if(!isActive)
            {
                startTime = now;
                myPlayer.HasteHack();
                myPlayer.AddPrimaryBuff(BuffNode);
                myPlayer.HasteHack();
                isActive = true;
            }
            ExpireNode.Value.Time = now + 15;
            myPlayer.MyContext.Server.RequeueNode(ExpireNode);
        }
        
        public override void ApplyBuff() //called in PlayerState.AddBuff || UpdateStats
        {
            myPlayer.mhSpeed /= 1.3;
            myPlayer.ohSpeed /= 1.3;
        }

        public override void Expire() //called from server, therefore don't need to dequeue
        {
            myPlayer.HasteHack();
            myPlayer.RemovePrimaryBuff(BuffNode);
            myPlayer.HasteHack();
            isActive = false;
            Charges = 0;
            uptime += myPlayer.MyContext.Server.Time - startTime;
        }

        public void RemoveCharge() //called from WhiteHitMH or WhiteHitOH, so need to dequeue from server
        {
            Charges--;
            if(Charges == 0)
            {
                Expire();
                myPlayer.MyContext.Server.RemoveNode(ExpireNode);
            }
        }
    }
}
