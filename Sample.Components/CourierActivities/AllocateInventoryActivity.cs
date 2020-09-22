using MassTransit;
using MassTransit.Clients;
using MassTransit.Courier;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Contracts;

namespace Sample.Components.CourierActivities
{
    public class AllocateInventoryActivity : IActivity<AllocateInventoryArguments, AllocateInventoryLog>
    {
        readonly IRequestClient<AllocateInventory> _client;

        public AllocateInventoryActivity(IRequestClient<AllocateInventory> client)
        {
            _client = client;
        }


        public async Task<CompensationResult> Compensate(CompensateContext<AllocateInventoryLog> context)
        {
            await context.Publish<AllocationReleaseRequested>(new
            {
                context.Log.AllocationId,
                Reason = "Order faulted",
            });

            return context.Compensated();
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<AllocateInventoryArguments> context)
        {
            var orderId = context.Arguments.OrderId;
            var itemNumber = context.Arguments.ItemNumber;
            var quantity = context.Arguments.Quantity;

            if (string.IsNullOrWhiteSpace(itemNumber)) throw new ArgumentNullException(nameof(itemNumber));
            if (quantity <= 0) throw new ArgumentNullException(nameof(quantity));

            var allocationId = NewId.NextGuid();
            await _client.GetResponse<InventoryAllocated>(new
            {
                AllocationId = allocationId,
                ItemNumber = itemNumber,
                Quantity = quantity
            });

            return context.Completed(new { AllocationId = allocationId });
        }
    }

    public interface AllocateInventoryArguments
    {
        Guid OrderId { get; set; }
        string ItemNumber { get; set; }
        decimal Quantity { get; set; }
    }


    public interface AllocateInventoryLog
    {
        Guid AllocationId { get; set; }
    }
}
