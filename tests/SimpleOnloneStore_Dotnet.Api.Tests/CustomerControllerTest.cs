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
    public class CustomerControllerTest {
        private class InitObjects {
            public required Mock<UserManager<User>> userManagerMock;
            public required Mock<DataContext> dataContextMock;
            public required CustomerController controller;
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
                userManagerMock = userManagerMock,
                dataContextMock = dataContextMock,
                controller = new CustomerController(dataContextMock.Object, userManagerMock.Object),
            };
        }

        [Fact]
        public async Task GetCustomerDetails_ReturnsBadRequest_ForUnknownUser() {
            var objects = CreateObjects();
            var controller = objects.controller;

            var tmp = await controller.GetCustomerDetails();
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid User", result.Value);
        }

        [Fact]
        public async Task GetCustomerDetails_ReturnsBadRequest_ForUnknownCustomer() {
            var objects = CreateObjects();
            var controller = objects.controller;
            objects.userManagerMock.Setup(_ => _.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new User()));
            var customers = new Mock<DbSet<Customer>>();
            objects.dataContextMock.Object.Customers = customers.Object;

            var tmp = await controller.GetCustomerDetails();
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid User", result.Value);
        }

        [Fact]
        public async Task GetCustomerDetails_ReturnsOk() {
            var objects = CreateObjects();
            var controller = objects.controller;
            objects.userManagerMock.Setup(_ => _.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new User()));
            var customers = new Mock<DbSet<Customer>>();
            customers.Setup(_ => _.FindAsync(It.IsAny<Guid>())).Returns(ValueTask.FromResult(new Customer()));
            objects.dataContextMock.Object.Customers = customers.Object;

            var tmp = await controller.GetCustomerDetails();
            var result = tmp.Result as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("{Name:,Address:,City:,PostalCode:0,Country:,}", result.Value);
        }

        [Fact]
        public async Task UpdateCustomerDetails_ReturnsBadRequest_ForUnknownUser() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var details = new CustomerController.CustomerDetails() {
                Name = "A",
                Address = "A Street",
                City = "A City",
                PostalCode = 101,
                Country = "A Kingdom"
            };

            var tmp = await controller.UpdateCustomerDetails(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid User", result.Value);
        }

        [Fact]
        public async Task UpdateCustomerDetails_ReturnsBadRequest_ForUnknownCustomer() {
            var objects = CreateObjects();
            var controller = objects.controller;
            objects.userManagerMock.Setup(_ => _.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new User()));
            var customers = new Mock<DbSet<Customer>>();
            objects.dataContextMock.Object.Customers = customers.Object;
            var details = new CustomerController.CustomerDetails() {
                Name = "A",
                Address = "A Street",
                City = "A City",
                PostalCode = 101,
                Country = "A Kingdom"
            };

            var tmp = await controller.UpdateCustomerDetails(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid User", result.Value);
        }

        [Fact]
        public async Task UpdateCustomerDetails_ReturnsOk() {
            var objects = CreateObjects();
            var controller = objects.controller;
            objects.userManagerMock.Setup(_ => _.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new User()));
            var customers = new Mock<DbSet<Customer>>();
            customers.Setup(_ => _.FindAsync(It.IsAny<Guid>())).Returns(ValueTask.FromResult(new Customer()));
            objects.dataContextMock.Object.Customers = customers.Object;
            var details = new CustomerController.CustomerDetails() {
                Name = "A",
                Address = "A Street",
                City = "A City",
                PostalCode = 101,
                Country = "A Kingdom"
            };

            var tmp = await controller.UpdateCustomerDetails(details);
            var result = tmp.Result as OkResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }
    }
}
