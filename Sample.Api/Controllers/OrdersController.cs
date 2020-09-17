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
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public OrdersController(
            ILogger<OrdersController> logger,
            IRequestClient<SubmitOrder> submitOrderRequestClient,
            ISendEndpointProvider sendEndpointProvider
            )
        {
            _logger = logger;
            _submitOrderRequestClient = submitOrderRequestClient;
            _sendEndpointProvider = sendEndpointProvider;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Guid id, string customerNumber)
        {

            var (accepted, rejected) = await _submitOrderRequestClient.GetResponse<OrderSubmissionAccepted, OrderSubmissionRejected>(new
            {
                OrderId = id,
                Timestmap = InVar.Timestamp,
                CustomerNumber = customerNumber,
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

        [HttpPut]
        public async Task<IActionResult> Put(Guid id, string customerNumber)
        {
            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("exchange:submit-order"));

            await endpoint.Send<SubmitOrder>(new
            {
                OrderId = id,
                Timestmap = InVar.Timestamp,
                CustomerNumber = customerNumber,
            });

            return Accepted();
        }
    }
}
