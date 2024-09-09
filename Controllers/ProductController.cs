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
        public ActionResult<string> Add(Product product)
        {
            _sosContext.Products.Add(product);
            return Ok(product.Id);
        }
    }
}
