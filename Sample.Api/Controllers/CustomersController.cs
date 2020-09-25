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
    public class CustomersController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public CustomersController(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id, string customerNumber)
        {
            await _publishEndpoint.Publish<CustomerAccountClosed>(new
            {
                CustomerNumber = customerNumber,
                CustomerId = id,
            });

            return Ok();
        }
    }
}
