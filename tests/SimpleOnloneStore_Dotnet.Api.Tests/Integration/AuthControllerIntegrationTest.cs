using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleOnlineStore_Dotnet.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOnloneStore_Dotnet.Api.Tests.Integration {
    public class AuthControllerIntegrationTests : IClassFixture<CustomWebAppFactory> {
        
        public CustomWebAppFactory _factory;
        public AuthControllerIntegrationTests(CustomWebAppFactory factory) {
            _factory = factory;
        }

        [Fact]
        public async Task Test1() {
            var client = _factory.CreateClient();

            var result = await client.GetAsync("api/v1/Hello/Hello");

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            string text = await result.Content.ReadAsStringAsync();
            Assert.Equal("Hello!", text);
        }
    }
}
