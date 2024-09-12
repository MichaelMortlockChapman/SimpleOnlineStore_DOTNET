package com.example.simpleonlinestore.controllers;

import org.springframework.security.access.annotation.Secured;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

import com.example.simpleonlinestore.security.UserRole;

// simple testing routes controller 
@RestController
public class HelloController {
  private String welcomeSalutation = "Hello";
  
  @GetMapping("/v1/hello")
  public String hello() {
    return welcomeSalutation + ", World!";
  }

  @GetMapping("/v1/hello/user")
  @Secured({UserRole.ROLE_USER, UserRole.ROLE_ADMIN})
  public String helloUser() {
    return welcomeSalutation + ", user!";
  }

  @GetMapping("/v1/hello/admin")
  @Secured({UserRole.ROLE_ADMIN})
  public String helloAdmin(Authentication authentication) {
    System.out.println(authentication.getAuthorities().toString());
    return welcomeSalutation + ", admin!";
  }
}
