using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SimpleOnloneStore_Dotnet.Api.Tests.Integration {
    public class AuthControllerIntegrationTests : IClassFixture<CustomWebAppFactory>, IAsyncLifetime {
        
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
        }

        public async Task DisposeAsync() {
            await _respawner.ResetAsync(_dbConnection);
        }

        public AuthControllerIntegrationTests(CustomWebAppFactory factory) {
            _factory = factory;
        }

        [Fact]
        public async Task Get_HelloAuth_Returns401WithoutCookieSession() {
            var client = _factory.CreateClient();

            var result = await client.GetAsync("api/v1/Hello/HelloAuth");

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task Post_Register_ReturnsOkAndValidCookie() {
            //Arrange
            var clientOptions = new WebApplicationFactoryClientOptions() { 
                HandleCookies = true
              
            };
            var client = _factory.CreateClient(clientOptions);
            JsonObject data = new() {
                { "Email", "a@a.com" },
                { "Password", "goodPassword1234!" },
                { "Name", "A" },
                { "Address", "A Street" },
                { "City", "A City" },
                { "PostalCode", 1001 },
                { "Country", "A Kingdom" }
            };

            //Act
            var result = await client.PostAsJsonAsync("api/v1/Auth/Register", data);

            //Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            //Check valid cookie session
            var cookies = result.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/Hello/HelloAuth");
            request.Headers.Add("Cookie", cookies);
            var result2 = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, result2.StatusCode);
        }

        [Fact]
        public async Task Post_Register_Returns400ForDupeEmail() {
            var clientOptions = new WebApplicationFactoryClientOptions() {
                HandleCookies = true

            };
            var client = _factory.CreateClient(clientOptions);
            JsonObject data = new() {
                { "Email", "a@a.com" },
                { "Password", "goodPassword1234!" },
                { "Name", "A" },
                { "Address", "A Street" },
                { "City", "A City" },
                { "PostalCode", 1001 },
                { "Country", "A Kingdom" }
            };
            await client.PostAsJsonAsync("api/v1/Auth/Register", data);

            var result = await client.PostAsJsonAsync("api/v1/Auth/Register", data);

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Post_Register_Returns400ForBadPassword() {
            var clientOptions = new WebApplicationFactoryClientOptions() {
                HandleCookies = true

            };
            var client = _factory.CreateClient(clientOptions);
            JsonObject data = new() {
                { "Email", "a@a.com" },
                { "Password", "goodPassword" },
                { "Name", "A" },
                { "Address", "A Street" },
                { "City", "A City" },
                { "PostalCode", 1001 },
                { "Country", "A Kingdom" }
            };

            var result = await client.PostAsJsonAsync("api/v1/Auth/Register", data);

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Contains("PasswordRequiresNonAlphanumeric", await result.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Post_Login_ReturnsOkAndValidCookie() {
            var clientOptions = new WebApplicationFactoryClientOptions() {
                HandleCookies = true
            };
            var client = _factory.CreateClient(clientOptions);
            JsonObject registerData = new() {
                { "Email", "a@a.com" },
                { "Password", "goodPassword1234!" },
                { "Name", "A" },
                { "Address", "A Street" },
                { "City", "A City" },
                { "PostalCode", 1001 },
                { "Country", "A Kingdom" }
            };
            await client.PostAsJsonAsync("api/v1/Auth/Register", registerData);
            JsonObject loginData = new() {
                { "Email", "a@a.com" },
                { "Password", "goodPassword1234!" },
            };

            var result = await client.PostAsJsonAsync("api/v1/Auth/Login", loginData);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            //Check valid cookie session
            var cookies = result.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
            var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/Hello/HelloAuth");
            request.Headers.Add("Cookie", cookies);
            var result2 = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, result2.StatusCode);
        }

        [Fact]
        public async Task Post_Login_Returns401ForUnknownEmail() {
            var clientOptions = new WebApplicationFactoryClientOptions() {
                HandleCookies = true
            };
            var client = _factory.CreateClient(clientOptions);
            JsonObject loginData = new() {
                { "Email", "a@a.com" },
                { "Password", "goodPassword1234!" },
            };

            var result = await client.PostAsJsonAsync("api/v1/Auth/Login", loginData);

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task Post_Login_Returns401ForBadPassword() {
            var clientOptions = new WebApplicationFactoryClientOptions() {
                HandleCookies = true
            };
            var client = _factory.CreateClient(clientOptions);
            JsonObject registerData = new() {
                { "Email", "a@a.com" },
                { "Password", "goodPassword1234!" },
                { "Name", "A" },
                { "Address", "A Street" },
                { "City", "A City" },
                { "PostalCode", 1001 },
                { "Country", "A Kingdom" }
            };
            await client.PostAsJsonAsync("api/v1/Auth/Register", registerData);
            JsonObject loginData = new() {
                { "Email", "a@a.com" },
                { "Password", "goodPassword1234!6" },
            };

            var result = await client.PostAsJsonAsync("api/v1/Auth/Login", loginData);

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }
    }
}
