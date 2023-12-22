﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class DeathWishBuff : Buff
    {
        public LinkedListNode<Buff> BuffNode;
        public LinkedListNode<TimedAction> ExpireNode;
        public DeathWishBuff(PlayerState p) : base(p)
        {
            BuffNode = new LinkedListNode<Buff>(this);
            ExpireNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Expire));
        }
        public override void Start()
        {
            startTime = myPlayer.MyContext.Server.Time;
            if (!isActive)
            {
                myPlayer.AddPrimaryBuff(BuffNode);
                isActive = true;
            }
            ExpireNode.Value.Time = startTime + 30;
            myPlayer.MyContext.Server.RequeueNode(ExpireNode);
        }

        public override void ApplyBuff() //called in PlayerState.AddBuff || UpdateStats
        {
            myPlayer.damageMultiplier *= 1.2;
        }

        public override void Expire() //called from server, therefore don't need to dequeue
        {
            myPlayer.RemovePrimaryBuff(BuffNode);
            isActive = false;
        }
    }
}
