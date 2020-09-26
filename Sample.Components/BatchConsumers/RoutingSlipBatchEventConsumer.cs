using MassTransit;
using MassTransit.Courier.Contracts;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.Components.BatchConsumers
{
    public class RoutingSlipBatchEventConsumer : IConsumer<Batch<RoutingSlipCompleted>>
    {
        private ILogger<Batch<RoutingSlipCompleted>> _logger;

        public RoutingSlipBatchEventConsumer(ILogger<Batch<RoutingSlipCompleted>> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<Batch<RoutingSlipCompleted>> context)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.Log(LogLevel.Information, $"Routing Slips Completed: {string.Join(", ", context.Message.Select(x => x.Message.TrackingNumber))}");

            return Task.CompletedTask;
        }
    }
}
