using MassTransit;
using System;
using System.Threading.Tasks;
using Warehouse.Contracts;

namespace Warehouse.Components
{
    public class AllocateInventoryConsumer : IConsumer<AllocateInventory>
    {
        public async Task Consume(ConsumeContext<AllocateInventory> context)
        {
            await Task.Delay(500);

            await context.RespondAsync<InventoryAllocated>(
                new {
                    AllocationId = context.Message.AllocationId,
                    ItemNumber = context.Message.ItemNumber,
                    Quantity = context.Message.Quantity
                });
        }
    }
}
