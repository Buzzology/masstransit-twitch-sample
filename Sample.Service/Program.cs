using MassTransit;
using MassTransit.Courier.Contracts;
using MassTransit.Definition;
using MassTransit.RabbitMqTransport.Topology.Entities;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.Components.BatchConsumers;
using Sample.Components.Consumers;
using Sample.Components.CourierActivities;
using Sample.Components.StateMachines;
using Sample.Components.StateMachines.OrderStateMachineActivity;
using Serilog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Warehouse.Contracts;

namespace Sample.Service
{
    class Program
    {
        static TelemetryClient _telemetryClient;
        static DependencyTrackingTelemetryModule _module;

        static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            // Add serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", true);
                    config.AddJsonFile("appsettings.Development.json", false);
                    config.AddEnvironmentVariables();

                    if (args != null) config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    #region Add telemetry

                    _module = new DependencyTrackingTelemetryModule();
                    _module.IncludeDiagnosticSourceActivities.Add("MassTransit");

                    TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
                    configuration.InstrumentationKey = hostContext.Configuration.GetSection("ApplicationInsights").GetValue<string>("InstrumentationKey");
                    configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());

                    _telemetryClient = new TelemetryClient(configuration);

                    _module.Initialize(configuration);

                    #endregion

                    // Manually added consumers, activities, etc. NOTE: Others will be added below via namespaces.
                    services.AddScoped<AcceptOrderActivity>();
                    services.AddScoped<RoutingSlipBatchEventConsumer>();

                    services.AddMassTransit(cfg =>
                    {
                        cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
                        cfg.AddActivitiesFromNamespaceContaining<AllocateInventoryActivity>();
                        cfg.AddSagaStateMachine<OrderStateMachine, OrderState>(typeof(OrderStateMachineDefinition))
                            .MongoDbRepository(r =>
                            {
                                r.Connection = "mongodb://root:example@localhost:27017";
                                r.DatabaseName = "orderdb";
                            }); 
                        cfg.AddBus(provider => ConfigureBus(provider));
                        cfg.AddRequestClient<AllocateInventory>();
                    });

                    services.AddHostedService<MassTransitConsoleHostedService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("logging"));
                    logging.AddSerilog(dispose: true);
                });

            if (isService)
                await builder.UseWindowsService().Build().RunAsync();
            else
                await builder.RunConsoleAsync();

            _telemetryClient?.Flush();
            _module?.Dispose();
        }

        static IBusControl ConfigureBus(IBusRegistrationContext provider)
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.UseMessageScheduler(new Uri("queue:quartz"));
                cfg.ReceiveEndpoint(DefaultEndpointNameFormatter.Instance.Consumer<RoutingSlipBatchEventConsumer>(), e =>
                {
                    e.PrefetchCount = 20;

                    e.Batch<RoutingSlipCompleted>(b =>
                    {
                        b.MessageLimit = 10;
                        b.TimeLimit = TimeSpan.FromSeconds(5);
                        b.Consumer<RoutingSlipBatchEventConsumer, RoutingSlipCompleted>(provider);
                    });
                });

                cfg.ConfigureEndpoints(provider);
            });
        }
    }
}
