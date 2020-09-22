using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Contracts
{
    public interface FulfillOrder
    {
        Guid OrderId { get; }
        string CustomerNumber { get; }

    }
}
