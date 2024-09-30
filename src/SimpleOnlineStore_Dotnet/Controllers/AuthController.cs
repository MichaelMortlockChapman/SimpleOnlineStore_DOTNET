using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleOnlineStore_Dotnet.Data;
using SimpleOnlineStore_Dotnet.Models;
using System.Linq;

namespace SimpleOnlineStore_Dotnet.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        

        SignInManager<User> signInManager;
        UserManager<User> userManager;
        RoleManager<IdentityRole> roleManager;
        readonly ILogger<AuthController> logger;

        public AuthController(
            UserManager<User> userManager, 
            RoleManager<IdentityRole> roleManager,
            SignInManager<User> signInManager,
            ILogger<AuthController> logger)
          {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }

        public class RegisterFormDetails {
            public required string Email { get; set; }
            public required string Password { get; set; }
        }
        [HttpPost("[action]")]
        public async Task<ActionResult<string>> Register(RegisterFormDetails details) {
            User user = new User { UserName = details.Email, Email = details.Email };
            if (await userManager.FindByEmailAsync(user.Email) != null) {
                return BadRequest("Username already exists");
            }
            var result = await userManager.CreateAsync(user, details.Password);
            if (result.Succeeded) {
                user.EmailConfirmed = true;
                await userManager.AddToRoleAsync(user, Roles.CUSTOMER_ROLE);
                await signInManager.SignInAsync(user, isPersistent: false);
                logger.LogInformation("User registered");
                return Ok("");
            } else {
                logger.LogError("Error Creating User");
                return BadRequest(result.Errors);
            }
        }

        public class LoginFormDetails {
            public required string Email { get; set; }
            public required string Password { get; set; }
        }
        [HttpPost("[action]")]
        public async Task<ActionResult<string>> Login(LoginFormDetails details) {
            User? user = await userManager.FindByEmailAsync(details.Email);
            if (user == null) {
                return Unauthorized();
            }
            var result = await signInManager.CheckPasswordSignInAsync(user, details.Password,lockoutOnFailure: false);
            if (result.Succeeded) {
                await signInManager.SignInAsync(user, isPersistent: false);
                logger.LogInformation("User Logged In");
                return Ok();
            } else {
                return Unauthorized();
            }
        }
    }
}
