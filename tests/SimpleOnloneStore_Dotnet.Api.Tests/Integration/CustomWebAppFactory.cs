using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleOnlineStore_Dotnet.Data;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SimpleOnloneStore_Dotnet.Api.Tests.Integration {
    public class CustomWebAppFactory : WebApplicationFactory<Program> {
        protected override void ConfigureWebHost(IWebHostBuilder builder) {
            builder.ConfigureTestServices(services => {
                services.AddDbContext<DataContext>(options => {
                    options.UseNpgsql(GetConnectionString());
                });
            });
        }

        public static string GetConnectionString() {
            var content = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "appsettings.json"));
            var config = JsonSerializer.Deserialize<JsonElement>(content);
            return config.GetProperty("ConnectionStrings").GetProperty("TestConnection").ToString();
        }

        public static JsonElement GetSuperUserDetails() {
            var content = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "appsettings.json"));
            var config = JsonSerializer.Deserialize<JsonElement>(content);
            return config.GetProperty("SuperUser");
        }

        public static JsonObject GetSuperUserLoginDetails() {
            var superUserJson = GetSuperUserDetails();
            JsonObject loginData = new() {
                { "Email", superUserJson.GetProperty("Email").ToString() },
                { "Password", superUserJson.GetProperty("Password").ToString() },
            };

            return loginData;
        }
    }
}
