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
                await signInManager.SignInAsync(user, isPersistent: false);
                logger.LogInformation("User registered");
                return Ok("");
            } else {
                await dataContext.DisposeAsync();
                logger.LogError("Error Creating User");
                return StatusCode((int)HttpStatusCode.InternalServerError);
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
                await signInManager.SignInAsync(user, isPersistent: false);
                logger.LogInformation("User Logged In");
                return Ok();
            } else {
                return Unauthorized();
            }
        }
    }
}
