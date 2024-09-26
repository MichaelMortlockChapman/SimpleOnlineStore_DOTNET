//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using SimpleOnlineStore_Dotnet.Data;
//using SimpleOnlineStore_Dotnet.Models;
//using System.Linq;

//namespace SimpleOnlineStore_Dotnet.Controllers
//{
//    [Route("/v1/[controller]")]
//    [ApiController]
//    public class ProductController : ControllerBase
//    {
//        private readonly SOSContext _sosContext;

//        public ProductController(SOSContext sosContext)
//        {
//            _sosContext = sosContext;
//        }

//        public enum ProductSortBy
//        {
//            Name,
//            Price
//        }

//        [HttpGet("[action]")]
//        public async Task<ActionResult<List<Product>>> GetNext(Guid? lastId, ProductSortBy sortBy)
//        {
//            var orderedElements = _sosContext.Products.OrderBy(p => p.Name);
//            switch (sortBy)
//            {
//                default:
//                case ProductSortBy.Name:
//                    orderedElements = _sosContext.Products.OrderBy(p => p.Name);
//                    break;
//                case ProductSortBy.Price:
//                    orderedElements = _sosContext.Products.OrderBy(p => p.Price);
//                    break;
//            }

//            Product? product = await _sosContext.Products.FindAsync(lastId);
//            return orderedElements
//                .ThenBy(p => p.Id)
//                .Where(p => product == null || p.Id > lastId)
//                .Take(10)
//                .ToList();
//        }

//        [HttpGet("[action]/{id}")]
//        public async Task<ActionResult<Product>> Get(Guid id)
//        {
//            Product? product = await _sosContext.Products.FindAsync(id);
//            if (product == null)
//            {
//                return BadRequest("Unknown Product ID");
//            }
//            return Ok(product);
//        }

//        [HttpPost("[action]")]
//        public async Task<ActionResult<string>> Create(ProductDetails productDetails)
//        {
//            Product product = new(productDetails.Name, productDetails.Description, productDetails.Price, productDetails.Stock);
//            await _sosContext.Products.AddAsync(product);
//            await _sosContext.SaveChangesAsync();
//            return CreatedAtAction("Create", product.Id);
//        }

//        [HttpDelete("[action]")]
//        public async Task<ActionResult<string>> Delete(Guid id)
//        {
//            Product? product = await _sosContext.Products.FindAsync(id);
           
//            if (product == null)
//            {
//                return BadRequest("Unknown Product ID");
//            }
//            _sosContext.Products.Remove(product);
//            await _sosContext.SaveChangesAsync();
//            return Ok();
//        }

//        [HttpPut("[action]")]
//        public async Task<ActionResult<string>> Update(Guid id, ProductDetails productDetails)
//        {
//            Product? product = await _sosContext.Products.FindAsync(id);
//            if (product == null)
//            {
//                return BadRequest("Unknown Product ID");
//            }
//            product.Name = productDetails.Name;
//            product.Description = productDetails.Description;
//            product.Price = productDetails.Price;
//            product.Stock = productDetails.Stock;
//            _sosContext.Products.Update(product);
//            await _sosContext.SaveChangesAsync();
//            return Ok();
//        }
//    }
//}
