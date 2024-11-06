//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.VisualStudio.TestPlatform.TestHost;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SimpleOnloneStore_Dotnet.Api.Tests {
//    public class AuthTests
//    : IClassFixture<WebApplicationFactory<Program>> {
//        private readonly WebApplicationFactory<Program> _factory;

//        public AuthTests(WebApplicationFactory<Program> factory) {
//            _factory = factory;
//        }

//        [Fact]
//        public async Task Post_AuthRegistrationAndLoginWork() {
//            var client = _factory.CreateClient();

//            Dictionary<string, object> requestBody = new();
//            requestBody.Add("email", Guid.NewGuid() + "@a.com");
//            requestBody.Add("password", "123abcABC!");
//            string jsonValue = JsonConvert.SerializeObject(requestBody);
//            StringContent content = new StringContent(jsonValue, Encoding.UTF8, "application/json");

//            var response = await client.PostAsync("/v1/Auth/Registration", content);
//            response.EnsureSuccessStatusCode(); // Status Code 200-299

//            response = await client.PostAsync("/v1/Auth/Login", content);
//            response.EnsureSuccessStatusCode();

//            IEnumerable<string> cookies = client.DefaultRequestHeaders.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
//            Assert.NotEmpty(cookies);
//        }
//    }
//}
