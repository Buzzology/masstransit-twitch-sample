using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Contracts
{
    public interface OrderSubmissionRejected
    {
        Guid OrderId { get; }

        DateTime Timestamp { get; }

        string CustomerNumber { get; }

        string Reason { get; }
    }
}
