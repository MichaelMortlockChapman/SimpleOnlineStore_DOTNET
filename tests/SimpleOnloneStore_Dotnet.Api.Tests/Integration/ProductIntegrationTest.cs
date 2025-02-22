using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SimpleOnloneStore_Dotnet.Api.Tests.Integration {
    public class ProductIntegrationTest : IClassFixture<CustomWebAppFactory>, IAsyncLifetime {
        
        public CustomWebAppFactory _factory;

        private NpgsqlConnection _dbConnection = default!;
        private Respawner _respawner = default!;

        public async Task InitializeAsync() {
            using (var scope = _factory.Services.CreateScope()) {
                _dbConnection = new NpgsqlConnection(CustomWebAppFactory.GetConnectionString());
                await _dbConnection.OpenAsync();
                _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions {
                    TablesToIgnore = new Respawn.Graph.Table[] {
                            "AspNetRoles"
                        },
                    SchemasToInclude = new[] {
                            "public"
                        },
                    DbAdapter = DbAdapter.Postgres,
                });
            }
            await DisposeAsync();
            using (var scope = _factory.Services.CreateScope()) {
                JsonElement superUserDetails = CustomWebAppFactory.GetSuperUserDetails();
                string username = superUserDetails.GetProperty("UserName").ToString();
                string email = superUserDetails.GetProperty("Email").ToString();
                string password = superUserDetails.GetProperty("Password").ToString();
                await Program.TryAddSuperUser(scope, username, email, password);
            }
        }

        public async Task DisposeAsync() {
            await _respawner.ResetAsync(_dbConnection);
        }

        public ProductIntegrationTest(CustomWebAppFactory factory) {
            _factory = factory;
        }

        [Fact]
        public async Task Post_Create_ReturnsCreated() {
            var client = _factory.CreateClient();
            var result = await client.PostAsJsonAsync("api/v1/Auth/Login", CustomWebAppFactory.GetSuperUserLoginDetails());
            var cookies = result.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            JsonObject data = new() {
                { "Name", "Box" },
                { "Description", "A box." },
                { "Price", "9.99" },
                { "Stock", "1" }
            };
            var requestContent = new StringContent(data.ToJsonString(), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/Product/Create") {
                Content = requestContent,
            };
            request.Headers.Add("Cookie", cookies);

            var result2 = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Created, result2.StatusCode);
            // Check Id is returned (id can be any number so check content/string is number)
            Assert.True(int.TryParse(await result2.Content.ReadAsStringAsync(), out var _));
        }

        [Fact]
        public async Task Post_GetId_ReturnsProduct() {
            var client = _factory.CreateClient();
            var result = await client.PostAsJsonAsync("api/v1/Auth/Login", CustomWebAppFactory.GetSuperUserLoginDetails());
            var cookies = result.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            JsonObject data = new() {
                { "Name", "Box" },
                { "Description", "A box." },
                { "Price", "9.99" },
                { "Stock", "1" }
            };
            var requestContent = new StringContent(data.ToJsonString(), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/Product/Create") {
                Content = requestContent,
            };
            request.Headers.Add("Cookie", cookies);
            var result2 = await client.SendAsync(request);
            var productId = await result2.Content.ReadAsStringAsync();

            var result3 = await client.GetAsync($"api/v1/Product/Get/{productId}");

            Assert.Equal(HttpStatusCode.OK, result3.StatusCode);
            Assert.Contains($"\"id\":{productId}", await result3.Content.ReadAsStringAsync());
        }
    }
}
