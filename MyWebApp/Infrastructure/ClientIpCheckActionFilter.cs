using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MyWebApp.Infrastructure
{
    public class ClientIpCheckActionFilter : ActionFilterAttribute
    {
        private readonly ILogger<ClientIpCheckActionFilter> _logger;
        private readonly List<IPAddress> _ipAddresses;
        private readonly List<IPNetwork> _ipNetworks;

        public ClientIpCheckActionFilter(IpSafeList safeList, ILogger<ClientIpCheckActionFilter> logger)
        {
            _ipAddresses = safeList.IpAddresses.Split(';').Select(IPAddress.Parse).ToList();
            _ipNetworks = safeList.IpNetworks.Split(';').Select(IPNetwork.Parse).ToList();
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
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
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            base.OnActionExecuting(context);
        }
    }


    public class IpSafeList
    {
        public string IpAddresses { get; set; }
        public string IpNetworks { get; set; }
    }
}
