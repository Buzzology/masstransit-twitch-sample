using Automatonymous;
using MassTransit.Saga;
using System;

namespace Sample.Components.StateMachines
{
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
