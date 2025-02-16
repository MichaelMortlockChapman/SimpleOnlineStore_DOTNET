using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleOnlineStore_Dotnet.Models;

namespace SimpleOnlineStore_Dotnet.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase {

        UserManager<User> userManager;
        public HelloController(UserManager<User> userManager) {
            this.userManager = userManager;
        }

        [HttpGet("[action]")]
        public ActionResult<string> Hello() {
            return Ok("Hello!");
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<string>> HelloAuthAsync() {
            User? user = await userManager.GetUserAsync(User);
            if (user == null) {
                return BadRequest("Authenticated user doesn't exist");
            }
            return Ok($"Hello authenticated {user.UserName}!");
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
