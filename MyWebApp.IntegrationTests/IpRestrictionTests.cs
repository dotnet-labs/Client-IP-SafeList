using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyWebApp.IntegrationTests;

[TestClass]
public class IpRestrictionTests
{
    [TestMethod]
    public void ParseConfigurations()
    {
        var b = IPNetwork.TryParse("2001:0db8::/64", out var ip);
        Assert.IsTrue(b);
        Assert.AreEqual(new IPNetwork(IPAddress.Parse("2001:0db8::"), 64), ip);
    }

    [TestMethod]
    public async Task HttpRequestWithAllowedIpAddressShouldReturn200()
    {
        var factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("https_port", "5001").ConfigureLogging(c => c.AddConsole());
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IStartupFilter>(new CustomRemoteIpStartupFilter(IPAddress.Parse("127.0.0.1")));
            });
        });
        var client = factory.CreateClient();
        var response = await client.GetAsync("values");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var json = await response.Content.ReadAsStringAsync();
        Assert.AreEqual("[\"value1\",\"value2\"]", json);
    }

    [TestMethod]
    public async Task HttpRequestWithForbiddenIpAddressShouldReturn403()
    {
        var factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("https_port", "5001").ConfigureLogging(c => c.AddConsole());
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IStartupFilter>(new CustomRemoteIpStartupFilter(IPAddress.Parse("127.168.1.32")));
            });
        });
        var client = factory.CreateClient();
        var response = await client.GetAsync("values");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task HttpRequestWithLocalHostIpAddressShouldReturn200()
    {
        var factory = new WebApplicationFactory<Startup>()
            .WithWebHostBuilder(builder => builder.UseSetting("https_port", "5001").ConfigureLogging(c => c.AddConsole()));

        var context = await factory.Server.SendAsync((c) =>
        {
            c.Connection.RemoteIpAddress = IPAddress.Parse("127.168.1.32");
            c.Request.Method = HttpMethods.Get;
            c.Request.Path = new PathString("/values");
            c.Request.IsHttps = true;

        });
        Assert.AreEqual((int)HttpStatusCode.Forbidden, context.Response.StatusCode);

        context = await factory.Server.SendAsync((c) =>
       {
           c.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.9");
           c.Request.Method = HttpMethods.Get;
           c.Request.Path = new PathString("/values");
           c.Request.IsHttps = true;

       });
        var response = context.Response;
        Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/json; charset=utf-8", response.ContentType);

        using var sr = new StreamReader(response.Body);
        var json = await sr.ReadToEndAsync();
        Assert.AreEqual("[\"value1\",\"value2\"]", json);
    }
}