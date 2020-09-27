using Automatonymous;
using GreenPipes;
using MassTransit;
using Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Components.StateMachines.OrderStateMachineActivity
{
    public class AcceptOrderActivity : Activity<OrderState, OrderAccepted>
    {
        public AcceptOrderActivity()
        {
            // NOTE: If we wanted our activity to have dependencies we would create an activity like this. This would allow us to pull stuff in via DI. He emphasises that you should never try to do this via the state machine itself but break it out into an activity.
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<OrderState, OrderAccepted> context, Behavior<OrderState, OrderAccepted> next)
        {
            var consumeContext = context.GetPayload<ConsumeContext>();
            var sendEndpoint = await consumeContext.GetSendEndpoint(new Uri("queue:fulfill-order"));

            await sendEndpoint.Send<FulfillOrder>(new
            {
                context.Data.OrderId,
                CustomerNumber = context.Instance.CustomerNumber,
                PaymentCardNumber = context.Instance.PaymentCardNumber,
            });

            Console.WriteLine($"Hello, World. Order is ${context.Data.OrderId}.");
            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OrderState, OrderAccepted, TException> context, Behavior<OrderState, OrderAccepted> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("accept-order");
        }
    }
}
