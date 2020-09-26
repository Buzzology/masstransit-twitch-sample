using Automatonymous;
using MassTransit;
using Sample.Components.StateMachines.OrderStateMachineActivity;
using Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sample.Components.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            // Use order id for correlation id
            Event(() => OrderSubmitted, x => x.CorrelateById(y => y.Message.OrderId));
            Event(() => OrderAccepted, x => {
                x.CorrelateById(y => y.Message.OrderId);
                x.OnMissingInstance(y => y.Execute(context => {
                    throw new InvalidOperationException($"This order cannot be accepted as it hasn't been received yet: {context.Message.OrderId}");
                }));
            });
            Event(() => FulfillmentFaulted, x => x.CorrelateById(y => y.Message.OrderId));
            Event(() => FulfillmentCompleted, x => x.CorrelateById(y => y.Message.OrderId));
            Event(() => FulfillOrderFaulted, x => x.CorrelateById(y => y.Message.Message.OrderId));
            Event(() => OrderStatusRequested, x => {
                x.CorrelateById(y => y.Message.OrderId);
                x.OnMissingInstance(m => m.ExecuteAsync(async context =>
                {
                    if (context.RequestId.HasValue)
                    {
                        Console.WriteLine($"Order not found: {context.Message.OrderId}");
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
                    .TransitionTo(Submitted),
                When(OrderAccepted)
                    .Then(context =>
                    {
                        throw new InvalidOperationException($"This order cannot be accepted as it hasn't been received yet: {context.Data.OrderId}");
                    }),
                When(OrderStatusRequested)
                    .Then(context =>
                    {
                        throw new InvalidOperationException($"This order cannot be retrieved as it hasn't been received yet: {context.Data.OrderId}");
                    })
                );

            During(Accepted,
                When(OrderSubmitted)
                    .Then(context =>
                    {
                        context.Instance.SubmitDate = context.Data.Timestamp;
                        context.Instance.CustomerNumber = context.Data.CustomerNumber;
                        context.Instance.Updated = DateTime.UtcNow;

                        if (string.IsNullOrWhiteSpace(context.Instance.PaymentCardNumber))
                        {
                            context.Instance.PaymentCardNumber = context.Data.PaymentCardNumber;
                        }
                    }),
                When(FulfillmentFaulted)
                    .TransitionTo(Faulted),
                When(FulfillOrderFaulted)
                    .TransitionTo(Faulted)
                    .Then(context => Console.WriteLine($"Fullfill order faulted: ${context.Data.Message.OrderId} ${context.Data?.Exceptions?.FirstOrDefault()?.Message}")),
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
        public Event<Fault<FulfillOrder>> FulfillOrderFaulted {get; private set; }
    }
}
