using DnsClient.Internal;
using MassTransit;
using MassTransit.Courier.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Components.Consumers
{
    public class RoutingSlipEventConsumer :
        IConsumer<RoutingSlipActivityCompleted>,
        IConsumer<RoutingSlipActivityFaulted>
    {
        private ILogger<RoutingSlipEventConsumer> _logger;

        public RoutingSlipEventConsumer(ILogger<RoutingSlipEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<RoutingSlipActivityCompleted> context)
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
                _logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"Routing Slip Activity Completed: {context.Message.TrackingNumber} {context.Message.ActivityName}");

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<RoutingSlipActivityFaulted> context)
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
                _logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, $"Routing Slip Activity Faulted: {context.Message.TrackingNumber} {context.Message.ExceptionInfo?.Message} {context.Message.ExceptionInfo?.StackTrace}");

            return Task.CompletedTask;
        }
    }
}
