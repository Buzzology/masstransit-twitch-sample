using System;

namespace Sample.Contracts
{
    public interface OrderStatus
    {
        public Guid OrderId { get; }

        string State { get; }
    }
}
