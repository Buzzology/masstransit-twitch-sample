using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.Components.Consumers;
using Sample.Contracts;
using MassTransit.Mediator;
using Microsoft.ApplicationInsights.DependencyCollector;
using MassTransit.MongoDbIntegration.MessageData;
using System;

namespace Sample.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
            {
                module.IncludeDiagnosticSourceActivities.Add("MassTransit");
            });

            services.AddControllers();

            services.AddMassTransit(mt => {
                mt.AddConsumer<SubmitOrderConsumer>(typeof(SubmitOrderConsumerDefinition));
                mt.AddConsumer<FulfillOrderConsumer>(typeof(FulfillOrderConsumerDefinition));
                mt.AddRequestClient<SubmitOrder>();
                mt.AddRequestClient<CheckOrder>();
                mt.UsingRabbitMq((context, cfg) =>
                {
                    MessageDataDefaults.ExtraTimeToLive = TimeSpan.FromDays(1);
                    MessageDataDefaults.TimeToLive = TimeSpan.FromDays(7);
                    cfg.UseMessageData(new MongoDbMessageDataRepository("mongodb://root:example@localhost:27017", "attachments"));
                });
            });

            services.AddMassTransitHostedService();
            services.AddOpenApiDocument(cfg => cfg.PostProcess = d => d.Info.Title = "Sample API Site");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseOpenApi(); // Service OpenAPI/Swagger documents
            app.UseSwaggerUi3(); // Serve Swagger UI

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
