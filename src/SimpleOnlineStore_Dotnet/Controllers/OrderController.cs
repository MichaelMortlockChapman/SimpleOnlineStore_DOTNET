using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleOnlineStore_Dotnet.Data;
using SimpleOnlineStore_Dotnet.Models;

namespace SimpleOnlineStore_Dotnet.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class OrderController : ControllerBase {
        private readonly DataContext dataContext;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        public OrderController(DataContext dataContext, SignInManager<User> signInManager, UserManager<User> userManager) {
            this.dataContext = dataContext;
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        private record FindCustomerAction(ActionResult<string>? PossibleActionResult, Customer? Customer);
        private async Task<FindCustomerAction> TryFindCustomer() {
            User? user = await signInManager.UserManager.GetUserAsync(signInManager.Context.User);
            if (user == null) {
                return new FindCustomerAction(BadRequest("Invalid User"), null);
            }
            Customer? customer = await dataContext.Customers.FindAsync(user.UserRoleId);
            if (customer == null) {
                return new FindCustomerAction(BadRequest("Invalid User"), null);
            }
            return new FindCustomerAction(null, customer); 
        }

        public class SimpleOrderDetails {
            public required int[] ProductIds { get; set; }
            public required int[] ProductQuantities { get; set; }
        }
        [Authorize(Policy = "RequireCustomerRole")]
        [HttpPost("/Customer/CreateSimple")]
        public async Task<ActionResult<string>> CreateSimple([FromBody] SimpleOrderDetails simpleOrderDetails) {
            if (simpleOrderDetails.ProductIds.Length != simpleOrderDetails.ProductIds.Length) {
                return BadRequest("Length of Product IDs does not match length of Product Quantities");
            }
            List<Product> products = new List<Product>();
            foreach (var productEntry in simpleOrderDetails.ProductIds) {
                Product? p = await dataContext.Products.FindAsync(productEntry);
                if (p == null) {
                    return BadRequest("Invalid Product ID");
                }
                products.Add(p);
            }
            FindCustomerAction? customerAction = await TryFindCustomer();
            if (customerAction.PossibleActionResult != null || customerAction.Customer == null) {
                return customerAction.PossibleActionResult != null ? customerAction.PossibleActionResult : StatusCode(500, "Error with TryFindCustomer Function - no action result nor customer");
            }
            Customer customer = customerAction.Customer;
            Order order = new Order { 
                Products=products, 
                ProductQuantities=simpleOrderDetails.ProductQuantities,
                Address=customer.Address, 
                City=customer.City, 
                Country=customer.Country, 
                PostalCode=customer.PostalCode, 
                Customer=customer, 
                Status=OrderStatuses.ORDERED 
            };
            await dataContext.Orders.AddAsync(order);
            await dataContext.SaveChangesAsync();
            return CreatedAtAction("CreateSimple", "done");
        }

        public class OrderDetails {
            public required int[] ProductIds { get; set; }
            public required int[] ProductQuantities { get; set; }
            public required string Address { get; set; }
            public required string City { get; set; }
            public required int PostalCode { get; set; }
            public required string Country { get; set; }
            public required string CustomerEmail { get; set; }
        }

        [HttpPost("/Customer/Create")]
        [Authorize(Policy = "RequireCustomerRole")]
        public async Task<ActionResult<string>> CreateOrder([FromBody] OrderDetails orderDetails) {
            if (orderDetails.ProductIds.Length != orderDetails.ProductIds.Length) {
                return BadRequest("Length of Product IDs does not match length of Product Quantities");
            }
            List<Product> products = new List<Product>();
            foreach (var productEntry in orderDetails.ProductIds) {
                Product? p = await dataContext.Products.FindAsync(productEntry);
                if (p == null) {
                    return BadRequest("Invalid Product ID");
                }
                products.Add(p);
            }
            FindCustomerAction? customerAction = await TryFindCustomer();
            if (customerAction.PossibleActionResult != null || customerAction.Customer == null) {
                return customerAction.PossibleActionResult != null ? customerAction.PossibleActionResult : StatusCode(500, "Error with TryFindCustomer Function - no action result nor customer");
            }
            Customer customer = customerAction.Customer;
            Order order = new Order {
                Products = products,
                ProductQuantities = orderDetails.ProductQuantities,
                Address = orderDetails.Address,
                City = orderDetails.City,
                Country = orderDetails.Country,
                PostalCode = orderDetails.PostalCode,
                Customer = customer,
                Status = OrderStatuses.ORDERED
            };
            await dataContext.Orders.AddAsync(order);
            await dataContext.SaveChangesAsync();
            return CreatedAtAction("CreateSimple", "done");
        }

        public class AdminOrderDetails {
            public required int[] ProductIds { get; set; }
            public required int[] ProductQuantities { get; set; }
            public required string Address { get; set; }
            public required string City { get; set; }
            public required int PostalCode { get; set; }
            public required string Country { get; set; }
            public required string CustomerEmail { get; set; }
            public required OrderStatuses Status { get; set; }
        }

        [HttpPost("/Admin/Create")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<string>> AdminCreateOrder([FromBody] AdminOrderDetails adminOrderDetails) {
            if (adminOrderDetails.ProductIds.Length != adminOrderDetails.ProductIds.Length) {
                return BadRequest("Length of Product IDs does not match length of Product Quantities");
            }
            List<Product> products = new List<Product>();
            foreach (var productEntry in adminOrderDetails.ProductIds) {
                Product? p = await dataContext.Products.FindAsync(productEntry);
                if (p == null) {
                    return BadRequest("Invalid Product ID");
                }
                products.Add(p);
            }
            User? user = await userManager.FindByEmailAsync(adminOrderDetails.CustomerEmail);
            if (user == null) {
                return BadRequest("Invalid User");
            }
            Customer? customer = await dataContext.Customers.FindAsync(user.UserRoleId);
            if (customer == null) {
                return BadRequest("Invalid User");
            }
            Order order = new Order {
                Products = products,
                ProductQuantities = adminOrderDetails.ProductQuantities,
                Address = adminOrderDetails.Address,
                City = adminOrderDetails.City,
                Country = adminOrderDetails.Country,
                PostalCode = adminOrderDetails.PostalCode,
                Customer = customer,
                Status = adminOrderDetails.Status.ToString(),
            };
            await dataContext.Orders.AddAsync(order);
            await dataContext.SaveChangesAsync();
            return CreatedAtAction("CreateSimple", "done");
        }
        

        public class OrderStatusUpdate {
            public required OrderStatuses Status { get; set; }
            public required Guid OrderId { get; set; }
        }
        [HttpPut("/Admin/Update/Status")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<string>> UpdateOrderStatus([FromBody] OrderStatusUpdate orderStatusUpdate) {
            Order? order = dataContext.Orders.Find(orderStatusUpdate.OrderId);
            if (order == null) {
                return BadRequest("Invalid Order Id");
            }
            order.Status = orderStatusUpdate.Status.ToString();
            dataContext.Orders.Update(order);
            dataContext.SaveChanges();
            return Ok();
        }

        public class CustomerOrderDetailsUpdate {
            public required int[] ProductIds { get; set; }
            public required int[] ProductQuantities { get; set; }
            public required string Address { get; set; }
            public required string City { get; set; }
            public required int PostalCode { get; set; }
            public required string Country { get; set; }
            public required Guid OrderId { get; set; }
        }
        [HttpPut("/Customer/Update")]
        [Authorize(Policy = "RequireCustomerRole")]
        public async Task<ActionResult<string>> CustomerUpdateOrder([FromBody] CustomerOrderDetailsUpdate customerOrderDetailsUpdate) {
            if (customerOrderDetailsUpdate.ProductIds.Length != customerOrderDetailsUpdate.ProductIds.Length) {
                return BadRequest("Length of Product IDs does not match length of Product Quantities");
            }
            List<Product> products = new List<Product>();
            foreach (var productEntry in customerOrderDetailsUpdate.ProductIds) {
                Product? p = await dataContext.Products.FindAsync(productEntry);
                if (p == null) {
                    return BadRequest("Invalid Product ID");
                }
                products.Add(p);
            }
            FindCustomerAction? customerAction = await TryFindCustomer();
            if (customerAction.PossibleActionResult != null || customerAction.Customer == null) {
                return customerAction.PossibleActionResult != null ? customerAction.PossibleActionResult : StatusCode(500, "Error with TryFindCustomer Function - no action result nor customer");
            }
            Customer customer = customerAction.Customer;
            Order? order = dataContext.Orders.Find(customerOrderDetailsUpdate.OrderId);
            if (order == null) {
                return BadRequest("Invalid Order Id");
            }
            if (order.Status.Equals(OrderStatuses.ORDERED)) {
                return BadRequest("Order underway or cancelled");
            }
            order.Products = products;
            order.ProductQuantities = customerOrderDetailsUpdate.ProductQuantities;
            order.Address = customerOrderDetailsUpdate.Address;
            order.City = customerOrderDetailsUpdate.City;
            order.Country = customerOrderDetailsUpdate.Country;
            order.PostalCode = customerOrderDetailsUpdate.PostalCode;
            order.Customer = customer;
            dataContext.Orders.Update(order);
            dataContext.SaveChanges();

            return Ok();
        }

        public class AdminOrderDetailsUpdate {
            public required int[] ProductIds { get; set; }
            public required int[] ProductQuantities { get; set; }
            public required string Address { get; set; }
            public required string City { get; set; }
            public required int PostalCode { get; set; }
            public required string Country { get; set; }
            public required string CustomerEmail { get; set; }
            public required Guid OrderId { get; set; }
            public required OrderStatuses Status { get; set; }
        }
        [HttpPut("/Admin/Update")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<string>> AdminUpdateOrder([FromBody] AdminOrderDetailsUpdate adminOrderDetailsUpdate) {
            if (adminOrderDetailsUpdate.ProductIds.Length != adminOrderDetailsUpdate.ProductIds.Length) {
                return BadRequest("Length of Product IDs does not match length of Product Quantities");
            }
            List<Product> products = new List<Product>();
            foreach (var productEntry in adminOrderDetailsUpdate.ProductIds) {
                Product? p = await dataContext.Products.FindAsync(productEntry);
                if (p == null) {
                    return BadRequest("Invalid Product ID");
                }
                products.Add(p);
            }
            User? user = await userManager.FindByEmailAsync(adminOrderDetailsUpdate.CustomerEmail);
            if (user == null) {
                return BadRequest("Invalid User");
            }
            Customer? customer = await dataContext.Customers.FindAsync(user.UserRoleId);
            if (customer == null) {
                return BadRequest("Invalid User");
            }
            Order? order = dataContext.Orders.Find(adminOrderDetailsUpdate.OrderId);
            if (order == null) {
                return BadRequest("Invalid Order Id");
            }
            order.Products = products;
            order.ProductQuantities = adminOrderDetailsUpdate.ProductQuantities;
            order.Address = adminOrderDetailsUpdate.Address;
            order.City = adminOrderDetailsUpdate.City;
            order.Country = adminOrderDetailsUpdate.Country;
            order.PostalCode = adminOrderDetailsUpdate.PostalCode;
            order.Status = adminOrderDetailsUpdate.Status.ToString();
            order.Customer = customer;
            dataContext.Orders.Update(order);
            dataContext.SaveChanges();

            return Ok();
        }

        [HttpGet("/Customer/Get/Active")]
        [Authorize(Policy = "RequireCustomerRole")]
        public async Task<ActionResult<string>> GetActiveOrders() {
            FindCustomerAction? customerAction = await TryFindCustomer();
            if (customerAction.PossibleActionResult != null || customerAction.Customer == null) {
                return customerAction.PossibleActionResult != null ? customerAction.PossibleActionResult : StatusCode(500, "Error with TryFindCustomer Function - no action result nor customer");
            }
            Customer customer = customerAction.Customer;
            List<Order> orders = dataContext.Orders.Where(o => o.Customer.Id.Equals(customer.Id) && OrderStatuses.IsActive(o.Status)).ToList();
            return $"[{String.Join(",", orders.Select(o => o.ToJSON()))}]";
        }

        [HttpGet("/Customer/Get/All")]
        [Authorize(Policy = "RequireCustomerRole")]
        public async Task<ActionResult<string>> GetAllOrders() {
            FindCustomerAction? customerAction = await TryFindCustomer();
            if (customerAction.PossibleActionResult != null || customerAction.Customer == null) {
                return customerAction.PossibleActionResult != null ? customerAction.PossibleActionResult : StatusCode(500, "Error with TryFindCustomer Function - no action result nor customer");
            }
            Customer customer = customerAction.Customer;
            List<Order> orders = dataContext.Orders.Where(o => o.Customer.Id.Equals(customer.Id)).ToList();
            return $"[{String.Join(",", orders.Select(o => o.ToJSON()))}]";
        }

        [HttpGet("/Customer/Get")]
        [Authorize(Policy = "RequireCustomerRole")]
        public async Task<ActionResult<string>> GetOrder([FromBody] Guid orderId) {
            FindCustomerAction? customerAction = await TryFindCustomer();
            if (customerAction.PossibleActionResult != null || customerAction.Customer == null) {
                return customerAction.PossibleActionResult != null ? customerAction.PossibleActionResult : StatusCode(500, "Error with TryFindCustomer Function - no action result nor customer");
            }
            Customer customer = customerAction.Customer;
            List<Order> orders = dataContext.Orders.Where(o => o.Customer.Id.Equals(customer.Id) && o.Id.Equals(orderId)).ToList();
            return $"[{String.Join(",", orders.Select(o => o.ToJSON()))}]";
        }
    }
}
