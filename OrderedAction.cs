using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class OrderedAction : IComparable<OrderedAction>
{
    public Action act = null;
    public int exeOrder = int.MaxValue;

    public int CompareTo(OrderedAction oa)
    {
        if (this.exeOrder > oa.exeOrder) return 1;
        if (this.exeOrder < oa.exeOrder) return -1;
        return 0;
    }
}
