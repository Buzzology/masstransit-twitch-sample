using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.MongoDbIntegration.MessageData;
using Microsoft.Extensions.DependencyInjection;
using Sample.Components.BatchConsumers;
using Sample.Components.Consumers;
using Sample.Components.CourierActivities;
using Sample.Components.StateMachines;
using Sample.Components.StateMachines.OrderStateMachineActivity;
using System;
using Warehouse.Contracts;

namespace Sample.Startup
{
    public class SampleStartupIPlatformStartup
    {
        public void ConfigureMassTransit(IServiceCollectionBusConfigurator cfg, IServiceCollection services)
        {
            // Manually added consumers, activities, etc. NOTE: Others will be added below via namespaces.
            services.AddScoped<AcceptOrderActivity>();
            services.AddScoped<RoutingSlipBatchEventConsumer>();
            
            cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
            cfg.AddActivitiesFromNamespaceContaining<AllocateInventoryActivity>();
            cfg.AddConsumer<RoutingSlipBatchEventConsumer>(x =>
            {
                x.Options<BatchOptions>(b => b.SetMessageLimit(10).SetTimeLimit(s: 1));
            });

            cfg.AddSagaStateMachine<OrderStateMachine, OrderState>(typeof(OrderStateMachineDefinition))
                .MongoDbRepository(r =>
                {
                    r.Connection = "mongodb://root:example@mongo:27017";
                    r.DatabaseName = "orderdb";
                });
            cfg.AddRequestClient<AllocateInventory>();
        }

        public void ConfigureBus<TEndpointConfigurator>(IBusFactoryConfigurator<TEndpointConfigurator> cfg, IBusRegistrationContext context) where TEndpointConfigurator : IReceiveEndpointConfigurator
        {
            cfg.UseMessageData(new MongoDbMessageDataRepository("mongodb://root:example@mongo:27017", "attachments"));

            // This can now be removed as it's set by an environment variable in docker-compose
            // cfg.UseMessageScheduler(new Uri("queue:quartz")); 
        }
    }
}
