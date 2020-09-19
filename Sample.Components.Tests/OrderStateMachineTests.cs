using MassTransit;
using MassTransit.Clients;
using MassTransit.Testing;
using NuGet.Frameworks;
using NUnit.Framework;
using Sample.Components.StateMachines;
using Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Components.Tests
{
    [TestFixture]
    public class OrderStateMachineTests
    {
        [Test]
        public async Task Should_create_a_state_instance()
        {
            var orderStateMachine = new OrderStateMachine();
            var harness = new InMemoryTestHarness { TestTimeout = TimeSpan.FromSeconds(5) };
            var saga = harness.StateMachineSaga<OrderState, OrderStateMachine>(new OrderStateMachine());

            await harness.Start();

            try
            {
                var orderId = NewId.NextGuid();
                await harness.Bus.Publish<OrderSubmitted>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = "12345"
                });

                Assert.That(saga.Created.Select(x => x.CorrelationId == orderId).Any(), Is.True);

                // Wait for the saga instance to be created
                var instanceId = await saga.Exists(orderId, x => x.Submitted);
                Assert.That(instanceId, Is.Not.Null);

                var instance = saga.Sagas.Contains(instanceId.Value);

                Assert.That(instance.CustomerNumber == "12345");
            }
            finally
            {
                await harness.Stop();
            }
        }


        [Test]
        public async Task Should_respond_to_status_checks()
        {
            var orderStateMachine = new OrderStateMachine();
            var harness = new InMemoryTestHarness { TestTimeout = TimeSpan.FromSeconds(5) };
            var saga = harness.StateMachineSaga<OrderState, OrderStateMachine>(new OrderStateMachine());

            await harness.Start();

            try
            {
                var orderId = NewId.NextGuid();
                await harness.Bus.Publish<OrderSubmitted>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = "12345"
                });

                Assert.That(saga.Created.Select(x => x.CorrelationId == orderId).Any(), Is.True);

                // Wait for the saga instance to be created
                var instanceId = await saga.Exists(orderId, x => x.Submitted);
                Assert.That(instanceId, Is.Not.Null);

                var requestClient = await harness.ConnectRequestClient<CheckOrder>();
                var response = await requestClient.GetResponse<OrderStatus>(new { orderId });

                Assert.That(response.Message.State, Is.EqualTo(orderStateMachine.Submitted.Name));
            }
            finally
            {
                await harness.Stop();
            }
        }
    }
}
