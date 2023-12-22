using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class Crusader : Buff
    {
        public LinkedListNode<Buff> BuffNode;
        public LinkedListNode<TimedAction> ExpireNode;
        public Crusader(PlayerState p) : base(p)
        {
            BuffNode = new LinkedListNode<Buff>(this);
            ExpireNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Expire));
        }
        public override void Start()
        {
            double now = myPlayer.MyContext.Server.Time;
            if (!isActive)
            {
                startTime = now;
                myPlayer.AddPrimaryBuff(BuffNode);
                isActive = true;
            }
            ExpireNode.Value.Time = now + 15;
            myPlayer.MyContext.Server.RequeueNode(ExpireNode);
        }

        public override void ApplyBuff() //called in PlayerState.AddBuff || UpdateStats
        {
            myPlayer.baseStr += 100;
        }

        public override void Expire() //called from server, therefore don't need to dequeue
        {
            myPlayer.RemovePrimaryBuff(BuffNode);
            isActive = false;
            uptime += myPlayer.MyContext.Server.Time - startTime;
        }
    }
}
