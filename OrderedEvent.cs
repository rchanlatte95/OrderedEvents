using System;
using System.Collections;
using System.Collections.Generic;

public class OrderedEvent
{
    public bool orderDirty = false; // Sort when it's time to invoke functions.
    public int reservedActCt = 0; // Number of actions to be executed during event.
    public bool ListeningToUE = false; // Listening to a UnityEvent
    public bool canAddActions { get => reservedActCt < Acts.Length; }

    public OrderedAction[] Acts;

    public void InvokeMethods()
    {
        if (orderDirty)
        {
            Array.Sort(Acts);
            orderDirty = false;
        }

        int len = System.Math.Min(Acts.Length, reservedActCt);
        for (int i = 0; i < len; ++i) { Acts[i].act.Invoke(); }
    }

    // Pushes action onto the top of the invocation list.
    public void Push(Action a)
    {
        if (canAddActions)
        {
            if (reservedActCt < 1)
            {
                Acts[0].act = a;
                Acts[0].exeOrder = int.MaxValue;
            }
            else if (reservedActCt == 1)
            {
                Acts[1].act = Acts[0].act;
                Acts[1].exeOrder = Acts[0].exeOrder;

                Acts[0].act = a;
                Acts[0].exeOrder = int.MaxValue;
            }
            else
            {
                // shift elements down one
                int i = reservedActCt;
                for (; i > 0; --i)
                {
                    // Cannot do a naive class copy since C# will bungle up
                    // the pointer indirection and cause duplicates.
                    Acts[i].act = Acts[i - 1].act;
                    Acts[i].exeOrder = Acts[i - 1].exeOrder;
                }

                Acts[0].act = a;
                Acts[0].exeOrder = int.MaxValue;
            }

            ++reservedActCt;
        }
    }

    // Adds to the end of the invocation list and raises flag for array sort
    // when it comes time to execute functions.
    public void Add(Action a, int exeOrder = int.MaxValue)
    {
        if (canAddActions)
        {
            Acts[reservedActCt].exeOrder = exeOrder;
            Acts[reservedActCt++].act = a;
            orderDirty = true;
        }
    }

    public OrderedEvent(int unityActionCt)
    {
        Acts = new OrderedAction[unityActionCt];
        for (int i = 0; i < unityActionCt; ++i) { Acts[i] = new OrderedAction(); }
    }
    public OrderedEvent()
    {
        Acts = new OrderedAction[8];
        Acts[0] = new OrderedAction(); Acts[1] = new OrderedAction();
        Acts[2] = new OrderedAction(); Acts[3] = new OrderedAction();
        Acts[4] = new OrderedAction(); Acts[5] = new OrderedAction();
        Acts[6] = new OrderedAction(); Acts[7] = new OrderedAction();
    }
}
