using System;
using System.Collections.Generic;

namespace OrderedEvents
{
    public class OrderedAction<TEventArgs> : IComparable<OrderedAction<TEventArgs>>
    {
        public const int DEFAULT_EXECUTION_POS = OrderedEvent<TEventArgs>.DEFAULT_EXECUTION_POS;

        // The event this action is subscribed too.
        public OrderedEvent<TEventArgs>? Subscription = null;
        public Action<object, TEventArgs>? BaseAct = null;

        private int exeOrder = DEFAULT_EXECUTION_POS;
        public int ExecutionOrder
        {
            get => exeOrder;
            set
            {
                exeOrder = value;
                if (Subscription != null) { Subscription.orderDirty = true; }
            }
        }

        public int CompareTo(OrderedAction<TEventArgs>? ordAct)
        {
            if (ordAct == null) throw new NullReferenceException("Cannot compare an OrderedAction to null!");
            if (exeOrder > ordAct.ExecutionOrder) return 1;
            if (exeOrder < ordAct.ExecutionOrder) return -1;
            return 0;
        }

        public OrderedAction(Action<object, TEventArgs> func, OrderedEvent<TEventArgs> subscribedToo, int orderOfExe = DEFAULT_EXECUTION_POS)
        {
            BaseAct = func;
            Subscription = subscribedToo;
            exeOrder = orderOfExe;
        }

        public OrderedAction(Action<object, TEventArgs> func) { BaseAct = func; }

        public OrderedAction() { }
    }

    public class OrderedEvent<TEventArgs>
    {
        public const int DEFAULT_EXECUTION_POS = 0;

        public bool disabled = true;
        public bool orderDirty = false;

        public List<OrderedAction<TEventArgs>> OrderedActs = new(32);

        public OrderedEvent() { }

        public OrderedAction<TEventArgs>? GetOrderedAction(Action<object, TEventArgs> target)
        {
            Action<object, TEventArgs>? currAct;
            for (int i = 0, ct = OrderedActs.Count; i < ct; ++i)
            {
                currAct = OrderedActs[i].BaseAct;
                if (currAct == null)
                {
                    throw new NullReferenceException($"OrderedAction cannot be null!");
                }
                if (currAct.Equals(target)) { return OrderedActs[i]; }
            }
            return null;
        }

        public void Sort()
        {
            OrderedActs.Sort();
            orderDirty = false;
        }

        public void Raise(object obj, TEventArgs eventArgs)
        {
            if (disabled) return;
            if (orderDirty) { Sort(); }

            OrderedAction<TEventArgs> currAct;
            for (int i = 0, ct = OrderedActs.Count; i < ct; ++i)
            {
                currAct = OrderedActs[i];
                if (currAct == null || currAct.BaseAct == null) continue;
                currAct.BaseAct(obj, eventArgs);
            }
        }

        public void Push(OrderedAction<TEventArgs> OrdAct)
        {
            if (OrdAct == null)
            {
                throw new NullReferenceException($"OrderedAction cannot be null!");
            }
            OrdAct.Subscription = this;
            OrderedActs.Insert(0, OrdAct);
        }

        public void Add(OrderedAction<TEventArgs> OrdAct)
        {
            if (OrdAct == null)
            {
                throw new NullReferenceException($"OrderedAction cannot be null!");
            }
            OrdAct.Subscription = this;
            OrderedActs.Add(OrdAct);
        }

        public void Insert(OrderedAction<TEventArgs> OrdAct)
        {
            if (OrdAct == null)
            {
                throw new NullReferenceException($"OrderedAction cannot be null!");
            }
            OrderedActs.Add(OrdAct);
            orderDirty = true;
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
        public void Push(Action<object, TEventArgs> Act)
        {
            if (Act == null)
            {
                throw new NullReferenceException($"Action cannot be null!");
            }
            OrderedActs.Insert(0, new OrderedAction<TEventArgs>(Act, this, OrderedActs[0].ExecutionOrder - 1));
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
        public void Add(Action<object, TEventArgs> Act)
        {
            if (Act == null)
            {
                throw new NullReferenceException($"Action cannot be null!");
            }
            OrderedActs.Add(new OrderedAction<TEventArgs>(Act, this));
        }

        /// <summary>
        ///
        /// Adds an action to be executed at the specified execution position.
        ///
        /// </summary>
        ///
        /// <param name="Act">Action you wish to execute when event is raised.</param>
        /// <param name="desiredOrder">Position of execution.</param>
        ///
        /// <exception cref="NullReferenceException">Throws if passed Action is null.</exception>
        public void Insert(Action<object, TEventArgs> Act, int desiredOrder)
        {
            if (Act == null)
            {
                throw new NullReferenceException($"Action cannot be null!");
            }
            OrderedActs.Add(new OrderedAction<TEventArgs>(Act, this, desiredOrder));
            orderDirty = true;
        }

        public static OrderedEvent<TEventArgs> operator +(OrderedEvent<TEventArgs> lhs, OrderedAction<TEventArgs> rhs)
        {
            if (rhs == null)
            {
                throw new NullReferenceException($"OrderedAction cannot be null!");
            }
            lhs.Insert(rhs);
            return lhs;
        }

        public static OrderedEvent<TEventArgs> operator +(OrderedEvent<TEventArgs> lhs, Action<object, TEventArgs> rhs)
        {
            if (rhs == null)
            {
                throw new NullReferenceException($"Action cannot be null!");
            }
            lhs.Add(rhs);
            return lhs;
        }

        public static OrderedEvent<TEventArgs> operator -(OrderedEvent<TEventArgs> lhs, OrderedAction<TEventArgs> rhs)
        {
            if (rhs == null) { return lhs; }
            _ = lhs.OrderedActs.Remove(rhs);
            return lhs;
        }

        public static OrderedEvent<TEventArgs> operator -(OrderedEvent<TEventArgs> lhs, Action<object, TEventArgs> rhs)
        {
            if (rhs == null) { return lhs; }
            List<OrderedAction<TEventArgs>> acts = lhs.OrderedActs;
            for (int i = 0, ct = acts.Count; i < ct; ++i)
            {
                if (rhs.Equals(acts[i].BaseAct)) { acts.RemoveAt(i); break; }
            }
            return lhs;
        }
    }
}
