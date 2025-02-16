using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Moq;
using SimpleOnlineStore_Dotnet.Controllers;
using SimpleOnlineStore_Dotnet.Data;
using SimpleOnlineStore_Dotnet.Models;
using System.Collections;
using System.Linq.Expressions;
using System.Net;

namespace SimpleOnloneStore_Dotnet.Api.Tests {
    public class ProductControllerTest {
        private class InitObjects {
            public required Mock<DataContext> dataContextMock;
            public required ProductController controller;
        }

        private InitObjects CreateObjects() {
            var dataContextMock = new Mock<DataContext>();

            return new InitObjects {
                dataContextMock = dataContextMock,
                controller = new ProductController(dataContextMock.Object),

            };
        }

        [Fact]
        public async Task CreateReturns_Created() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            var details = new ProductDetails() {
                Name = "A",
                Description = "A"
            };

            var tmp = await controller.Create(details);
            var result = tmp.Result as CreatedAtActionResult; 

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
        }

        [Fact]
        public async Task DeleteReturns_BadRequest_ForUnknownProduct() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;

            var tmp = await controller.Delete(1);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Unknown Product ID", result.Value);
        }

        [Fact]
        public async Task DeleteReturns_Ok() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));

            var tmp = await controller.Delete(1);
            var result = tmp.Result as OkResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task UpdateReturns_BadRequest_ForUnknownProduct() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            var details = new ProductDetails() {
                Name = "B",
                Description = "B"
            };

            var tmp = await controller.Update(1, details);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Unknown Product ID", result.Value);
        }

        [Fact]
        public async Task UpdateReturns_Ok() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));
            var details = new ProductDetails() {
                Name = "B",
                Description = "B"
            };

            var tmp = await controller.Update(1, details);
            var result = tmp.Result as OkResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task GetReturns_BadRequest_ForUnknownProduct() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;

            var tmp = await controller.Get(1);
            var result = tmp.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Unknown Product ID", result.Value);
        }

        [Fact]
        public async Task GetReturns_Ok() {
            var objects = CreateObjects();
            var controller = objects.controller;
            var products = new Mock<DbSet<Product>>();
            objects.dataContextMock.Object.Products = products.Object;
            products.Setup(_ => _.FindAsync(It.IsAny<int>())).Returns(ValueTask.FromResult(new Product(
                "A", "A", (double)1, 1)));

            var tmp = await controller.Get(1);
            var result = tmp.Result as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }
    }
}
