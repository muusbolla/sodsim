using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class BonereaversEdgeBuff : Buff 
    {
        public LinkedListNode<Buff> BuffNode;
        public LinkedListNode<TimedAction> ExpireNode;

        private double breStacks = 0;
        private double breStacksCap = 1;
        public double breWeightedUptime = 0;
        public BonereaversEdgeBuff(PlayerState p) : base(p)
        {
            BuffNode = new LinkedListNode<Buff>(this);
            ExpireNode = new LinkedListNode<TimedAction>(new TimedAction(0.0, Expire));
        }
        public override void Start()
        {
            double now = myPlayer.MyContext.Server.Time;
            double bre_time = now - startTime;
            breWeightedUptime += bre_time * breStacks;

            startTime = now;
            breStacks = Math.Min(breStacksCap, breStacks + 1);
            if (!isActive)
            {
                myPlayer.AddPrimaryBuff(BuffNode);
                isActive = true;
            }
            ExpireNode.Value.Time = now + 10;
            myPlayer.MyContext.Server.RequeueNode(ExpireNode);
            if (myPlayer.logging) myPlayer.Log.Add(String.Format("BRE {0}", breStacks));
        }

        public override void ApplyBuff() //called in PlayerState.AddBuff || UpdateStats
        {
            myPlayer.armorPen += breStacks * 700;
        }

        public override void Expire() //called from server, therefore don't need to dequeue
        {
            double bre_time = 10;
            breWeightedUptime += bre_time * breStacks;
            breStacks = 0;
            myPlayer.RemovePrimaryBuff(BuffNode);
            isActive = false;
            if (myPlayer.logging) myPlayer.Log.Add(String.Format("BRE Expired", breStacks));
        }
    }
}
