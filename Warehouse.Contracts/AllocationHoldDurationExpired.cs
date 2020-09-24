using System;
using System.Collections.Generic;
using System.Text;

namespace Warehouse.Contracts
{
    using System;

    public interface AllocationHoldDurationExpired
    {
        Guid AllocationId { get; }
    }
}
