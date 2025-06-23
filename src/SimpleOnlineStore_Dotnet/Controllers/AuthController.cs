using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleOnlineStore_Dotnet.Data;
using SimpleOnlineStore_Dotnet.Models;
using System.Net;

namespace SimpleOnlineStore_Dotnet.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {

        private readonly DataContext dataContext;
        SignInManager<User> signInManager;
        UserManager<User> userManager;
        readonly ILogger<AuthController> logger;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AuthController> logger,
            DataContext dataContext) {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.dataContext = dataContext;
        }

        public class RegisterFormDetails {
            public required string Email { get; set; }
            public required string Password { get; set; }
            public required string Name { get; set; }
            public required string Address { get; set; }
            public required string City { get; set; }
            public required int PostalCode { get; set; }
            public required string Country { get; set; }
        }
        [HttpPost("[action]")]
        public async Task<ActionResult<string>> Register(RegisterFormDetails details) {
            if (await userManager.FindByEmailAsync(details.Email) != null) {
                return BadRequest("Username already exists");
            }
            Customer customer = new Customer { Name = details.Name, Address = details.Address, City = details.City, Country = details.Country, PostalCode = details.PostalCode };
            dataContext.Add(customer);
            User user = new User { UserName = details.Email, Email = details.Email, UserRoleId = customer.Id };
            var result = await userManager.CreateAsync(user, details.Password);
            if (result.Succeeded) {
                await dataContext.SaveChangesAsync();
                user.EmailConfirmed = true;
                await userManager.AddToRoleAsync(user, Roles.CUSTOMER_ROLE);
                await signInManager.SignInAsync(user, isPersistent: true);
                logger.LogInformation("User registered");
                return Ok("");
            } else {
                await dataContext.DisposeAsync();
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
            var result = await signInManager.CheckPasswordSignInAsync(user, details.Password, lockoutOnFailure: false);
            if (result.Succeeded) {
                await signInManager.SignInAsync(user, isPersistent: true);
                logger.LogInformation("User Logged In");
                return Ok();
            } else {
                return Unauthorized();
            }
        }

        [HttpPost("[action]")]
        [Authorize]
        public async Task<ActionResult<string>> Logout() {
            await signInManager.SignOutAsync();
            return Ok();
        }

        [HttpGet("[action]")]
        [Authorize]
        public async Task<ActionResult<string>> Info() {
            User? user = await userManager.GetUserAsync(User);
            if (user == null) {
                return BadRequest("Invalid User");
            }
            string emailConfirmed = user.EmailConfirmed ? "true" : "false";
            var roles = await userManager.GetRolesAsync(user);
            string userInfoResult = "{" +
                    $"\"email\":\"{user.Email}\"," +
                    $"\"emailConfirmed\":{emailConfirmed}," +
                    $"\"claims\":[{string.Join(",", roles.Select((role) => $"\"{role}\"").ToList())}]" +
                "}";
            return Ok(userInfoResult);
        }
    }
}
