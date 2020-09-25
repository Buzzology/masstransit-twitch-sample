using Automatonymous;
using MassTransit;
using Sample.Components.StateMachines.OrderStateMachineActivity;
using Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Components.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            // Use order id for correlation id
            Event(() => OrderSubmitted, x => x.CorrelateById(y => y.Message.OrderId));
            Event(() => OrderAccepted, x => x.CorrelateById(y => y.Message.OrderId));
            Event(() => FulfillmentFaulted, x => x.CorrelateById(y => y.Message.OrderId));
            Event(() => FulfillmentCompleted, x => x.CorrelateById(y => y.Message.OrderId));
            Event(() => OrderStatusRequested, x => {
                x.CorrelateById(y => y.Message.OrderId);
                x.OnMissingInstance(m => m.ExecuteAsync(async context =>
                {
                    if (context.RequestId.HasValue)
                    {
                        await context.RespondAsync<OrderNotFound>(new { context.Message.OrderId });
                    }
                }));
            });
            Event(() => AccountClosed, x => x.CorrelateBy((instance, context) => instance.CustomerNumber == context.Message.CustomerNumber));

            // Tell it how to retrieve the current state
            InstanceState(x => x.CurrentState);

            Initially(
                When(OrderSubmitted)
                    .Then(context => {
                        context.Instance.SubmitDate = context.Data.Timestamp;
                        context.Instance.CustomerNumber = context.Data.CustomerNumber;
                        context.Instance.Updated = DateTime.UtcNow;
                        context.Instance.PaymentCardNumber = context.Data.PaymentCardNumber;
                    })
                    .TransitionTo(Submitted)                
                );

            During(Accepted,
                When(FulfillmentFaulted)
                    .TransitionTo(Faulted),
                When(FulfillmentCompleted)
                    .TransitionTo(Completed)
                );

            During(Submitted,
                // When an order is already submitted, ignore it
                Ignore(OrderSubmitted),
                When(AccountClosed)
                    .TransitionTo(Cancelled),
                When(OrderAccepted)
                    .Activity(x => x.OfType<AcceptOrderActivity>())
                    .TransitionTo(Accepted)
            );

            // If receive a submit event out of order we'll just save the timestamp and customer number
            DuringAny(
                When(OrderSubmitted)
                .Then(context =>
                {
                    context.Instance.SubmitDate = context.Data.Timestamp;
                    context.Instance.CustomerNumber = context.Data.CustomerNumber;
                    context.Instance.PaymentCardNumber = context.Data.PaymentCardNumber;
                })
            );

            DuringAny(
                When(OrderStatusRequested)
                .RespondAsync(x =>
                {
                    return x.Init<OrderStatus>(new
                    {
                        OrderId = x.Instance.CorrelationId,
                        State = x.Instance.CurrentState,
                    });
                }));
        }

        public State Submitted { get; private set; }
        public State Cancelled { get; private set; }
        public State Accepted { get; private set; }
        public State Faulted { get; private set; }
        public State Completed { get; private set; }

        public Event<OrderSubmitted> OrderSubmitted { get; private set; }
        public Event<CheckOrder> OrderStatusRequested { get; private set; }
        public Event<CustomerAccountClosed> AccountClosed { get; private set; }
        public Event<OrderAccepted> OrderAccepted { get; private set; }
        public Event<OrderFulfillmentFaulted> FulfillmentFaulted { get; private set; }
        public Event<OrderFulfillmentCompleted> FulfillmentCompleted { get; private set; }
    }
}
