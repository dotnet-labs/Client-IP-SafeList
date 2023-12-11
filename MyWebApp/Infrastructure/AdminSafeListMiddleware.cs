namespace MyWebApp.Infrastructure
{
    public class AdminSafeListMiddleware(
        RequestDelegate next,
        ILogger<AdminSafeListMiddleware> logger,
        IpSafeList safeList)
    {
        public Task Invoke(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;
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
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            return next.Invoke(context);
        }
    }
}
