using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SimpleOnlineStore_Dotnet.Controllers;
using SimpleOnlineStore_Dotnet.Models;
using System.Net;
using System.Security.Claims;

namespace SimpleOnloneStore_Dotnet.Api.Tests {
    public class HelloControllerTest {
        private HelloController CreateController() {
            var userManagerMock = new Mock<UserManager<User>>(
                /* IUserStore<TUser> store */Mock.Of<IUserStore<User>>(),
                /* IOptions<IdentityOptions> optionsAccessor */null,
                /* IPasswordHasher<TUser> passwordHasher */null,
                /* IEnumerable<IUserValidator<TUser>> userValidators */null,
                /* IEnumerable<IPasswordValidator<TUser>> passwordValidators */null,
                /* ILookupNormalizer keyNormalizer */null,
                /* IdentityErrorDescriber errors */null,
                /* IServiceProvider services */null,
                /* ILogger<UserManager<TUser>> logger */null);

            return new HelloController(userManagerMock.Object);
        }

        [Fact]
        public void StandardHelloReturns_Ok() {
            // Arrange
            var controller = CreateController();

            // Act
            var tmp = controller.Hello();
            var result = tmp.Result as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Hello!", result.Value);
        }

        [Fact]
        public void AdminHelloReturns_Ok() {
            // Arrange
            var controller = CreateController();

            // Act
            var tmp = controller.HelloAdmin();
            var result = tmp.Result as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Hello admin!", result.Value);
        }

        [Fact]
        public void CustomerHelloReturns_Ok() {
            // Arrange
            var controller = CreateController();

            // Act
            var tmp = controller.HelloCustomer();
            var result = tmp.Result as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Hello customer!", result.Value);
        }

        [Fact]
        public async Task AuthHelloReturns_Ok() {
            // Arrange
            var userManagerMock = new Mock<UserManager<User>>(
                /* IUserStore<TUser> store */Mock.Of<IUserStore<User>>(),
                /* IOptions<IdentityOptions> optionsAccessor */null,
                /* IPasswordHasher<TUser> passwordHasher */null,
                /* IEnumerable<IUserValidator<TUser>> userValidators */null,
                /* IEnumerable<IPasswordValidator<TUser>> passwordValidators */null,
                /* ILookupNormalizer keyNormalizer */null,
                /* IdentityErrorDescriber errors */null,
                /* IServiceProvider services */null,
                /* ILogger<UserManager<TUser>> logger */null);
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(new User("John")));

            var controller = new HelloController(userManagerMock.Object);
            controller.ControllerContext.HttpContext = Mock.Of<HttpContext>();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

            // Act
            var tmp = await controller.HelloAuthAsync();
            var result = tmp.Result as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Hello authenticated John!", result.Value);
        }

        [Fact]
        public async Task AuthHelloReturns_BadRequest_FromMissingUser() {
            // Arrange
            var controller = CreateController();
            var tmp = await controller.HelloAuthAsync();

            // Act
            var result = tmp.Result as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(((int)HttpStatusCode.BadRequest), result.StatusCode);
        }
    }
}
