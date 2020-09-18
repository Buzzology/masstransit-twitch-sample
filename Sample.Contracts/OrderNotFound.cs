using System;

namespace Sample.Contracts
{
    public interface OrderNotFound
    {
        public Guid OrderId { get; set; }
    }
}
