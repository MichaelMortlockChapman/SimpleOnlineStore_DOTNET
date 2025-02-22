using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SimpleOnlineStore_Dotnet.Controllers;
using SimpleOnlineStore_Dotnet.Data;
using SimpleOnlineStore_Dotnet.Models;
using System.Net;
using System.Security.Claims;

namespace SimpleOnloneStore_Dotnet.Api.Tests {
    public class OrderControllerTest {
        private class InitObjects {
            public required Mock<DataContext> dataContextMock;
            public required Mock<UserManager<User>> userManagerMock;
            public required OrderController controller;
        }

        private InitObjects CreateObjects() {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            var dataContextMock = new Mock<DataContext>();

            return new InitObjects {
                dataContextMock = dataContextMock,
                userManagerMock = userManagerMock,
                controller = new OrderController(dataContextMock.Object, userManagerMock.Object),
            };
        }

        [Fact]
        public async Task CreateSimpleReturns_BadRequest_FromUnknownProduct() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            var details = new OrderController.SimpleOrderDetails() {
                ProductIds = [101],
                ProductQuantities = [1]
            };

            var tmp = await controller.CreateSimple(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid Product ID", result.Value);
        }

        [Fact]
        public async Task CreateSimpleReturns_BadRequest_FromUnknownUser() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            var details = new OrderController.SimpleOrderDetails() {
                ProductIds = [101],
                ProductQuantities = [1]
            };
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));

            var tmp = await controller.CreateSimple(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid User", result.Value);
        }

        [Fact]
        public async Task CreateSimpleReturns_BadRequest_FromUnknownCustomer() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            var details = new OrderController.SimpleOrderDetails() {
                ProductIds = [101],
                ProductQuantities = [1]
            };
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));
            var customers = new Mock<DbSet<Customer>>();
            objects.dataContextMock.Object.Customers = customers.Object;
            objects.userManagerMock.Setup(_ => _.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new User()));

            var tmp = await controller.CreateSimple(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid Customer", result.Value);
        }

        [Fact]
        public async Task CreateSimpleReturns_Ok() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            var details = new OrderController.SimpleOrderDetails() {
                ProductIds = [101],
                ProductQuantities = [1]
            };
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));
            var customers = new Mock<DbSet<Customer>>();
            customers.Setup(_ => _.FindAsync(It.IsAny<Guid>())).Returns(ValueTask.FromResult(new Customer()));
            objects.dataContextMock.Object.Customers = customers.Object;
            objects.userManagerMock.Setup(_ => _.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new User()));
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;

            var tmp = await controller.CreateSimple(details);
            var result = tmp.Result as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
        }

        [Fact]
        public async Task CreateSimpleReturns_401_ForDiffProductInfoLen() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            var details = new OrderController.SimpleOrderDetails() {
                ProductIds = [101],
                ProductQuantities = [1,2]
            };
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));
            var customers = new Mock<DbSet<Customer>>();
            customers.Setup(_ => _.FindAsync(It.IsAny<Guid>())).Returns(ValueTask.FromResult(new Customer()));
            objects.dataContextMock.Object.Customers = customers.Object;
            objects.userManagerMock.Setup(_ => _.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new User()));
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;

            var tmp = await controller.CreateSimple(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Length of Product IDs does not match length of Product Quantities", result.Value);
        }

        [Fact]
        public async Task CreateSimpleReturns_400_ForUnkownCustomer() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            var details = new OrderController.SimpleOrderDetails() {
                ProductIds = [101],
                ProductQuantities = [1]
            };
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));
            var customers = new Mock<DbSet<Customer>>();
            objects.dataContextMock.Object.Customers = customers.Object;
            objects.userManagerMock.Setup(_ => _.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new User()));
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;

            var tmp = await controller.CreateSimple(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid Customer", result.Value);
        }

        [Fact]
        public async Task CreateOrderReturns_BadRequest_FromUnevenListLens() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var details = new OrderController.OrderDetails() {
                ProductIds = [101],
                ProductQuantities = [1,2],
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom",
                CustomerEmail = "a@a.com"
            };

            var tmp = await controller.CreateOrder(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Length of Product IDs does not match length of Product Quantities", result.Value);
        }

        [Fact]
        public async Task CreateOrderReturns_BadRequest_FromUnkownCustomer() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var details = new OrderController.OrderDetails() {
                ProductIds = [101],
                ProductQuantities = [1],
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom",
                CustomerEmail = "a@a.com"
            };
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", 1, 1)));
            var customers = new Mock<DbSet<Customer>>();
            objects.dataContextMock.Object.Customers = customers.Object;
            objects.userManagerMock.Setup(_ => _.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new User()));
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;

            var tmp = await controller.CreateOrder(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid Customer", result.Value);
        }

        [Fact]
        public async Task CreateOrderReturns_Ok() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var details = new OrderController.OrderDetails() {
                ProductIds = [101],
                ProductQuantities = [1],
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom",
                CustomerEmail = "a@a.com"
            };
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));
            var customers = new Mock<DbSet<Customer>>();
            customers.Setup(_ => _.FindAsync(It.IsAny<Guid>())).Returns(ValueTask.FromResult(new Customer()));
            objects.dataContextMock.Object.Customers = customers.Object;
            objects.userManagerMock.Setup(_ => _.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new User()));
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;

            var tmp = await controller.CreateOrder(details);
            var result = tmp.Result as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
        }

        [Fact]
        public async Task AdminCreateOrderReturns_Ok() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var details = new OrderController.AdminOrderDetails() {
                ProductIds = [101],
                ProductQuantities = [1],
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom",
                CustomerEmail = "a@a.com",
                Status = OrderStatuses.Statuses.ORDERED,
            };
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));
            var customers = new Mock<DbSet<Customer>>();
            customers.Setup(_ => _.FindAsync(It.IsAny<Guid>())).Returns(ValueTask.FromResult(new Customer()));
            objects.dataContextMock.Object.Customers = customers.Object;
            objects.userManagerMock.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).Returns(Task.FromResult(new User()));
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;

            var tmp = await controller.AdminCreateOrder(details);
            var result = tmp.Result as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
        }

        [Fact]
        public async Task AdminCreateOrderReturns_400_ForDiffProductInfoLens() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var details = new OrderController.AdminOrderDetails() {
                ProductIds = [101],
                ProductQuantities = [1,1],
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom",
                CustomerEmail = "a@a.com",
                Status = OrderStatuses.Statuses.ORDERED,
            };
            var products = new Mock<DbSet<Product>>();

            var tmp = await controller.AdminCreateOrder(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Length of Product IDs does not match length of Product Quantities", result.Value);
        }

        [Fact]
        public async Task AdminCreateOrderReturns_400_ForUnkownCustomer() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var details = new OrderController.AdminOrderDetails() {
                ProductIds = [101],
                ProductQuantities = [1],
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom",
                CustomerEmail = "a@a.com",
                Status = OrderStatuses.Statuses.ORDERED,
            };
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));
            var customers = new Mock<DbSet<Customer>>();
            objects.dataContextMock.Object.Customers = customers.Object;
            objects.userManagerMock.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).Returns(Task.FromResult(new User()));
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;

            var tmp = await controller.AdminCreateOrder(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid Customer", result.Value);
        }

        [Fact]
        public async Task UpdateOrderStatusReturns_Ok() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;
            var id = new Guid();
            var details = new OrderController.OrderStatusUpdate() {
                OrderId = id,
                Status = OrderStatuses.Statuses.CANCELED,
            };
            orders.Setup(_ => _.Find(It.IsAny<Guid>())).Returns(new Order());

            var tmp = await controller.UpdateOrderStatus(details);
            var result = tmp.Result as OkResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task UpdateOrderStatusReturns_400_ForBadOrderId() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;
            var id = new Guid();
            var details = new OrderController.OrderStatusUpdate() {
                OrderId = id,
                Status = OrderStatuses.Statuses.CANCELED,
            };

            var tmp = await controller.UpdateOrderStatus(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid Order Id", result.Value);
        }

        [Fact]
        public async Task CustomerUpdateOrderReturns_Ok() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var id = new Guid();
            var details = new OrderController.CustomerOrderDetailsUpdate() {
                ProductIds = [101],
                ProductQuantities = [1],
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom",
                OrderId = id,
            };
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));
            var customers = new Mock<DbSet<Customer>>();
            customers.Setup(_ => _.FindAsync(It.IsAny<Guid>())).Returns(ValueTask.FromResult(new Customer()));
            objects.dataContextMock.Object.Customers = customers.Object;
            objects.userManagerMock.Setup(_ => _.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new User()));
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;
            orders.Setup(_ => _.Find(It.IsAny<Guid>())).Returns(new Order() { Status = OrderStatuses.ORDERED });

            var tmp = await controller.CustomerUpdateOrder(details);
            var result = tmp.Result as OkResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task CustomerUpdateOrderReturns_400_ForUnderwayOrder() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var id = new Guid();
            var details = new OrderController.CustomerOrderDetailsUpdate() {
                ProductIds = [101],
                ProductQuantities = [1],
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom",
                OrderId = id,
            };
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));
            var customers = new Mock<DbSet<Customer>>();
            customers.Setup(_ => _.FindAsync(It.IsAny<Guid>())).Returns(ValueTask.FromResult(new Customer()));
            objects.dataContextMock.Object.Customers = customers.Object;
            objects.userManagerMock.Setup(_ => _.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new User()));
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;
            orders.Setup(_ => _.Find(It.IsAny<Guid>())).Returns(new Order() { Status = OrderStatuses.SHIPPED });

            var tmp = await controller.CustomerUpdateOrder(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Order underway or cancelled", result.Value);
        }

        [Fact]
        public async Task CustomerUpdateOrderReturns_400_ForUnkownCustomer() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var id = new Guid();
            var details = new OrderController.CustomerOrderDetailsUpdate() {
                ProductIds = [101],
                ProductQuantities = [1],
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom",
                OrderId = id,
            };
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));
            var customers = new Mock<DbSet<Customer>>();
            objects.dataContextMock.Object.Customers = customers.Object;
            objects.userManagerMock.Setup(_ => _.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new User()));
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;
            orders.Setup(_ => _.Find(It.IsAny<Guid>())).Returns(new Order() { Status = OrderStatuses.ORDERED });

            var tmp = await controller.CustomerUpdateOrder(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid Customer", result.Value);
        }

        [Fact]
        public async Task CustomerUpdateOrderReturns_400_ForInvalidOrderId () {
            var objects = CreateObjects();
            var controller = objects.controller;
            var id = new Guid();
            var details = new OrderController.CustomerOrderDetailsUpdate() {
                ProductIds = [101],
                ProductQuantities = [1],
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom",
                OrderId = id,
            };
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));
            var customers = new Mock<DbSet<Customer>>();
            customers.Setup(_ => _.FindAsync(It.IsAny<Guid>())).Returns(ValueTask.FromResult(new Customer()));
            objects.dataContextMock.Object.Customers = customers.Object;
            objects.userManagerMock.Setup(_ => _.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new User()));
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;

            var tmp = await controller.CustomerUpdateOrder(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid Order Id", result.Value);
        }

        [Fact]
        public async Task CustomerUpdateOrderReturns_400_ForDiffProductInfoLen() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var id = new Guid();
            var details = new OrderController.CustomerOrderDetailsUpdate() {
                ProductIds = [101],
                ProductQuantities = [1,1],
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom",
                OrderId = id,
            };
            var products = new Mock<DbSet<Product>>();
            
            var tmp = await controller.CustomerUpdateOrder(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Length of Product IDs does not match length of Product Quantities", result.Value);
        }

        [Fact]
        public async Task AdminUpdateOrderReturns_Ok() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var id = new Guid();
            var details = new OrderController.AdminOrderDetailsUpdate() {
                ProductIds = [101],
                ProductQuantities = [1],
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom",
                OrderId = id,
                CustomerEmail = "a@a.com",
                Status = OrderStatuses.Statuses.CANCELED,

            };
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));
            var customers = new Mock<DbSet<Customer>>();
            customers.Setup(_ => _.FindAsync(It.IsAny<Guid>())).Returns(ValueTask.FromResult(new Customer()));
            objects.dataContextMock.Object.Customers = customers.Object;
            objects.userManagerMock.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).Returns(Task.FromResult(new User()));
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;
            orders.Setup(_ => _.Find(It.IsAny<Guid>())).Returns(new Order() { Status = OrderStatuses.ORDERED });

            var tmp = await controller.AdminUpdateOrder(details);
            var result = tmp.Result as OkResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task AdminUpdateOrderReturns_400_ForInvalidCustomer() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var id = new Guid();
            var details = new OrderController.AdminOrderDetailsUpdate() {
                ProductIds = [101],
                ProductQuantities = [1],
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom",
                OrderId = id,
                CustomerEmail = "a@a.com",
                Status = OrderStatuses.Statuses.CANCELED,

            };
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));
            var customers = new Mock<DbSet<Customer>>();
            objects.dataContextMock.Object.Customers = customers.Object;
            objects.userManagerMock.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).Returns(Task.FromResult(new User()));
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;
            orders.Setup(_ => _.Find(It.IsAny<Guid>())).Returns(new Order() { Status = OrderStatuses.ORDERED });

            var tmp = await controller.AdminUpdateOrder(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid Customer", result.Value);
        }

        [Fact]
        public async Task AdminUpdateOrderReturns_400_ForInvalidOrderId() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var id = new Guid();
            var details = new OrderController.AdminOrderDetailsUpdate() {
                ProductIds = [101],
                ProductQuantities = [1],
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom",
                OrderId = id,
                CustomerEmail = "a@a.com",
                Status = OrderStatuses.Statuses.CANCELED,

            };
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));
            var customers = new Mock<DbSet<Customer>>();
            customers.Setup(_ => _.FindAsync(It.IsAny<Guid>())).Returns(ValueTask.FromResult(new Customer()));
            objects.dataContextMock.Object.Customers = customers.Object;
            objects.userManagerMock.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).Returns(Task.FromResult(new User()));
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;

            var tmp = await controller.AdminUpdateOrder(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid Order Id", result.Value);
        }

        [Fact]
        public async Task AdminUpdateOrderReturns_400_ForDiffProductInfoLens() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var id = new Guid();
            var details = new OrderController.AdminOrderDetailsUpdate() {
                ProductIds = [101],
                ProductQuantities = [1,1],
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom",
                OrderId = id,
                CustomerEmail = "a@a.com",
                Status = OrderStatuses.Statuses.CANCELED,

            };
            var products = new Mock<DbSet<Product>>();

            var tmp = await controller.AdminUpdateOrder(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Length of Product IDs does not match length of Product Quantities", result.Value);
        }

        [Fact]
        public async Task GetReturns_Ok() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;
            orders.Setup(_ => _.FindAsync(It.IsAny<Guid>())).Returns(ValueTask.FromResult(new Order() {
                Products = [new Product("A", "A", 1, 1)],
                ProductQuantities = [1],
                Address = "A Street",
                Customer = new Customer(),
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom",
                DateCreated = DateTime.Now,
            }));

            var tmp = await controller.Get(new Guid());
            var result = tmp.Result as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task GetReturns_400_ForBadOrderId() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var orders = new Mock<DbSet<Order>>();
            objects.dataContextMock.Object.Orders = orders.Object;

            var tmp = await controller.Get(new Guid());
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid Order Id", result.Value);
        }
    }
}
