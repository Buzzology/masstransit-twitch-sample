using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.Platform.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using Warehouse.Components;

namespace Warehouse.Startup
{
    public class WarehouseStartup :
        IPlatformStartup
    {
        public void ConfigureMassTransit(IServiceCollectionBusConfigurator cfg, IServiceCollection services)
        {
            cfg.AddConsumersFromNamespaceContaining<AllocateInventoryConsumer>();
            cfg.AddSagaStateMachine<AllocationStateMachine, AllocationState>(typeof(AllocateStateMachineDefinition))
                .MongoDbRepository(r =>
                {
                    r.Connection = "mongodb://root:example@mongo:27017";
                    r.DatabaseName = "allocations";
                });
        }

        public void ConfigureBus<TEndpointConfigurator>(IBusFactoryConfigurator<TEndpointConfigurator> configurator, IBusRegistrationContext context) where TEndpointConfigurator : IReceiveEndpointConfigurator
        {
            //throw new NotImplementedException();
        }
    }
}
