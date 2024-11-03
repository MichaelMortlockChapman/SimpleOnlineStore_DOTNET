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

        public class SimpleOrderDetails {
            public required int[] ProductIds { get; set; }
            public required int[] ProductQuantities { get; set; }
        }
        [Authorize(Policy = "RequireCustomerRole")]
        [HttpPost("[action]")]
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
            User? user = await signInManager.UserManager.GetUserAsync(signInManager.Context.User);
            if (user == null) {
                return BadRequest("Invalid User");
            }
            Customer? customer = await dataContext.Customers.FindAsync(user.UserRoleId);
            if (customer == null) {
                return BadRequest("Invalid User");
            }
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

        [HttpPost("[action]")]
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
            User? user = await userManager.FindByEmailAsync(orderDetails.CustomerEmail);
            if (user == null) {
                return BadRequest("Invalid User");
            }
            Customer? customer = await dataContext.Customers.FindAsync(user.UserRoleId);
            if (customer == null) {
                return BadRequest("Invalid User");
            }
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

        [HttpPost("[action]")]
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
        [HttpPut("[action]")]
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
            public required string CustomerEmail { get; set; }
            public required Guid OrderId { get; set; }
        }
        [HttpPut("[action]")]
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
            User? user = await userManager.FindByEmailAsync(customerOrderDetailsUpdate.CustomerEmail);
            if (user == null) {
                return BadRequest("Invalid User");
            }
            Customer? customer = await dataContext.Customers.FindAsync(user.UserRoleId);
            if (customer == null) {
                return BadRequest("Invalid User");
            }
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
        [HttpPut("[action]")]
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

        //// converts order list to JSON string of JSON orders
        //private static String toJSONList(Iterable<Order> orders) {
        //  String ordersJSON = "[";
        //  Iterator<Order> orderIter = orders.iterator();
        //  while (orderIter.hasNext()) {
        //    Order order = orderIter.next();
        //    ordersJSON += order.toJSON();
        //    if (orderIter.hasNext()) {
        //      ordersJSON += ",";
        //    }
        //  }
        //  ordersJSON += "]";
        //  return ordersJSON;
        //}

        ///**
        // * gets all active orders of customer
        // * @param loginCookieValue user login
        // * @return JSON String of active orders
        // */
        //@GetMapping("/v1/order/get/active")
        //@Secured({UserRole.ROLE_USER})
        //public ResponseEntity<String> getActiveOrders(@CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue) {
        //  return ResponseEntity.ok(toJSONList(orderRepository.getAllActiveOrders(userRepository.getRoleIdFromLogin(loginCookieValue))));
        //}


        ///**
        // * gets all orders of customer
        // * @param loginCookieValue user login
        // * @return JSON String of all orders done by customer
        // */
        //@GetMapping("/v1/order/get/all")
        //@Secured({UserRole.ROLE_USER})
        //public ResponseEntity<String> getAllOrders(@CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue) {
        //  return ResponseEntity.ok(toJSONList(orderRepository.getAllOrders(userRepository.getRoleIdFromLogin(loginCookieValue))));
        //}

        ///**
        // * gets specified customer order
        // * @param loginCookieValue user login
        // * @return JSON String of order
        // */
        //@GetMapping("/v1/order/get/{orderId}")
        //@Secured({UserRole.ROLE_USER})
        //public ResponseEntity<String> getOrder(@PathVariable String orderId, @CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue) {
        //  Optional<Order> order = orderRepository.findById(UUID.fromString(orderId));
        //  if (!order.isPresent()) {
        //    return new ResponseEntity<>("Bad OrderId", HttpStatus.BAD_REQUEST);
        //  }
        //  return ResponseEntity.ok(order.get().toJSON());
        //}
    }
}
