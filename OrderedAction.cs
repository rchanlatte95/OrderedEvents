using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class OrderedAction : IComparable<OrderedAction>
{
    public Action? act = null;
    public int exeOrder = int.MaxValue;

    public int CompareTo(OrderedAction oa)
    {
        if (this.exeOrder > oa.exeOrder) return 1;
        if (this.exeOrder < oa.exeOrder) return -1;
        return 0;
    }

    public OrderedAction(Action func, int orderOfExe)
    {
        this.act = func;
        this.exeOrder = orderOfExe;
    }

    public OrderedAction(Action func)
    {
        this.act = func;
        this.exeOrder = int.MaxValue;
    }

    public OrderedAction() { act = null; exeOrder = int.MaxValue; }
}
