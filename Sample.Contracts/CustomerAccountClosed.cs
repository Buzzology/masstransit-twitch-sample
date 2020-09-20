using System;

namespace Sample.Contracts
{
    public interface CustomerAccountClosed
    {
        Guid CustomerId { get; }
        string CustomerNumber { get; }
    }
}
