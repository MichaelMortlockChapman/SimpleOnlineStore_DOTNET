using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleOnlineStore_Dotnet.Data;

namespace SimpleOnlineStore_Dotnet.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase {
        public HelloController() { }

        [HttpGet("[action]")]
        public ActionResult<string> Hello() {
            return Ok("Hello!");
        }

        [Authorize]
        [HttpGet("[action]")]
        public ActionResult<string> HelloAuth() {
            return Ok("Hello authenticated user!");
        }

        [Authorize(Policy = "RequireCustomerRole")]
        [HttpGet("[action]")]
        public ActionResult<string> HelloCustomer() {
            return Ok("Hello customer!");
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("[action]")]
        public ActionResult<string> HelloAdmin() {
            return Ok("Hello admin!");
        }
    }
}
