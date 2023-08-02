using System;
using System.Collections;
using System.Collections.Generic;

public class OrderedEvent
{
    public bool orderDirty = false; // Sort when it's time to invoke functions.
    public bool ListeningToEventHandler = false;
    public List<OrderedAction> Acts = new List<OrderedAction>();

    public void InvokeMethods()
    {
        if (orderDirty)
        {
            Acts.Sort();
            orderDirty = false;
        }

        int len = Acts.Count;
        for (int i = 0; i < len; ++i) { Acts[i]?.act(); }
    }

    public void Push(OrderedAction Act)
    {
        if (Act == null)
        {
            throw new ArgumentException("Function passed cannot be null!", nameof(Act));
        }
        orderDirty = Act.exeOrder != int.MaxValue;
    }

    // Pushes action onto the top of the invocation list.
    public void Push(Action Act, int exeOrder = int.MaxValue)
    {
        if (Act == null)
        {
            throw new ArgumentException("Function passed cannot be null!", nameof(Act));
        }

        OrderedAction oa = new OrderedAction(Act, exeOrder);
        Acts.Insert(0, oa);
        orderDirty = exeOrder != int.MaxValue;
    }

    // Adds to the end of the invocation list
    public void Add(Action Act, int exeOrder = int.MaxValue)
    {
        if (Act == null)
        {
            throw new ArgumentException("Function passed cannot be null!", nameof(Act));
        }

        OrderedAction oa = new OrderedAction(Act, exeOrder);
        Acts.Add(oa);
        orderDirty = exeOrder != int.MaxValue;
    }

    public void Add(OrderedAction Oact)
    {
        if(Oact == null)
        {
            throw new ArgumentException("OrderedAction cannot be null!", nameof(Oact));
        }
        if(Oact.act == null)
        {
            throw new ArgumentException("OrderedAction function cannot be null!", nameof(Oact.act));
        }

        Acts.Add(Oact);
        orderDirty = Oact.exeOrder != int.MaxValue;
    }

}
