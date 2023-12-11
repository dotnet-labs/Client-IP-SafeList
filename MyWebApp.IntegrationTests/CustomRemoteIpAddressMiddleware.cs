using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MyWebApp.IntegrationTests;

public class CustomRemoteIpAddressMiddleware(RequestDelegate next, IPAddress? fakeIpAddress = null)
{
    private readonly IPAddress _fakeIpAddress = fakeIpAddress ?? IPAddress.Parse("127.0.0.1");

    public async Task Invoke(HttpContext httpContext)
    {
        httpContext.Connection.RemoteIpAddress = _fakeIpAddress;
        await next(httpContext);
    }
}

public class CustomRemoteIpStartupFilter(IPAddress? remoteIp = null) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseMiddleware<CustomRemoteIpAddressMiddleware>(remoteIp);
            next(app);
        };
    }
}