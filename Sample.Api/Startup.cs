using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.Components.Consumers;
using Sample.Contracts;
using MassTransit.Mediator;

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
            services.AddControllers();

            services.AddMassTransit(cfg => {
                cfg.AddConsumer<SubmitOrderConsumer>(typeof(SubmitOrderConsumerDefinition));
                cfg.AddRequestClient<SubmitOrder>();
                cfg.AddRequestClient<CheckOrder>();
                cfg.UsingRabbitMq();
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
