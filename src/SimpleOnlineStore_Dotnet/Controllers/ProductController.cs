using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleOnlineStore_Dotnet.Data;
using SimpleOnlineStore_Dotnet.Models;

namespace SimpleOnlineStore_Dotnet.Controllers {
    [Authorize(Policy = "RequireAdminRole")]
    [Route("[controller]")]
    [ApiController]
    public class ProductController : ControllerBase {
        private readonly DataContext _dataContext;

        public ProductController(DataContext dataContext) {
            _dataContext = dataContext;
        }

        public enum ProductSortBy {
            Name,
            Price
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<Product>>> GetNext(int? lastId, ProductSortBy sortBy) {
            var orderedElements = _dataContext.Products.OrderBy(p => p.Name);
            switch (sortBy) {
                default:
                case ProductSortBy.Name:
                    orderedElements = _dataContext.Products.OrderBy(p => p.Name);
                    break;
                case ProductSortBy.Price:
                    orderedElements = _dataContext.Products.OrderBy(p => p.Price);
                    break;
            }

            Product? product = await _dataContext.Products.FindAsync(lastId);
            return orderedElements
                .ThenBy(p => p.Id)
                .Where(p => product == null || p.Id > lastId)
                .Take(10)
                .ToList();
        }

        [AllowAnonymous]
        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<Product>> Get(int id) {
            Product? product = await _dataContext.Products.FindAsync(id);
            if (product == null) {
                return BadRequest("Unknown Product ID");
            }
            return Ok(product);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<string>> Create(ProductDetails productDetails) {
            Product product = new(productDetails.Name, productDetails.Description, productDetails.Price, productDetails.Stock);
            await _dataContext.Products.AddAsync(product);
            await _dataContext.SaveChangesAsync();
            return CreatedAtAction("Create", product.Id);
        }

        [HttpDelete("[action]")]
        public async Task<ActionResult<string>> Delete(int id) {
            Product? product = await _dataContext.Products.FindAsync(id);

            if (product == null) {
                return BadRequest("Unknown Product ID");
            }
            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("[action]")]
        public async Task<ActionResult<string>> Update(int id, ProductDetails productDetails) {
            Product? product = await _dataContext.Products.FindAsync(id);
            if (product == null) {
                return BadRequest("Unknown Product ID");
            }
            product.Name = productDetails.Name;
            product.Description = productDetails.Description;
            product.Price = productDetails.Price;
            product.Stock = productDetails.Stock;
            _dataContext.Products.Update(product);
            await _dataContext.SaveChangesAsync();
            return Ok();
        }
    }
}
