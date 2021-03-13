using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MyWebApp.Infrastructure
{
    public class AdminSafeListMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AdminSafeListMiddleware> _logger;
        private readonly List<IPAddress> _ipAddresses;
        private readonly List<IPNetwork> _ipNetworks;

        public AdminSafeListMiddleware(RequestDelegate next, ILogger<AdminSafeListMiddleware> logger, IpSafeList safeList)
        {
            _ipAddresses = safeList.IpAddresses.Split(';').Select(IPAddress.Parse).ToList();
            _ipNetworks = safeList.IpNetworks.Split(';').Select(IPNetwork.Parse).ToList();

            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;
            if (remoteIp == null)
            {
                throw new ArgumentException("Remote IP is NULL, may due to missing ForwardedHeaders.");
            }
            _logger.LogDebug("Remote IpAddress: {RemoteIp}", remoteIp);

            if (remoteIp.IsIPv4MappedToIPv6)
            {
                remoteIp = remoteIp.MapToIPv4();
            }

            if (!_ipAddresses.Contains(remoteIp) && !_ipNetworks.Any(x => x.Contains(remoteIp)))
            {
                _logger.LogWarning($"Forbidden Request from IP: {remoteIp}");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            await _next.Invoke(context);
        }
    }
}
