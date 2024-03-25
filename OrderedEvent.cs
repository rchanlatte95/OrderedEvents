using System;
using System.Collections.Generic;

namespace OrderedEvents
{
    public class OrderedEvent
    {
        private const int INIT_ACT_CT = 32;
        public const int DEFAULT_EXECUTION_POS = 1024;

        public bool orderDirty = false; // Sort when event raised, before execution.
        public bool ListeningToEventHandler = false;
        public List<OrderedAction> Acts = new(INIT_ACT_CT);

        public void Raise()
        {
            if (orderDirty)
            {
                Acts.Sort();
                orderDirty = false;
            }

            int len = Acts.Count;
            OrderedAction action;
            for (int i = 0; i < len; ++i)
            {
                action = Acts[i];
                if (action == null || action.Act == null) continue;
                action.Act();
            }
        }

        public void Push(OrderedAction OrdAct)
        {
            if (OrdAct == null)
            {
                throw new NullReferenceException($"Ordered Action {nameof(OrdAct)} cannot be null!");
            }

            OrdAct.Subscription = this;
            Acts.Insert(0, OrdAct);
        }

        public void Add(OrderedAction OrdAct)
        {
            if (OrdAct == null)
            {
                throw new NullReferenceException($"Ordered Action {nameof(OrdAct)} cannot be null!");
            }

            OrdAct.Subscription = this;
            Acts.Add(OrdAct);
        }

        /// <summary>
        ///
        /// Pushes action onto the top of the execution stack.
        /// Will be executed first unless it is superceded after push.
        ///
        /// </summary>
        ///
        /// <param name="Act">Action you wish to execute when event is raised.</param>
        ///
        /// <exception cref="NullReferenceException">Throws if passed Action is null.</exception>
        public void Push(Action Act)
        {
            if (Act == null)
            {
                throw new NullReferenceException($"Action passed ({nameof(Act)}) cannot be null!");
            }

            OrderedAction oa = new(Act, Acts[0].exeOrder - 1, this);
            Acts.Insert(0, oa);
        }

        /// <summary>
        ///
        /// Adds action to the bottom of the execution stack.
        /// Will be executed last unless some other function is added after call.
        ///
        /// </summary>
        ///
        /// <param name="Act">Action you wish to execute when event is raised.</param>
        ///
        /// <exception cref="NullReferenceException">Throws if passed Action is null.</exception>
        public void Add(Action Act)
        {
            if (Act == null)
            {
                throw new NullReferenceException($"Action passed ({nameof(Act)}) cannot be null!");
            }

            OrderedAction oa = new(Act, DEFAULT_EXECUTION_POS, this);
            Acts.Add(oa);
        }

        public void Insert(OrderedAction OrdAct)
        {
            if (OrdAct == null)
            {
                throw new NullReferenceException($"OrderedAction passed ({nameof(OrdAct)}) cannot be null!");
            }

            Acts.Add(OrdAct);
            orderDirty = true;
        }

        public void Insert(Action Act, int desiredExeOrder)
        {
            if (Act == null)
            {
                throw new NullReferenceException($"Action passed ({nameof(Act)}) cannot be null!");
            }

            OrderedAction oa = new(Act, desiredExeOrder, this);
            Acts.Add(oa);
        }

        public static OrderedEvent operator +(OrderedEvent lhs, OrderedAction rhs)
        {
            lhs.Insert(rhs);
            return lhs;
        }

        public static OrderedEvent operator +(OrderedEvent lhs, Action rhs)
        {
            lhs.Add(rhs);
            return lhs;
        }
    }
}
