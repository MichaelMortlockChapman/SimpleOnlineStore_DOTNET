package com.example.simpleonlinestore.controllers;

import java.nio.charset.StandardCharsets;
import java.util.Base64;
import java.util.Optional;
import java.util.regex.Pattern;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.server.ResponseStatusException;

import com.example.simpleonlinestore.database.customer.Customer;
import com.example.simpleonlinestore.database.customer.CustomerRepository;
import com.example.simpleonlinestore.database.sessions.Session;
import com.example.simpleonlinestore.database.sessions.SessionRepository;
import com.example.simpleonlinestore.database.users.User;
import com.example.simpleonlinestore.database.users.UserRepository;
import com.example.simpleonlinestore.security.UserRole;
import com.example.simpleonlinestore.security.filters.cookies.CookieGenerator;

import jakarta.servlet.http.Cookie;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;

import org.springframework.web.bind.annotation.CookieValue;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestHeader;
import org.springframework.web.bind.annotation.PutMapping;


@RestController
public class UserController {

  @Value("${backend.secret}")
  private String secret;
  
  @Autowired
  private UserRepository userRepository;

  @Autowired
  private CookieGenerator cookieGenerator;

  @Autowired
  private SessionRepository sessionRepository;

  @Autowired
  private PasswordEncoder passwordEncoder;

  @Autowired
  private CustomerRepository customerRepository;

  // simple record for login info
  public record LoginDetails(String login, String password) {}

  public record CustomerSignupRequest(String login, String password, 
    String name, String address, String city, 
    Integer postalCode, String country) {}

  // pattern for valid emails
  private static Pattern emailRegex = Pattern.compile("(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])");

  /**
   * if user is authenticated (done by loginFilter) returns session cookie using login info from 
   *  basic "Authorization" header. Otherwise returns BAD_REQUEST
   * @param authHeader with basic auth deatails password and email
   * @return ResponseEntity with status code
   */
  @PostMapping("/v1/auth/signin")
  public ResponseEntity<String> signin(@RequestHeader("Authorization") String authHeader, HttpServletResponse response) {
    String base64Credentials = authHeader.substring("Basic".length()).trim();
    byte[] credDecoded = Base64.getDecoder().decode(base64Credentials);
    String credentials = new String(credDecoded, StandardCharsets.UTF_8);
    String[] values = credentials.split(":", 2);
    LoginDetails loginRequest = new LoginDetails(values[0], values[1]);

    Authentication authentication = new UsernamePasswordAuthenticationToken(loginRequest.login(), loginRequest.password(), userRepository.findByLogin(loginRequest.login()).getAuthorities());
    if (authentication.isAuthenticated()) {
      Cookie c = cookieGenerator.generateToken(loginRequest.login());
      sessionRepository.save(new Session(c.getValue(), loginRequest.login()));
      response.addCookie(c);
      return ResponseEntity.ok().build(); 
    } else {
      return ResponseEntity.badRequest().build();
    }
  }  

  /**
   * runs validating logic on loginRequest details, if valid returned result is empty
   * @param loginRequest
   * @return Optional<ResponseEntity<String>>
   */
  private Optional<ResponseEntity<String>> validateLoginRequest(LoginDetails loginRequest) {
    Optional<ResponseEntity<String>> result = Optional.empty();
    if (userRepository.findByLogin(loginRequest.login()) != null) {
      Optional.of(new ResponseEntity<String>("Login already in use", HttpStatus.BAD_REQUEST));
    } else if (!emailRegex.matcher(loginRequest.login()).find()) {
      Optional.of(new ResponseEntity<String>("Invalid email", HttpStatus.BAD_REQUEST));
    } else if (loginRequest.password.length() < 8) {
      Optional.of(new ResponseEntity<String>("Password less than 8 characters", HttpStatus.BAD_REQUEST));
    }
    return result;
  }
  
  /**
   * creates users and sends back session cookie back on valid signup.
   *  Returns BAD_REQUEST status if email already in use, email is invalid, and or password in less than 8 chars
   * @param response
   * @param loginRequest record of signup infomation (email, password)
   * @return ResponseEntity with status code
   */
  @PostMapping("/v1/auth/signup/customer")
  public ResponseEntity<String> signup(HttpServletResponse response, @RequestBody CustomerSignupRequest signupRequest) {
    Optional<ResponseEntity<String>> invalidatingReponse = validateLoginRequest(
      new LoginDetails(signupRequest.login(), signupRequest.password())
    );
    if (invalidatingReponse.isPresent()) {
      return invalidatingReponse.get();
    }

    Customer customer = new Customer(
      signupRequest.name(), signupRequest.address(), signupRequest.city(), 
      signupRequest.postalCode(), signupRequest.country()
    );
    customerRepository.save(customer);

    String encodedPassword = passwordEncoder.encode(signupRequest.password() + secret);
    User user = new User(signupRequest.login(), encodedPassword, UserRole.ROLE_USER, customer.getId());
    userRepository.save(user);
      
    Cookie c = cookieGenerator.generateToken(signupRequest.login());
    sessionRepository.save(new Session(c.getValue(), signupRequest.login()));
    response.addCookie(c);

    return new ResponseEntity<String>("User signed up", HttpStatus.CREATED);
  }

  /***
   * checks new login info is valid, if it is valid updates user info and associated tables. 
   * Also if the password changes ends all sessions.
   * @param response
   * @param loginRequest 
   * @param loginCookieValue record of signup infomation (email, password)
   * @param authCookieValue
   * @return ResponseEntity with status code and string
   */
  @PutMapping("/v1/auth/user/update")
  public ResponseEntity<String> updateUserCreds(HttpServletResponse response, @RequestBody LoginDetails loginRequest,
    @CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue,
    @CookieValue(CookieGenerator.COOKE_NAME) String authCookieValue
  ) {
    Optional<ResponseEntity<String>> validatingReponse = validateLoginRequest(loginRequest);
    if (validatingReponse.isPresent()) {
      return validatingReponse.get();
    }
    
    String oldEncodedPassword = userRepository.findByLogin(loginCookieValue).getPassword();
    String encodedPassword = passwordEncoder.encode(loginRequest.password + secret);
    // update user creds
    userRepository.UpdateUserCreds(loginCookieValue, loginRequest.login(), encodedPassword);
    // update associated tables
    userRepository.updateAssociationsFromUpdate(loginCookieValue, loginRequest.login());
    // if new password, deletes all sessions
    if (!passwordEncoder.matches(loginRequest.password + secret, oldEncodedPassword)) {
      sessionRepository.deleteAllUserSessions(loginRequest.login());
      response.addCookie(cookieGenerator.invalidateCookie(loginRequest.login()));
    } else {
      Cookie newCookie = cookieGenerator.generateToken(loginRequest.login());
      newCookie.setValue(authCookieValue);
      response.addCookie(newCookie);
    }

    return ResponseEntity.ok("Done");
  }

  /**
   * removes cookie sessions and user from repositorys and sends back invalidated cookie (max age = 0) 
   *  so client deletes it
   * @param response
   * @param loginCookieValue value of login
   * @param authCookieValue value of token cookie
   * @return
   */
  @DeleteMapping("/v1/auth/delete")
  public ResponseEntity<String> deleteUser(HttpServletResponse response, 
    @CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue,
    @CookieValue(CookieGenerator.COOKE_NAME) String authCookieValue
  ) throws ResponseStatusException {

    response.addCookie(cookieGenerator.invalidateCookie(loginCookieValue));
    sessionRepository.deleteById(authCookieValue);
    
    userRepository.deleteByLogin(loginCookieValue);

    return ResponseEntity.ok("Done");
  }
  
  /**
   * removes received session from repository and sends back invalidated cookie (max age = 0) 
   *  so client deletes it
   * @param request
   * @param response
   * @param loginCookieValue value of login
   * @param authCookieValue value of token cookie
   * @return
   */
  @PostMapping("/v1/auth/signout")
  public ResponseEntity<String> signout(HttpServletRequest request, HttpServletResponse response,
    @CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue, 
    @CookieValue(CookieGenerator.COOKE_NAME) String authCookieValue
  ) {
    SecurityContextHolder.clearContext();

    response.addCookie(cookieGenerator.invalidateCookie(loginCookieValue));
    sessionRepository.deleteById(authCookieValue);

    return ResponseEntity.ok("Done");
  }
  
  /**
   * removes all sessions of user from repository and sends back invalidated cookie (max age = 0) 
   *  so client deletes it
   * @param request
   * @param response
   * @param loginCookieValue value of login
   * @param authCookieValue value of token cookie
   * @return
   */
  @PostMapping("/v1/auth/signout/all")
  public ResponseEntity<String> signoutAll(HttpServletRequest request, HttpServletResponse response,
    @CookieValue(CookieGenerator.COOKIE_LOGIN) String loginCookieValue, 
    @CookieValue(CookieGenerator.COOKE_NAME) String authCookieValue
  ) {
    SecurityContextHolder.clearContext();

    response.addCookie(cookieGenerator.invalidateCookie(loginCookieValue));
    sessionRepository.deleteById(authCookieValue);
    sessionRepository.deleteAllUserSessions(loginCookieValue);

    return ResponseEntity.ok("Done");
  }
}
