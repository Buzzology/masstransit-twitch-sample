using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Contracts
{
    public interface OrderFulfillmentFaulted
    {
        Guid OrderId { get; }

        DateTime Timestamp { get; }
    }
}
