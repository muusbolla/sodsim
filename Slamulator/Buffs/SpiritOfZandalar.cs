using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class SpiritOfZandalar : Buff
    {
        public LinkedListNode<Buff> BuffNode;
        public LinkedListNode<TimedAction> ExpireNode;
        public SpiritOfZandalar(PlayerState p) : base(p)
        {
            BuffNode = new LinkedListNode<Buff>(this);
            ExpireNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Expire));
        }
        public override void Start()
        {
            startTime = myPlayer.MyContext.Server.Time;
            if (!isActive)
            {
                myPlayer.AddSecondaryBuff(BuffNode);
                isActive = true;
            }
            //ExpireNode.Value.Time = startTime + 15; //never expire
            //Globals.Server.RequeueNode(ExpireNode);
        }

        public override void ApplyBuff() //called in PlayerState.AddBuff || UpdateStats
        {
            myPlayer.str += myPlayer.baseStr * 0.15;
            myPlayer.agi += myPlayer.baseAgi * 0.15;
        }

        public override void Expire() //called from server, therefore don't need to dequeue
        {
            myPlayer.RemoveSecondaryBuff(BuffNode);
            isActive = false;
        }
    }
}
