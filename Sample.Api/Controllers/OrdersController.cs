using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sample.Contracts;
using System;
using System.Threading.Tasks;

namespace Sample.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly IRequestClient<SubmitOrder> _submitOrderRequestClient;
        private readonly IRequestClient<CheckOrder> _checkOrderRequestClient;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrdersController(
            ILogger<OrdersController> logger,
            IRequestClient<SubmitOrder> submitOrderRequestClient,
            IRequestClient<CheckOrder> checkOrderRequestClient,
            ISendEndpointProvider sendEndpointProvider,
            IPublishEndpoint publishEndpoint
            )
        {
            _logger = logger;
            _submitOrderRequestClient = submitOrderRequestClient;
            _checkOrderRequestClient = checkOrderRequestClient;
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Guid id, string customerNumber, string paymentCardNumber)
        {
            var (accepted, rejected) = await _submitOrderRequestClient.GetResponse<OrderSubmissionAccepted, OrderSubmissionRejected>(
                new {
                    OrderId = id,
                    InVar.Timestamp,
                    CustomerNumber = customerNumber,
                    PaymentCardNumber = paymentCardNumber,
                });

            if (accepted.IsCompletedSuccessfully)
            {
                var response = await accepted;
                return Accepted(response.Message);
            }
            else
            {
                return BadRequest((await rejected).Message);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Get(Guid id)
        {
            var (status, notFound) = await _checkOrderRequestClient.GetResponse<OrderStatus, OrderNotFound>(new
            {
                OrderId = id,
            });

            if (status.IsCompletedSuccessfully)
            {
                return Ok((await status).Message);
            }

            return NotFound((await notFound).Message);
        }


        [HttpPut]
        public async Task<IActionResult> Put(Guid id, string customerNumber)
        {
            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:submit-order"));

            await endpoint.Send<SubmitOrder>(new
            {
                OrderId = id,
                CustomerNumber = customerNumber,
                Timestamp = default(DateTime),
                PaymentCardNumber = default(string)
            });

            return Accepted();
        }


        [HttpPatch]
        public async Task<IActionResult> Patch(Guid orderId)
        {
            await _publishEndpoint.Publish<OrderAccepted>(new
            {
                OrderId = orderId,
                InVar.Timestamp,
            });

            return Accepted();
        }
    }
}
