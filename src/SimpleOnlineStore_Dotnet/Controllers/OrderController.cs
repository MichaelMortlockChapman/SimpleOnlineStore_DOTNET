using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SimpleOnlineStore_Dotnet.Data;
using SimpleOnlineStore_Dotnet.Models;

namespace SimpleOnlineStore_Dotnet.Controllers
{
    //[Authorize]
    [Route("[controller]")]
    [ApiController]
    public class OrderController : ControllerBase {
        private readonly DataContext _dataContext;

        public OrderController(DataContext dataContext) {
            _dataContext = dataContext;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<ActionResult<string>> CreateSimple(
            [FromBody] List<(int, int)> products
        ) {
            
            //List<(Product, int)> _products = new();
            //foreach (var productEntry in products) {
            //    Product p = await _dataContext.Products.FindAsync(productEntry.Item1);
            //    if (p == null) {
            //        return BadRequest("Invalid Product ID");
            //    }
            //    _products.Add((p, productEntry.Item2));
            //}
            //await _dataContext.Orders.AddAsync(new Order(
            //    _products,
            //    null,
            //    "",
            //    "",
            //    1,
            //    "",
            //    ""
            //));
            //await _dataContext.SaveChangesAsync();
            return CreatedAtAction("CreateSimple","done");
        }
    }
}
