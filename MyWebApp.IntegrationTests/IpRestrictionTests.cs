using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyWebApp.IntegrationTests
{
    [TestClass]
    public class IpRestrictionTests
    {
        [TestMethod]
        public async Task HttpRequestWithAllowedIpAddressShouldReturn200()
        {
            var factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                builder.UseSetting("https_port", "5001");
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
                builder.UseSetting("https_port", "5001");
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IStartupFilter>(new CustomRemoteIpStartupFilter(IPAddress.Parse("127.168.1.32")));
                });
            });
            var client = factory.CreateClient();
            var response = await client.GetAsync("values");

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.AreEqual("application/problem+json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Ignore("I haven't figured out how to test in this way.")]
        [TestMethod]
        public async Task HttpRequestWithLocalHostIpAddressShouldReturn200()
        {
            var factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder => builder.UseSetting("https_port", "5001"));
            var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true
            });

            // tried to use TestServer.SendAsync to set RemoteIP, but failed
            // https://github.com/aspnet/Hosting/issues/1135
            // https://github.com/aspnet/Hosting/pull/1248
            // If without setting the RemoteIP, the test server will simply use an empty IP, which breaks the test.

            //var context = await factory.Server.SendAsync((c) =>
            //{
            //    c.Connection.RemoteIpAddress = IPAddress.Parse("127.168.1.32");
            //    c.Request.Method = HttpMethods.Get;
            //    c.Request.Path = new PathString("/values");

            //});
            //var response = context.Response;

            //Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            //Assert.AreEqual("application/json; charset=utf-8", response.ContentType);

            //var json =  response.Body.ToString();

            var response = await client.GetAsync("values");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

            var json = await response.Content.ReadAsStringAsync();
            Assert.AreEqual("[\"value1\",\"value2\"]", json);
        }
    }
}
