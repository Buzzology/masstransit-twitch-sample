using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Contracts
{
    public interface CheckOrder
    {
        public Guid OrderId { get; set; }
    }
}
