using Microsoft.AspNetCore.Mvc;
using MyWebApp.Infrastructure;

namespace MyWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class ValuesController(ILogger<ValuesController> logger) : ControllerBase
{
    [ServiceFilter(typeof(ClientIpCheckActionFilter))]
    [HttpGet]
    public IEnumerable<string> Get()
    {
        logger.LogInformation("Client IP: {remoteIpAddress}", HttpContext.Connection.RemoteIpAddress?.ToString());
        return new[] { "value1", "value2" };
    }
}