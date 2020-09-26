using MassTransit;
using Microsoft.Extensions.Logging;
using Sample.Contracts;
using System.Threading.Tasks;

namespace Sample.Components.Consumers
{
    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        readonly ILogger<SubmitOrderConsumer> _logger;

        public SubmitOrderConsumer()
        {
        }

        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            _logger?.Log(LogLevel.Debug, $"SubmitOrderConsumer: {context.Message.CustomerNumber}");

            if (context.Message.CustomerNumber != null && context.Message.CustomerNumber.Contains("TEST", System.StringComparison.OrdinalIgnoreCase))
            {
                if(context.RequestId != null)
                {
                    await context.RespondAsync<OrderSubmissionRejected>(
                        new
                        {
                            InVar.Timestamp,
                            OrderId = context.Message.OrderId,
                            CustomerNumber = context.Message.CustomerNumber,
                            Reason = $"Test customer cannot submit orders: {context.Message.CustomerNumber}"
                        });

                    return;
                }
            }

            var notes = context.Message.Notes;
            if (notes.HasValue)
            {
                string notesValue = await notes.Value;
                System.Console.WriteLine($"NOTES: {notesValue}");
            }

            await context.Publish<OrderSubmitted>(new
            {
                InVar.Timestamp,
                OrderId = context.Message.OrderId,
                CustomerNumber = context.Message.CustomerNumber,
                PaymentCardNumber = context.Message.PaymentCardNumber,
                Notes = context.Message.Notes
            });

            if (context.RequestId != null)
            {
                await context.RespondAsync<OrderSubmissionAccepted>(new
                {
                    InVar.Timestamp,
                    OrderId = context.Message.OrderId,
                    CustomerNumber = context.Message.CustomerNumber,
                });
            }
        }
    }
}
