# SimpleOnlineStore .NET v1.0.0

## Routes
All leaf nodes are valid route resolutions. For example The '/Hello' leaf indicates the valid route '/api/v1/Hello/Hello'
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
  |   |   |-- /Update
  |   |   |-- /Get
  |   |   |-- /Get/All
  |   |   `-- /Get/Active
  |   |-- /Admin
  |   |   |-- /Create
  |   |   |-- /GetNext
  |   |   |-- /Get/{id}
  |   |   |-- /Update
  |   |   `-- /Update/Status
  `-- /Hello
      |-- /Hello
      |-- /HelloCustomer
      |-- /HelloAdmin
      `-- /HelloAuth
```
