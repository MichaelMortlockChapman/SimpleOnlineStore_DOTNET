using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleOnlineStore_Dotnet.Data;
using SimpleOnlineStore_Dotnet.Models;
using System.Linq;

namespace SimpleOnlineStore_Dotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly SOSContext _sosContext;

        public ProductController(SOSContext sosContext)
        {
            _sosContext = sosContext;
        }

        [HttpGet("[action]")]
        public List<Product> GetAll()
        {
            return _sosContext.Products.ToList();
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<string>> Add(ProductDetails productDetails)
        {
            Product product = new(productDetails.Name, productDetails.Description, productDetails.Price, productDetails.Stock);
            await _sosContext.Products.AddAsync(product);
            await _sosContext.SaveChangesAsync();
            return Ok(product.Id);
        }
    }
}
