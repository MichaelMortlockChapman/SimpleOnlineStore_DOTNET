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
            var signInManagerMock = new Mock<SignInManager<User>>(
                 userManagerMock.Object,
                 /* IHttpContextAccessor contextAccessor */Mock.Of<IHttpContextAccessor>(),
                 /* IUserClaimsPrincipalFactory<TUser> claimsFactory */Mock.Of<IUserClaimsPrincipalFactory<User>>(),
                 /* IOptions<IdentityOptions> optionsAccessor */null,
                 /* ILogger<SignInManager<TUser>> logger */null,
                 /* IAuthenticationSchemeProvider schemes */null,
                 /* IUserConfirmation<TUser> confirmation */null);

            return new HelloController(signInManagerMock.Object);
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
            userManagerMock.Setup(x => x.GetUserAsync(new ClaimsPrincipal())).Returns(Task.FromResult(new User()));

            var context = new Mock<HttpContext>();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(x => x.HttpContext).Returns(context.Object);
            contextAccessor.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal());

            var signInManagerMock = new Mock<SignInManager<User>>(
                 userManagerMock.Object,
                 /* IHttpContextAccessor contextAccessor */contextAccessor.Object,
                 /* IUserClaimsPrincipalFactory<TUser> claimsFactory */Mock.Of<IUserClaimsPrincipalFactory<User>>(),
                 /* IOptions<IdentityOptions> optionsAccessor */null,
                 /* ILogger<SignInManager<TUser>> logger */null,
                 /* IAuthenticationSchemeProvider schemes */null,
                 /* IUserConfirmation<TUser> confirmation */null);
            //signInManagerMock.SetupProperty(_ => _.Context.User, new ClaimsPrincipal());

            var controller = new HelloController(signInManagerMock.Object);

            // Act
            var tmp = await controller.HelloAuthAsync();
            var result = tmp.Result as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Hello admin!", result.Value);
        }
    }
}
