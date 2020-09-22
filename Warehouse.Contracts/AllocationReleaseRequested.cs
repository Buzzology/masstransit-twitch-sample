using System;
using System.Collections.Generic;
using System.Text;

namespace Warehouse.Contracts
{
    public interface AllocationReleaseRequested
    {
        public Guid AllocationId { get; set; }
        public string Reason { get; set; }
    }
}
