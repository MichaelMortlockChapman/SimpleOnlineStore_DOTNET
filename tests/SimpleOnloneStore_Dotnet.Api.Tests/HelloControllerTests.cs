using Microsoft.AspNetCore.Mvc.Testing;
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
        public void Add1plus1()
        {
            Assert.Equal(1 + 1, 2);
        }

        [Fact]
        public async Task Get_HelloWorld()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/v1/Hello/HelloWorld");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            string content = await response.Content.ReadAsStringAsync();
            Assert.Equal("hello world!", content);
        }
    }
}
