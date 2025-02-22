using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleOnlineStore_Dotnet.Data;
using SimpleOnlineStore_Dotnet.Models;
using static SimpleOnlineStore_Dotnet.Controllers.OrderController;

namespace SimpleOnlineStore_Dotnet.Controllers {
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class OrderController : ControllerBase {
        private readonly DataContext dataContext;
        private readonly UserManager<User> userManager;

        public OrderController(DataContext dataContext, UserManager<User> userManager) {
            this.dataContext = dataContext;
            this.userManager = userManager;
        }

        private record FindCustomerResult(bool succeeded, string? errorMsg, Customer? customer);
        private async Task<FindCustomerResult> TryFindCustomer(User? user) {
            if (user == null) {
                return new FindCustomerResult(false, "Invalid User", null);
            }
            Customer? customer = await dataContext.Customers.FindAsync(user.UserRoleId);
            if (customer == null) {
                return new FindCustomerResult(false, "Invalid Customer", null);
            }
            return new FindCustomerResult(true, null, customer);
        }

        private record ProductValidationResult(bool succeeded, string? errorMsg, List<Product>? products);
        private async Task<ProductValidationResult> ValidateProducts(int[] productIds, int[] productQuantities) {
            if (productIds.Length != productQuantities.Length) {
                return new ProductValidationResult(false, "Length of Product IDs does not match length of Product Quantities", null);
            }
            List<Product> products = new List<Product>();
            foreach (var productEntry in productIds) {
                Product? p = await dataContext.Products.FindAsync(productEntry);
                if (p == null) {
                    return new ProductValidationResult(false, "Invalid Product ID", null);
                }
                products.Add(p);
            }
            return new ProductValidationResult(true, null, products);
        }

        public class SimpleOrderDetails {
            public required int[] ProductIds { get; set; }
            public required int[] ProductQuantities { get; set; }
        }
        [HttpPost("Customer/CreateSimple")]
        [Authorize(Policy = "RequireCustomerRole")]
        public async Task<ActionResult<string>> CreateSimple([FromBody] SimpleOrderDetails simpleOrderDetails) {
            var productValidResult = await ValidateProducts(simpleOrderDetails.ProductIds, simpleOrderDetails.ProductQuantities);
            if (!productValidResult.succeeded) {
                return BadRequest(productValidResult.errorMsg);
            }
            List<Product> products = productValidResult.products!;
            FindCustomerResult? customerResult = await TryFindCustomer(await userManager.GetUserAsync(User));
            if (!customerResult.succeeded) {
                return BadRequest(customerResult.errorMsg);
            }
            Customer customer = customerResult.customer!;
            Order order = new Order {
                Products = products,
                ProductQuantities = simpleOrderDetails.ProductQuantities,
                Address = customer.Address,
                City = customer.City,
                Country = customer.Country,
                PostalCode = customer.PostalCode,
                Customer = customer,
                Status = OrderStatuses.ORDERED
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

        [HttpPost("Customer/Create")]
        [Authorize(Policy = "RequireCustomerRole")]
        public async Task<ActionResult<string>> CreateOrder([FromBody] OrderDetails orderDetails) {
            var productValidResult = await ValidateProducts(orderDetails.ProductIds, orderDetails.ProductQuantities);
            if (!productValidResult.succeeded) {
                return BadRequest(productValidResult.errorMsg);
            }
            List<Product> products = productValidResult.products!;
            FindCustomerResult? customerResult = await TryFindCustomer(await userManager.GetUserAsync(User));
            if (!customerResult.succeeded) {
                return BadRequest(customerResult.errorMsg);
            }
            Customer customer = customerResult.customer!;
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
            public required OrderStatuses.Statuses Status { get; set; }
        }

        [HttpPost("Admin/Create")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<string>> AdminCreateOrder([FromBody] AdminOrderDetails adminOrderDetails) {
            var productValidResult = await ValidateProducts(adminOrderDetails.ProductIds, adminOrderDetails.ProductQuantities);
            if (!productValidResult.succeeded) {
                return BadRequest(productValidResult.errorMsg);
            }
            List<Product> products = productValidResult.products!;
            FindCustomerResult? customerResult = await TryFindCustomer(await userManager.FindByEmailAsync(adminOrderDetails.CustomerEmail));
            if (!customerResult.succeeded) {
                return BadRequest(customerResult.errorMsg);
            }
            Customer customer = customerResult.customer!;
            Order order = new Order (
                products,
                adminOrderDetails.ProductQuantities,
                customer,
                adminOrderDetails.Address,
                adminOrderDetails.City,
                adminOrderDetails.PostalCode,
                adminOrderDetails.Country,
                adminOrderDetails.Status.ToString()
            );
            await dataContext.Orders.AddAsync(order);
            await dataContext.SaveChangesAsync();
            return CreatedAtAction("CreateSimple", "done");
        }

        public class OrderStatusUpdate {
            public required OrderStatuses.Statuses Status { get; set; }
            public required Guid OrderId { get; set; }
        }
        [HttpPut("Admin/Update/Status")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<string>> UpdateOrderStatus([FromBody] OrderStatusUpdate orderStatusUpdate) {
            Order? order = dataContext.Orders.Find(orderStatusUpdate.OrderId);
            if (order == null) {
                return BadRequest("Invalid Order Id");
            }
            order.Status = orderStatusUpdate.Status.ToString();
            dataContext.Orders.Update(order);
            await dataContext.SaveChangesAsync();
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
        [HttpPut("Customer/Update")]
        [Authorize(Policy = "RequireCustomerRole")]
        public async Task<ActionResult<string>> CustomerUpdateOrder([FromBody] CustomerOrderDetailsUpdate customerOrderDetailsUpdate) {
            var productValidResult = await ValidateProducts(customerOrderDetailsUpdate.ProductIds, customerOrderDetailsUpdate.ProductQuantities);
            if (!productValidResult.succeeded) {
                return BadRequest(productValidResult.errorMsg);
            }
            var products = productValidResult.products!;
            FindCustomerResult? customerResult = await TryFindCustomer(await userManager.GetUserAsync(User));
            if (!customerResult.succeeded) {
                return BadRequest(customerResult.errorMsg);
            }
            Customer customer = customerResult.customer!;
            Order? order = dataContext.Orders.Find(customerOrderDetailsUpdate.OrderId);
            if (order == null) {
                return BadRequest("Invalid Order Id");
            }
            if (!order.Status.Equals(OrderStatuses.ORDERED)) {
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
            await dataContext.SaveChangesAsync();

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
            public required OrderStatuses.Statuses Status { get; set; }
        }
        [HttpPut("Admin/Update")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<string>> AdminUpdateOrder([FromBody] AdminOrderDetailsUpdate adminOrderDetailsUpdate) {
            var productValidResult = await ValidateProducts(adminOrderDetailsUpdate.ProductIds, adminOrderDetailsUpdate.ProductQuantities);
            if (!productValidResult.succeeded) {
                return BadRequest(productValidResult.errorMsg);
            }
            var products = productValidResult.products!;
            FindCustomerResult? customerResult = await TryFindCustomer(
                await userManager.FindByEmailAsync(adminOrderDetailsUpdate.CustomerEmail));
            if (!customerResult.succeeded) {
                return BadRequest(customerResult.errorMsg);
            }
            Customer customer = customerResult.customer!;
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
            await dataContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("Customer/Get/Active")]
        [Authorize(Policy = "RequireCustomerRole")]
        public async Task<ActionResult<string>> GetActiveOrders() {
            FindCustomerResult? customerResult = await TryFindCustomer(await userManager.GetUserAsync(User));
            if (!customerResult.succeeded) {
                return BadRequest(customerResult.errorMsg);
            }
            Customer customer = customerResult.customer!;
            List<Order> orders = dataContext.Orders.Where(o => o.Customer.Id.Equals(customer.Id) && OrderStatuses.IsActive(OrderStatuses.ToEnum(o.Status))).ToList();
            return Ok($"[{String.Join(",", orders.Select(o => o.ToJSON()))}]");
        }

        [HttpGet("Customer/Get/All")]
        [Authorize(Policy = "RequireCustomerRole")]
        public async Task<ActionResult<string>> GetAllOrders() {
            FindCustomerResult? customerResult = await TryFindCustomer(await userManager.GetUserAsync(User));
            if (!customerResult.succeeded) {
                return BadRequest(customerResult.errorMsg);
            }
            Customer customer = customerResult.customer!;
            List<Order> orders = dataContext.Orders.Where(o => o.Customer.Id.Equals(customer.Id)).ToList();
            return Ok($"[{String.Join(",", orders.Select(o => o.ToJSON()))}]");
        }

        [HttpGet("Customer/Get/{id}")]
        [Authorize(Policy = "RequireCustomerRole")]
        public async Task<ActionResult<string>> GetOrder(Guid id) {
            FindCustomerResult? customerResult = await TryFindCustomer(await userManager.GetUserAsync(User));
            if (!customerResult.succeeded) {
                return BadRequest(customerResult.errorMsg);
            }
            Customer customer = customerResult.customer!;
            List<Order> orders = dataContext.Orders.Where(o => o.Customer.Id.Equals(customer.Id) && o.Id.Equals(id)).ToList();
            return Ok($"[{String.Join(",", orders.Select(o => o.ToJSON()))}]");
        }

        [HttpGet("Admin/GetNext")]
        [Authorize(Policy = "RequireAdminRole")]
        public ActionResult<List<Product>> GetNext(Guid? lastId, bool getActiveOnly) {
            var orderedElements = dataContext.Orders.Where(o => !getActiveOnly || OrderStatuses.IsActive(OrderStatuses.ToEnum(o.Status))).OrderBy(o => o.DateCreated);
            List<string> ordersSegment = orderedElements
                .ThenBy(o => o.Id)
                .Where(o => o != null && o.Id > lastId)
                .Take(30)
                .Select(o => o.ToJSON())
            .ToList();

            return Ok($"[{String.Join(",", ordersSegment)}]");
        }

        [HttpGet("Admin/Get/{id}")]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<ActionResult<Product>> Get(Guid id) {
            Order? order = await dataContext.Orders.FindAsync(id);
            if (order == null) {
                return BadRequest("Invalid Order Id");
            }
            return Ok(order.ToJSON());
        }
    }
}
