# SimpleOnlineStore .NET v0.2.0
![.NET](https://github.com/MichaelMortlockChapman/SimpleOnlineStore_DOTNET/actions/workflows/dotnet.yml/badge.svg)

This project was built to develop and showcase my skills with the ASP.NET Core framework. The web service created utilizes ASP.NET Core 8, .NET 8, and a PostgreSQL database, forming the RESTful API backend of a fictional online store. The service provides business logic for authentication, authorization, customer management, product catalogue, and order processing. Through this project, I deepened my understanding of ASP.NET’s architecture, including dependency injection, entity frameworks, and middleware pipeline configuration.

The applications uses the default 'Cookie Identity' schema for authentication. A simple two state role policy is used for authorization, the two roles being `Admin` and `Customer`.

## Authentication & Authorization
The application uses the default Cookie Identity schema for authentication, leveraging ASP.NET Core Identity to manage user accounts securely. Additionally, a custom two state role policy is used for authorization, with the two roles being:
- `Admin` - Full control over application state. They are able to create/update orders and products.
- `Customer` - Limited control. Allowed to create new orders and update orders only if they are not already underway.

## Routes
All leaf nodes are valid route resolutions. For example The '/Hello' leaf indicates the valid route `/api/v1/Hello/Hello`. Further route info is included in the Swagger details on application start.
```
  /api/v1
  |-- /Auth
  |   |-- /Register
  |   |-- /Login
  |-- /Customer
  |   |-- /GetDetails
  |   `-- /Update
  |-- /Product
  |   |-- /GetNext
  |   |-- /Get/{id}
  |   |-- /Create
  |   |-- /Delete
  |   `-- /Update
  |-- /Order
  |   |-- /Customer
  |   |   |-- /Create
  |   |   |-- /CreateSimple
  |   |   |-- /Get/{id}
  |   |   |-- /Get/All
  |   |   `-- /Get/Active
  |   |   |-- /Update
  |   |-- /Admin
  |   |   |-- /Create
  |   |   |-- /Get/{id}
  |   |   |-- /GetNext
  |   |   |-- /Update
  |   |   `-- /Update/Status
  `-- /Hello
      |-- /Hello
      |-- /HelloCustomer
      |-- /HelloAdmin
      `-- /HelloAuth
```

## Key Features
- User Management - Authentication & Authorization
- Data Persistence with a PostgreSQL Database
- Object Pagination
- RESTful API Design 
- Unit Testing

## Future Enhancements
-  Further integration testing: Authentication and authorization are implemented, but integration testing for 'business logic' controllers is for the most part still missing.
- Improve secret management: Currently, all secrets are stored directly within project files, which is a poor security practice. Instead, environment variables should be used to keep sensitive information out of source code.
