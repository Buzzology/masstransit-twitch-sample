using MassTransit;
using MassTransit.Courier;
using Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Components.Consumers
{
    public class FulfillOrderConsumer : IConsumer<FulfillOrder>
    {
        public async Task Consume(ConsumeContext<FulfillOrder> context)
        {
            var builder = new RoutingSlipBuilder(NewId.NextGuid());

            builder.AddActivity("AllocateInventory",
                new Uri("queue:AllocateInventory_execute"),
                new
                {
                    ItemNumber = "Item123",
                    Quantity = 10,
                });

            builder.AddActivity("PaymentActivity", new Uri("queue:Payment_execute"), new
            {
                CardNumber = "5999123456789",
                Amount = 99.95M
            });


            // If the transaction fails we want to send a message to the source address saying that it has failed
            await builder.AddSubscription(
                context.SourceAddress,
                MassTransit.Courier.Contracts.RoutingSlipEvents.Faulted,
                MassTransit.Courier.Contracts.RoutingSlipEventContents.None,
                x => x.Send<OrderFulfillmentFaulted>(new
                {
                    context.Message.OrderId,
                })
            );

            builder.AddVariable("OrderId", context.Message.OrderId);

            var routingSlip = builder.Build();

            await context.Execute(routingSlip);
        }
    }
}
