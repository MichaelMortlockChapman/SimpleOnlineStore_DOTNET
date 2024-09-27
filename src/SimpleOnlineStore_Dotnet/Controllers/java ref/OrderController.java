package com.example.simpleonlinestore.controllers;

import java.util.Iterator;
import java.util.Optional;
import java.util.UUID;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.annotation.Secured;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.bind.annotation.CookieValue;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;

import com.example.simpleonlinestore.database.customer.Customer;
import com.example.simpleonlinestore.database.customer.CustomerRepository;
import com.example.simpleonlinestore.database.order.Order;
import com.example.simpleonlinestore.database.order.OrderRepository;
import com.example.simpleonlinestore.database.order.OrderStatuses;
import com.example.simpleonlinestore.database.product.ProductRepository;
import com.example.simpleonlinestore.database.users.UserRepository;
import com.example.simpleonlinestore.security.UserRole;
import com.example.simpleonlinestore.security.filters.cookies.CookieGenerator;

@RestController
public class OrderController {
  
  @Autowired
  private OrderRepository orderRepository;

  @Autowired
  private ProductRepository productRepository;

  @Autowired
  private CustomerRepository customerRepository;

  @Autowired
  private UserRepository userRepository;

  public record OrderSimpleRequest(Integer productId, Integer quantity) {}
  public record OrderRequest(
    Integer productId, Integer quantity,
    String address, String city, Integer postalCode, String country
  ) {}
  public record AdminOrderRequest(
    String login, Integer productId, Integer quantity,
    String address, String city, Integer postalCode, String country
  ) {}
  public record OrderStatusUpdateRequest(UUID orderId, OrderStatuses.statuses orderStatus) {}

  // same func as productController's but can't use static as it uses productRepository, easier to copy than use some work around
  public Optional<ResponseEntity<String>> checkProductExists(Integer productId) {
    Optional<ResponseEntity<String>> result = Optional.empty();
    if (!productRepository.findById(productId).isPresent()) {
      result = Optional.of(new ResponseEntity<>("Unknown ProductId", HttpStatus.BAD_REQUEST));
    }
    return result;
  }
  
  /**
   * creates an order for user with user's delivery info
   * @param loginCookieValue
   * @param orderRequest record of productId and quantity
   * @return error msg or JSON obj with orderId
   */
  @PostMapping("/v1/order/create/simple")
  @Secured({UserRole.ROLE_USER})
  public ResponseEntity<String> createSimpleOrder(
    @CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue,  
    @RequestBody OrderSimpleRequest orderRequest
  ) {
    UUID customerId = userRepository.getRoleIdFromLogin(loginCookieValue);
    Optional<ResponseEntity<String>> invalidatingReponse = checkProductExists(orderRequest.productId());
    if (invalidatingReponse.isPresent()) {
      return invalidatingReponse.get();
    }
    Customer customer = customerRepository.findById(customerId).get();
    Order order = new Order(
      productRepository.findById(orderRequest.productId()).get(), 
      customer, 
      orderRequest.quantity(), 
      customer.getAddress(), customer.getCity(), 
      customer.getPostalCode(), customer.getCountry(), 
      OrderStatuses.ORDERED
    );
    orderRepository.save(order);
    return new ResponseEntity<>("{\"orderId\":\"" + order.getId() + "\"}", HttpStatus.CREATED);
  }

  /**
   * creates an order for user with specified delivery info 
   * @param loginCookieValue
   * @param orderRequest record of order info
   * @return error msg or JSON obj with orderId
   */
  @PostMapping("/v1/order/create")
  @Secured({UserRole.ROLE_USER})
  public ResponseEntity<String> createOrder(
    @CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue,  
    @RequestBody OrderRequest orderRequest
  ) {
    UUID customerId = userRepository.getRoleIdFromLogin(loginCookieValue);
    Optional<ResponseEntity<String>> invalidatingReponse = checkProductExists(orderRequest.productId());
    if (invalidatingReponse.isPresent()) {
      return invalidatingReponse.get();
    }
    Customer customer = customerRepository.findById(customerId).get();
    Order order = new Order(
      productRepository.findById(orderRequest.productId()).get(), 
      customer, 
      orderRequest.quantity(), 
      orderRequest.address(), orderRequest.city(), 
      orderRequest.postalCode(), orderRequest.country(), 
      OrderStatuses.ORDERED
    );
    orderRepository.save(order);
    return new ResponseEntity<>("{\"orderId\":\"" + order.getId() + "\"}", HttpStatus.CREATED);
  }

  /***
   * creates an order for any user, only usable by admins
   * @param orderRequest record of order info with customer login
   * @return error msg or JSON obj with orderId
   */
  @PostMapping("/v1/order/admin/create")
  @Secured({UserRole.ROLE_ADMIN})
  public ResponseEntity<String> createOrderAsAdmin(@RequestBody AdminOrderRequest orderRequest) {
    UUID customerId = userRepository.getRoleIdFromLogin(orderRequest.login());
    Optional<ResponseEntity<String>> invalidatingReponse = checkProductExists(orderRequest.productId());
    if (invalidatingReponse.isPresent()) {
      return invalidatingReponse.get();
    }
    Order order = new Order(
      productRepository.findById(orderRequest.productId()).get(), 
      customerRepository.findById(customerId).get(),
      orderRequest.quantity(), 
      orderRequest.address(), orderRequest.city(), 
      orderRequest.postalCode(), orderRequest.country(), 
      OrderStatuses.ORDERED
    );
    orderRepository.save(order);
    return new ResponseEntity<>("{\"orderId\":\"" + order.getId() + "\"}", HttpStatus.CREATED);
  }

  /**
   * updates the status of specified order
   * @param orderId
   * @param orderStatus
   * @return "Done" msg
   */
  @PutMapping("/v1/order/admin/update/status")
  @Secured({UserRole.ROLE_ADMIN})
  public ResponseEntity<String> updateOrderStatus(@RequestBody OrderStatusUpdateRequest statusUpdateRequest) { 
    orderRepository.updateOrderStatus(statusUpdateRequest.orderId(), statusUpdateRequest.orderStatus.name());
    return ResponseEntity.ok("Done");
  }
  
  // converts order list to JSON string of JSON orders
  private static String toJSONList(Iterable<Order> orders) {
    String ordersJSON = "[";
    Iterator<Order> orderIter = orders.iterator();
    while (orderIter.hasNext()) {
      Order order = orderIter.next();
      ordersJSON += order.toJSON();
      if (orderIter.hasNext()) {
        ordersJSON += ",";
      }
    }
    ordersJSON += "]";
    return ordersJSON;
  }

  public record CustomerUpdateOrderRequest(
    UUID orderId, Integer quantity, String address, 
    String city, Integer postalCode, String country
  ) {}
  /**
   * allows the customer to update their order's quanity or delivery info if the order is not already underway or cancelled
   * @param loginCookieValue user login
   * @param updateRequest the updated order info
   * @return either "Done" or error msg
   */
  @PutMapping("/v1/order/update")
  @Secured({UserRole.ROLE_USER})
  public ResponseEntity<String> updateOrderAsCustomer(
    @CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue,
    @RequestBody CustomerUpdateOrderRequest updateRequest
  ) {
    Optional<Order> order = orderRepository.findById(updateRequest.orderId());
    if (!order.isPresent()) {
      return new ResponseEntity<>("Bad OrderId", HttpStatus.BAD_REQUEST);
    } else if (!order.get().getStatus().equals(OrderStatuses.ORDERED)) {
      return new ResponseEntity<>("Order underway or cancelled", HttpStatus.BAD_REQUEST);
    }
    orderRepository.updateOrder(
      updateRequest.orderId(), userRepository.getRoleIdFromLogin(loginCookieValue), 
      updateRequest.quantity(), updateRequest.address(), 
      updateRequest.city(), updateRequest.postalCode(), updateRequest.country()
    );

    return ResponseEntity.ok("Done");
  }

  public record AdminOrderUpdateRequest(
    UUID orderId, String customerLogin, Integer quantity, String address, 
    String city, Integer postalCode, String country
  ) {}
  /**
   * allows admins to update an orders information. Does not update status as route "/v1/order/admin/update/status" exists
   * @param updateRequest the updated order info
   * @return
   */
  @PutMapping("/v1/order/admin/update")
  @Secured({UserRole.ROLE_ADMIN})
  public ResponseEntity<String> updateOrderAsAdmin(
    @RequestBody AdminOrderUpdateRequest updateRequest
  ) {
    Optional<Order> order = orderRepository.findById(updateRequest.orderId());
    if (!order.isPresent()) {
      return new ResponseEntity<>("Bad OrderId", HttpStatus.BAD_REQUEST);
    } else if (!userRepository.existsRoleIdFromLogin(updateRequest.customerLogin())) {
      return new ResponseEntity<>("Bad customer login", HttpStatus.BAD_REQUEST);
    }
    UUID customerId = userRepository.getRoleIdFromLogin(updateRequest.customerLogin());
    orderRepository.updateOrder(
      updateRequest.orderId(), customerId, 
      updateRequest.quantity(), updateRequest.address(), 
      updateRequest.city(), updateRequest.postalCode(), updateRequest.country()
    );

    return ResponseEntity.ok("Done");
  }


  /**
   * gets all active orders of customer
   * @param loginCookieValue user login
   * @return JSON String of active orders
   */
  @GetMapping("/v1/order/get/active")
  @Secured({UserRole.ROLE_USER})
  public ResponseEntity<String> getActiveOrders(@CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue) {
    return ResponseEntity.ok(toJSONList(orderRepository.getAllActiveOrders(userRepository.getRoleIdFromLogin(loginCookieValue))));
  }
 

  /**
   * gets all orders of customer
   * @param loginCookieValue user login
   * @return JSON String of all orders done by customer
   */
  @GetMapping("/v1/order/get/all")
  @Secured({UserRole.ROLE_USER})
  public ResponseEntity<String> getAllOrders(@CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue) {
    return ResponseEntity.ok(toJSONList(orderRepository.getAllOrders(userRepository.getRoleIdFromLogin(loginCookieValue))));
  }

  /**
   * gets specified customer order
   * @param loginCookieValue user login
   * @return JSON String of order
   */
  @GetMapping("/v1/order/get/{orderId}")
  @Secured({UserRole.ROLE_USER})
  public ResponseEntity<String> getOrder(@PathVariable String orderId, @CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue) {
    Optional<Order> order = orderRepository.findById(UUID.fromString(orderId));
    if (!order.isPresent()) {
      return new ResponseEntity<>("Bad OrderId", HttpStatus.BAD_REQUEST);
    }
    return ResponseEntity.ok(order.get().toJSON());
  }
}