using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MyWebApp.Infrastructure;

public class ClientIpCheckActionFilter(IpSafeList safeList, ILogger<ClientIpCheckActionFilter> logger)
    : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
        if (remoteIp == null)
        {
            throw new ArgumentException("Remote IP is NULL, may due to missing ForwardedHeaders.");
        }
        logger.LogDebug("Remote IpAddress: {RemoteIp}", remoteIp);

        if (remoteIp.IsIPv4MappedToIPv6)
        {
            remoteIp = remoteIp.MapToIPv4();
        }

        if (!safeList.IsSafeIp(remoteIp))
        {
            logger.LogWarning("Forbidden Request from IP: {remoteIp}", remoteIp);
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            return;
        }

        base.OnActionExecuting(context);
    }
}

public class IpSafeList
{
    private readonly List<IPAddress> _safeIpAddresses;

    private readonly List<IPNetwork> _safeIpNetworks;

    public IpSafeList(string? ipAddresses, string? ipNetworks  )
    {
        if (string.IsNullOrWhiteSpace(ipAddresses))
        {
            _safeIpAddresses = [];
        }
        else
        {
            _safeIpAddresses = ipAddresses.Split(';')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(IPAddress.Parse).ToList();
        }

        _safeIpNetworks = [];

        foreach (var i in (ipNetworks??string.Empty).Split(';'))
        {
            if (IPNetwork.TryParse(i, out var ip))
            {
                _safeIpNetworks.Add(ip);
            }
            else
            {
              
            }
        }
    }

    public bool IsSafeIp(IPAddress remoteIp) => _safeIpAddresses.Contains(remoteIp) || _safeIpNetworks.Any(x => x.Contains(remoteIp));
}
