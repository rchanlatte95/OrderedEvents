namespace OrderedEvents
{
    public class OrderedAction<TEventArgs> : IComparable<OrderedAction<TEventArgs>>
    {
        public Action<object, TEventArgs>? Act = null;

        /// <summary>
        /// The event this action is subscribed too.
        /// </summary>
        public OrderedEvent<TEventArgs>? Subscription = null;

        private int exeOrder = OrderedEvent<TEventArgs>.DEFAULT_EXECUTION_POS;
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

        public OrderedAction(Action<object, TEventArgs> func, int orderOfExe, OrderedEvent<TEventArgs> subscribedToo)
        {
            Act = func;
            exeOrder = orderOfExe;
            Subscription = subscribedToo;
        }

        public OrderedAction(Action<object, TEventArgs> func, int orderOfExe)
        {
            Act = func;
            exeOrder = orderOfExe;
        }

        public OrderedAction(Action<object, TEventArgs> func)
        {
            Act = func;
        }

        public OrderedAction() { }
    }

    public class OrderedEvent<TEventArgs>
    {
        private const int INIT_ACT_CT = 32;
        public const int DEFAULT_EXECUTION_POS = 0;

        public bool executeEvents = true;
        public bool orderDirty = false; // Sort when event raised, before execution.

        public List<OrderedAction<TEventArgs>> Acts = new(INIT_ACT_CT);

        public OrderedEvent() { }

        public OrderedAction<TEventArgs>? GetOrderedAction(Action<object, TEventArgs> target)
        {
            Action<object, TEventArgs>? currAct;
            for (int i = 0, ct = Acts.Count; i < ct; ++i)
            {
                currAct = Acts[i].Act;
                if (currAct == null)
                {
                    throw new NullReferenceException($"OrderedAction cannot be null!");
                }

                if (currAct.Equals(target))
                {
                    return Acts[i];
                }
            }

            return null;
        }

        public void Sort()
        {
            Acts.Sort();
            orderDirty = false;
        }

        public void Raise(object obj, TEventArgs eventArgs)
        {
            if (executeEvents == false) return;

            if (orderDirty)
            {
                Acts.Sort();
                orderDirty = false;
            }

            int len = Acts.Count;
            OrderedAction<TEventArgs> action;
            for (int i = 0; i < len; ++i)
            {
                action = Acts[i];
                if (action == null || action.Act == null) continue;
                action.Act(obj, eventArgs);
            }
        }

        public void Push(OrderedAction<TEventArgs> OrdAct)
        {
            if (OrdAct == null)
            {
                throw new NullReferenceException($"OrderedAction cannot be null!");
            }

            OrdAct.Subscription = this;
            Acts.Insert(0, OrdAct);
        }

        public void Add(OrderedAction<TEventArgs> OrdAct)
        {
            if (OrdAct == null)
            {
                throw new NullReferenceException($"OrderedAction cannot be null!");
            }

            OrdAct.Subscription = this;
            Acts.Add(OrdAct);
        }

        public void Insert(OrderedAction<TEventArgs> OrdAct)
        {
            if (OrdAct == null)
            {
                throw new NullReferenceException($"OrderedAction passed cannot be null!");
            }

            Acts.Add(OrdAct);
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
                throw new NullReferenceException($"Action passed cannot be null!");
            }

            OrderedAction<TEventArgs> oa = new(Act, Acts[0].ExecutionOrder - 1, this);
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
        public void Add(Action<object, TEventArgs> Act)
        {
            if (Act == null)
            {
                throw new NullReferenceException($"Action passed cannot be null!");
            }

            OrderedAction<TEventArgs> oa = new(Act, DEFAULT_EXECUTION_POS, this);
            Acts.Add(oa);
        }

        /// <summary>
        ///
        /// Adds an action to be executed at the specified execution position.
        ///
        /// </summary>
        ///
        /// <param name="Act">Action you wish to execute when event is raised.</param>
        /// <param name="desiredExeOrder">Position of execution.</param>
        ///
        /// <exception cref="NullReferenceException">Throws if passed Action is null.</exception>
        public void Insert(Action<object, TEventArgs> Act, int desiredExeOrder)
        {
            if (Act == null)
            {
                throw new NullReferenceException($"Action passed cannot be null!");
            }

            OrderedAction<TEventArgs> oa = new(Act, desiredExeOrder, this);
            Acts.Add(oa);
            orderDirty = true;
        }

        public static OrderedEvent<TEventArgs> operator +(OrderedEvent<TEventArgs> lhs, OrderedAction<TEventArgs> rhs)
        {
            if (rhs == null)
            {
                throw new NullReferenceException($"OrderedAction passed cannot be null!");
            }

            lhs.Insert(rhs);
            return lhs;
        }

        public static OrderedEvent<TEventArgs> operator +(OrderedEvent<TEventArgs> lhs, Action<object, TEventArgs> rhs)
        {
            if (rhs == null)
            {
                throw new NullReferenceException($"Action passed cannot be null!");
            }

            lhs.Add(rhs);
            return lhs;
        }

        public static OrderedEvent<TEventArgs> operator -(OrderedEvent<TEventArgs> lhs, OrderedAction<TEventArgs> rhs)
        {
            if (rhs == null) { return lhs; }

            lhs.Acts.Remove(rhs);
            return lhs;
        }

        public static OrderedEvent<TEventArgs> operator -(OrderedEvent<TEventArgs> lhs, Action<object, TEventArgs> rhs)
        {
            if (rhs == null) { return lhs; }

            for(int i = 0; i < lhs.Acts.Count; ++i)
            {
                if (rhs.Equals(lhs.Acts[i].Act))
                {
                    lhs.Acts.RemoveAt(i);
                }
            }

            return lhs;
        }
    }
}
