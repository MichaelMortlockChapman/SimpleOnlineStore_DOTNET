using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleOnlineStore_Dotnet.Data;
using SimpleOnlineStore_Dotnet.Models;
using static SimpleOnlineStore_Dotnet.Controllers.OrderController;

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

  //      /***
  // * creates an order for any user, only usable by admins
  // * @param orderRequest record of order info with customer login
  // * @return error msg or JSON obj with orderId
  // */
  //@PostMapping("/v1/order/admin/create")
  //@Secured({UserRole.ROLE_ADMIN})
  //public ResponseEntity<String> createOrderAsAdmin(@RequestBody AdminOrderRequest orderRequest) {
  //  UUID customerId = userRepository.getRoleIdFromLogin(orderRequest.login());
  //  Optional<ResponseEntity<String>> invalidatingReponse = checkProductExists(orderRequest.productId());
  //  if (invalidatingReponse.isPresent()) {
  //    return invalidatingReponse.get();
  //  }
  //  Order order = new Order(
  //    productRepository.findById(orderRequest.productId()).get(), 
  //    customerRepository.findById(customerId).get(),
  //    orderRequest.quantity(), 
  //    orderRequest.address(), orderRequest.city(), 
  //    orderRequest.postalCode(), orderRequest.country(), 
  //    OrderStatuses.ORDERED
  //  );
  //  orderRepository.save(order);
  //  return new ResponseEntity<>("{\"orderId\":\"" + order.getId() + "\"}", HttpStatus.CREATED);
  //}

  ///**
  // * updates the status of specified order
  // * @param orderId
  // * @param orderStatus
  // * @return "Done" msg
  // */
  //@PutMapping("/v1/order/admin/update/status")
  //@Secured({UserRole.ROLE_ADMIN})
  //public ResponseEntity<String> updateOrderStatus(@RequestBody OrderStatusUpdateRequest statusUpdateRequest) { 
  //  orderRepository.updateOrderStatus(statusUpdateRequest.orderId(), statusUpdateRequest.orderStatus.name());
  //  return ResponseEntity.ok("Done");
  //}
  
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

  //public record CustomerUpdateOrderRequest(
  //  UUID orderId, Integer quantity, String address, 
  //  String city, Integer postalCode, String country
  //) {}
  ///**
  // * allows the customer to update their order's quanity or delivery info if the order is not already underway or cancelled
  // * @param loginCookieValue user login
  // * @param updateRequest the updated order info
  // * @return either "Done" or error msg
  // */
  //@PutMapping("/v1/order/update")
  //@Secured({UserRole.ROLE_USER})
  //public ResponseEntity<String> updateOrderAsCustomer(
  //  @CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue,
  //  @RequestBody CustomerUpdateOrderRequest updateRequest
  //) {
  //  Optional<Order> order = orderRepository.findById(updateRequest.orderId());
  //  if (!order.isPresent()) {
  //    return new ResponseEntity<>("Bad OrderId", HttpStatus.BAD_REQUEST);
  //  } else if (!order.get().getStatus().equals(OrderStatuses.ORDERED)) {
  //    return new ResponseEntity<>("Order underway or cancelled", HttpStatus.BAD_REQUEST);
  //  }
  //  orderRepository.updateOrder(
  //    updateRequest.orderId(), userRepository.getRoleIdFromLogin(loginCookieValue), 
  //    updateRequest.quantity(), updateRequest.address(), 
  //    updateRequest.city(), updateRequest.postalCode(), updateRequest.country()
  //  );

  //  return ResponseEntity.ok("Done");
  //}

  //public record AdminOrderUpdateRequest(
  //  UUID orderId, String customerLogin, Integer quantity, String address, 
  //  String city, Integer postalCode, String country
  //) {}
  ///**
  // * allows admins to update an orders information. Does not update status as route "/v1/order/admin/update/status" exists
  // * @param updateRequest the updated order info
  // * @return
  // */
  //@PutMapping("/v1/order/admin/update")
  //@Secured({UserRole.ROLE_ADMIN})
  //public ResponseEntity<String> updateOrderAsAdmin(
  //  @RequestBody AdminOrderUpdateRequest updateRequest
  //) {
  //  Optional<Order> order = orderRepository.findById(updateRequest.orderId());
  //  if (!order.isPresent()) {
  //    return new ResponseEntity<>("Bad OrderId", HttpStatus.BAD_REQUEST);
  //  } else if (!userRepository.existsRoleIdFromLogin(updateRequest.customerLogin())) {
  //    return new ResponseEntity<>("Bad customer login", HttpStatus.BAD_REQUEST);
  //  }
  //  UUID customerId = userRepository.getRoleIdFromLogin(updateRequest.customerLogin());
  //  orderRepository.updateOrder(
  //    updateRequest.orderId(), customerId, 
  //    updateRequest.quantity(), updateRequest.address(), 
  //    updateRequest.city(), updateRequest.postalCode(), updateRequest.country()
  //  );

  //  return ResponseEntity.ok("Done");
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
