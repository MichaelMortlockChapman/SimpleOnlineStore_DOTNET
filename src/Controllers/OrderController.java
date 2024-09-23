package com.example.simpleonlinestore.controllers;

import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.annotation.Secured;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RestController;

import com.example.simpleonlinestore.database.customer.Customer;
import com.example.simpleonlinestore.database.customer.CustomerRepository;
import com.example.simpleonlinestore.database.order.Order;
import com.example.simpleonlinestore.database.order.OrderRepository;
import com.example.simpleonlinestore.database.product.ProductRepository;
import com.example.simpleonlinestore.database.users.UserRepository;
import com.example.simpleonlinestore.security.UserRole;
import com.example.simpleonlinestore.security.filters.cookies.CookieGenerator;

import org.springframework.web.bind.annotation.CookieValue;

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
    String address, String city, Integer postal_code, String country
  ) {}

  // same func as productController's but can't use static as it uses productRepository, easier to copy than use some work around
  public Optional<ResponseEntity<String>> checkProductExists(Integer productId) {
    Optional<ResponseEntity<String>> result = Optional.empty();
    if (!productRepository.findById(productId).isPresent()) {
      result = Optional.of(new ResponseEntity<>("Unknown ProductId", HttpStatus.BAD_REQUEST));
    }
    return result;
  }
  
  @PostMapping("/v1/order/create")
  @Secured({UserRole.ROLE_USER})
  public ResponseEntity<String> createSimpleOrder(
    @CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue,  
    @RequestBody OrderSimpleRequest orderRequest
  ) {
    Optional<ResponseEntity<String>> invalidatingReponse = checkProductExists(orderRequest.productId());
    if (invalidatingReponse.isPresent()) {
      return invalidatingReponse.get();
    }
    Customer customer = customerRepository.findById(userRepository.getRoleIdFromLogin(loginCookieValue)).get();
    Order order = new Order(
      productRepository.findById(orderRequest.productId()).get(), 
      customer, 
      orderRequest.quantity(), 
      customer.getAddress(), customer.getCity(), 
      customer.getPostalCode(), customer.getCountry(), 
      ""
    );
    orderRepository.save(order);
    return new ResponseEntity<>("Done", HttpStatus.CREATED);
  }

  // @PutMapping("/v1/order/admin/update")
  // @Secured({UserRole.ROLE_USER})
  // public String updateOrderStatus() {
  //     //TODO: process PUT request
      
  //     return entity;
  // }
}
