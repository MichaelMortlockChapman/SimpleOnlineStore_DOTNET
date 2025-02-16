using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleOnlineStore_Dotnet.Data;
using SimpleOnlineStore_Dotnet.Models;

namespace SimpleOnlineStore_Dotnet.Controllers {

    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class CustomerController : Controller {
        private readonly DataContext dataContext;
        private readonly UserManager<User> userManager;

        public CustomerController(DataContext dataContext, UserManager<User> userManager) {
            this.dataContext = dataContext;
            this.userManager = userManager;
        }

        [HttpGet("GetDetails")]
        [Authorize(Policy = "RequireCustomerRole")]
        public async Task<ActionResult<string>> GetCustomerDetails() {
            User? user = await userManager.GetUserAsync(User);
            if (user == null) {
                return BadRequest("Invalid User");
            }
            Customer? customer = await dataContext.Customers.FindAsync(user.UserRoleId);
            if (customer == null) {
                return BadRequest("Invalid User");
            }

            return Ok(customer.ToJSON());
        }

        public class CustomerDetails() {
            public required string Name { get; set; }
            public required string Address { get; set; }
            public required string City { get; set; }
            public required int PostalCode { get; set; }
            public required string Country { get; set; }
        }
        [HttpGet("Update")]
        [Authorize(Policy = "RequireCustomerRole")]
        public async Task<ActionResult<string>> UpdateCustomerDetails([FromBody] CustomerDetails customerDetails) {
            User? user = await userManager.GetUserAsync(User);
            if (user == null) {
                return BadRequest("Invalid User");
            }
            Customer? customer = await dataContext.Customers.FindAsync(user.UserRoleId);
            if (customer == null) {
                return BadRequest("Invalid User");
            }
            customer.Name = customerDetails.Name;
            customer.Address = customerDetails.Address;
            customer.City = customerDetails.City;
            customer.PostalCode = customerDetails.PostalCode;
            customer.Country = customerDetails.Country;
            dataContext.Customers.Update(customer);
            await dataContext.SaveChangesAsync();

            return Ok();
        }
    }
}
