using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slamulator
{
    class ServerSide
    {
        LinkedList<TimedAction> actionList;
        public double Time;

        public ServerSide()
        {
            actionList = new LinkedList<TimedAction>();
        }

        public void DoNext()
        {
            Time = actionList.First.Value.Time;
            Action curr = actionList.First.Value.Action;
            actionList.RemoveFirst();
            curr.Invoke();
        }

        public void ResetServer()
        {
            Time = 0.0;
            actionList.Clear();
        }

        public void RequeueNode(LinkedListNode<TimedAction> newNode)
        {
            if (newNode.List != null)
            {
                actionList.Remove(newNode);
            }
            EnqueueNode(newNode);
        }

        public void EnqueueNode(LinkedListNode<TimedAction> newNode)
        {
            double t = newNode.Value.Time;
            LinkedListNode<TimedAction> insertNode = actionList.Last;
            while (insertNode != null && insertNode.Value.Time > t)
            {
                insertNode = insertNode.Previous;
            }
            if (insertNode == null)
            {
                actionList.AddFirst(newNode);
            }
            else
            {
                actionList.AddAfter(insertNode, newNode);
            }
        }

        public void RemoveNode(LinkedListNode<TimedAction> removeNode)
        {
            actionList.Remove(removeNode);
        }

        public void RemoveAllActionsOfTypeFromQueue(Action a)
        {
            LinkedListNode<TimedAction> removeNode = actionList.First;
            LinkedListNode<TimedAction> next;
            while (removeNode != null)
            {
                next = removeNode.Next;
                if (removeNode.Value.Action == a)
                {
                    actionList.Remove(removeNode);
                }
                removeNode = next;
            }
        }

        public void EnqueueAction(double t, Action a)
        {
            EnqueueNode(new LinkedListNode<TimedAction>(new TimedAction(t, a)));
        }
    }
}
