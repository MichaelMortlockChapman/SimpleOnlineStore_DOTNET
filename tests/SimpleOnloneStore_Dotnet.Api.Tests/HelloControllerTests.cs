using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace SimpleOnlineStore_Dotnet.SimpleOnloneStore_Dotnet.Api.Tests
{
    public class HelloControllerTests
    : IClassFixture<WebApplicationFactory<Program>> 
    {
        private readonly WebApplicationFactory<Program> _factory;

        public HelloControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_HelloWorld()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/v1/Hello/HelloWorld");

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            string content = await response.Content.ReadAsStringAsync();
            Assert.Equal("hello world!", content);
        }

        [Fact]
        public async Task Get_HelloWorldAuth_UnAuth() {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/v1/Hello/HelloWorldAuth");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode) ;
        }

        //[Fact]
        //public async Task Get_HelloWorldAuth_Forbidden() {
        //    var client = _factory.CreateClient();
        //    var response = await client.GetAsync("/v1/Hello/HelloWorldAuth");
        //    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        //}

        [Fact]
        public async Task Get_BadRoute404() {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/v1/HelloImABadRoute/HelloWorld");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
