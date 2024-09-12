package com.example.simpleonlinestore.controllers;

import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.server.ResponseStatusException;

import com.example.simpleonlinestore.database.customer.Customer;
import com.example.simpleonlinestore.database.customer.CustomerRepository;
import com.example.simpleonlinestore.database.users.UserRepository;
import com.example.simpleonlinestore.security.UserRole;
import com.example.simpleonlinestore.security.filters.cookies.CookieGenerator;

import java.util.Optional;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.annotation.Secured;
import org.springframework.web.bind.annotation.CookieValue;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;


@RestController
public class CustomerController {

  @Autowired
  private final CustomerRepository customerRepository;

  @Autowired
  private UserRepository userRepository;

  public record CustomerDetails(
    String name, String address, 
    String city, Integer postalCode, 
    String country
  ) {};

  public CustomerController(CustomerRepository customerRepository) {
    this.customerRepository = customerRepository;
  }

  /**
   * gets user details as a JSON string
   * @param loginCookieValue
   * @return JSON string of user details
   * @throws ResponseStatusException
   */
  @GetMapping("/v1/customer/details")
  @Secured({UserRole.ROLE_USER})
  public ResponseEntity<String> getCustomerDetails(
    @CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue
  ) throws ResponseStatusException {
    Optional<Customer> customer = customerRepository.findById(userRepository.getRoleIdFromLogin(loginCookieValue));
    return ResponseEntity.ok(customer.get().toJSON());
  }


  @PutMapping("/v1/customer/update")
  @Secured({UserRole.ROLE_USER})
  public ResponseEntity<String> updateCustomerDetails(
    @RequestBody CustomerDetails customerDetails,
    @CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue
  ) throws ResponseStatusException {
    Optional<Customer> customer = customerRepository.findById(userRepository.getRoleIdFromLogin(loginCookieValue));
    customerRepository.updateCustomerDetails(
      customer.get().getId(), customerDetails.name(), customerDetails.address(),
      customerDetails.city(), customerDetails.postalCode(), customerDetails.country()
    );
    return ResponseEntity.ok("Done");
  }
}
