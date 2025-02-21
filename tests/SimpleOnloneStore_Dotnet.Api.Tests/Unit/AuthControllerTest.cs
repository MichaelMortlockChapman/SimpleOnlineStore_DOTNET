using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SimpleOnlineStore_Dotnet.Controllers;
using SimpleOnlineStore_Dotnet.Data;
using SimpleOnlineStore_Dotnet.Models;
using System.Net;

namespace SimpleOnloneStore_Dotnet.Api.Tests {
    public class AuthControllerTest {
        private class InitObjects {
            public required Mock<UserManager<User>> userManagerMock;
            public required Mock<SignInManager<User>> signInManagerMock;
            public required Mock<DataContext> dataContextMock;
            public required AuthController controller;
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
            var signInManagerMock = new Mock<SignInManager<User>>(
                userManagerMock.Object,
                /* IHttpContextAccessor contextAccessor */Mock.Of<IHttpContextAccessor>(),
                /* IUserClaimsPrincipalFactory<TUser> claimsFactory */Mock.Of<IUserClaimsPrincipalFactory<User>>(),
                /* IOptions<IdentityOptions> optionsAccessor */null,
                /* ILogger<SignInManager<TUser>> logger */null,
                /* IAuthenticationSchemeProvider schemes */null,
                /* IUserConfirmation<TUser> confirmation */null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            var dataContextMock = new Mock<DataContext>();
            var loggerMock = new Mock<ILogger<AuthController>>();

            return new InitObjects {
                userManagerMock = userManagerMock,
                signInManagerMock = signInManagerMock,
                dataContextMock = dataContextMock,
                controller = new AuthController(userManagerMock.Object, signInManagerMock.Object, loggerMock.Object, dataContextMock.Object),
            };
        }

        [Fact]
        public async Task RegisterReturns_BadRequest_WithDupEmail() {
            var objects = CreateObjects();
            var controller = objects.controller;
            objects.userManagerMock.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).Returns(Task.FromResult(new User()));
            var details = new AuthController.RegisterFormDetails {
                Email = "a@a.com",
                Password = "strongPassword",
                Name = "A",
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom"
            };

            var tmp = await controller.Register(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Username already exists", result.Value);
        }

        [Fact]
        public async Task RegisterReturns_BadRequest_ForUserCreationFailure() {
            var objects = CreateObjects();
            var controller = objects.controller;
            objects.userManagerMock.Setup(_ => _.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).Returns(Task.FromResult(new IdentityResult()));
            var details = new AuthController.RegisterFormDetails {
                Email = "a@a.com",
                Password = "strongPassword",
                Name = "A",
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom"
            };

            var tmp = await controller.Register(details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task RegisterReturns_Ok_ForUserCreationSuccess() {
            var objects = CreateObjects();
            var controller = objects.controller;
            objects.userManagerMock.Setup(_ => _.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).Returns(Task.FromResult(IdentityResult.Success));
            var details = new AuthController.RegisterFormDetails {
                Email = "a@a.com",
                Password = "strongPassword",
                Name = "A",
                Address = "A Street",
                City = "A City",
                PostalCode = 1001,
                Country = "A Kingdom"
            };

            var tmp = await controller.Register(details);
            var result = tmp.Result as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task LoginReturns_401_ForUnknownEmail() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var details = new AuthController.LoginFormDetails {
                Email = "a@a.com",
                Password = "strongPassword",
            };

            var tmp = await controller.Login(details);
            var result = tmp.Result as UnauthorizedResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task LoginReturns_401_ForBadPassword() {
            var objects = CreateObjects();
            var controller = objects.controller;
            objects.userManagerMock.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).Returns(Task.FromResult(new User()));
            objects.signInManagerMock.Setup(_ => _.CheckPasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), false)).Returns(Task.FromResult(new Microsoft.AspNetCore.Identity.SignInResult()));
            var details = new AuthController.LoginFormDetails {
                Email = "a@a.com",
                Password = "strongPassword",
            };

            var tmp = await controller.Login(details);
            var result = tmp.Result as UnauthorizedResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }
        [Fact]
        public async Task LoginReturns_Ok_ForCorrectPassword() {
            var objects = CreateObjects();
            var controller = objects.controller;
            objects.userManagerMock.Setup(_ => _.FindByEmailAsync(It.IsAny<string>())).Returns(Task.FromResult(new User()));
            objects.signInManagerMock.Setup(_ => _.CheckPasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), false))
                .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Success));
            var details = new AuthController.LoginFormDetails {
                Email = "a@a.com",
                Password = "strongPassword",
            };

            var tmp = await controller.Login(details);
            var result = tmp.Result as OkResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }
    }
}