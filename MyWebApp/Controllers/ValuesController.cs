using Microsoft.AspNetCore.Mvc;
using MyWebApp.Infrastructure;

namespace MyWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(ILogger<ValuesController> logger)
        {
            _logger = logger;
        }

        [ServiceFilter(typeof(ClientIpCheckActionFilter))]
        [HttpGet]
        public IEnumerable<string> Get()
        {
            _logger.LogInformation("Client IP: {remoteIpAddress}", HttpContext.Connection.RemoteIpAddress?.ToString());
            return new[] { "value1", "value2" };
        }
    }
}
