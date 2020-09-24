using MassTransit.Courier;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Components.CourierActivities
{
    public class PaymentActivity : IActivity<PaymentArguments, PaymentLog>
    {
        public async Task<CompensationResult> Compensate(CompensateContext<PaymentLog> context)
        {
            await Task.Delay(100);

            return context.Compensated();
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<PaymentArguments> context)
        {
            string cardNumber = context.Arguments.CardNumber;
            if (string.IsNullOrWhiteSpace(cardNumber)) throw new ArgumentNullException(nameof(cardNumber));

            await Task.Delay(5000);

            if (cardNumber.StartsWith("5999"))
            {
                throw new InvalidOperationException($"The card number was invalid: {context.Arguments.CardNumber}");
            }

            await Task.Delay(500);

            return context.Completed(new { AuthorizationCode = "77777" });
        }
    }


    public interface PaymentArguments
    {
        Guid OrderId { get; }
        decimal Amount { get; }
        string CardNumber { get; }
    }


    public interface PaymentLog
    {
        string AuthorizationCode { get; }
    }
}
