using Automatonymous;
using MassTransit.Saga;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Sample.Components.StateMachines
{
    public class OrderState : SagaStateMachineInstance, ISagaVersion
    {
        [BsonId]
        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; }

        public string CustomerNumber { get; set; }

        public DateTime? Updated { get; set; }

        public DateTime? SubmitDate { get; set; }

        public int Version { get; set; } // Versioned saga is required for redis and requires version property

        public string FaultReason { get; set; }
    }
}
