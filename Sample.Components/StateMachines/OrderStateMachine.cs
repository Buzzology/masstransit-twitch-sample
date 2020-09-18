using Automatonymous;
using MassTransit;
using MassTransit.RedisIntegration;
using MassTransit.Saga;
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
            Event(() => OrderStatusRequested, x => {
                x.CorrelateById(y => y.Message.OrderId);
                x.OnMissingInstance(m => m.ExecuteAsync(async context =>
                {
                    if (context.RequestId.HasValue)
                    {
                        context.RespondAsync<OrderNotFound>(new { context.Message.OrderId });
                    }
                }));
            });

            // Tell it how to retrieve the current state
            InstanceState(x => x.CurrentState);

            Initially(
                When(OrderSubmitted)
                    .Then(context => {
                        context.Instance.SubmitDate = context.Data.Timestamp;
                        context.Instance.CustomerNumber = context.Data.CustomerNumber;
                        context.Instance.Updated = DateTime.UtcNow;
                    })
                    .TransitionTo(Submitted)
                );

            // When an order is already submitted, ignore it
            During(Submitted,
                Ignore(OrderSubmitted));

            // If receive a submit event out of order we'll just save the timestamp and customer number
            DuringAny(
                When(OrderSubmitted)
                .Then(context =>
                {
                    context.Instance.SubmitDate = context.Data.Timestamp;
                    context.Instance.CustomerNumber = context.Data.CustomerNumber;
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

        public Event<OrderSubmitted> OrderSubmitted { get; private set; }
        public Event<CheckOrder> OrderStatusRequested { get; private set; }
    }

    
    public class OrderState : SagaStateMachineInstance, ISagaVersion
    {
        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; }

        public string CustomerNumber { get; set; }

        public DateTime? Updated { get; set; }

        public DateTime? SubmitDate { get; set; }

        public int Version { get; set; } // Versioned saga is required for redis and requires version property
    }
}
