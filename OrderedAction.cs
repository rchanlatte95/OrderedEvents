using System;

namespace OrderedEvents
{
    public class OrderedAction : IComparable<OrderedAction>
    {
        public Action? Act = null;
        public OrderedEvent? Subscription = null;
        public int exeOrder = OrderedEvent.DEFAULT_EXECUTION_POS;

        public bool ChangeExecutionOrder(int newExePos)
        {
            if (Subscription == null) return false;

            exeOrder = newExePos;
            Subscription.orderDirty = true;

            return true;
        }

        public int CompareTo(OrderedAction ordAct)
        {
            if (exeOrder > ordAct.exeOrder) return 1;
            if (exeOrder < ordAct.exeOrder) return -1;
            return 0;
        }

        public OrderedAction(Action func, int orderOfExe, OrderedEvent subscribedToo)
        {
            Act = func;
            exeOrder = orderOfExe;
            Subscription = subscribedToo;
        }

        public OrderedAction(Action func, int orderOfExe)
        {
            Act = func;
            exeOrder = orderOfExe;
        }

        public OrderedAction(Action func) { Act = func; }

        public OrderedAction() { }
    }
}
