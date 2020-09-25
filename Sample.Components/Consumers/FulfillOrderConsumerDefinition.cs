using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;
using System;

namespace Sample.Components.Consumers
{
    public class FulfillOrderConsumerDefinition : ConsumerDefinition<FulfillOrderConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<FulfillOrderConsumer> consumerConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r =>
            {
                r.Ignore<InvalidOperationException>();
                r.Interval(3, 1000);
            });

            // NOTE: Uncommenting this will discard all fault messages for this consumer instead of sending them to the error queue
            // endpointConfigurator.DiscardFaultedMessages();
        }
    }
}
