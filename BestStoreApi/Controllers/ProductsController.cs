using BestStoreApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ProductsController(ApplicationDbContext context)
        {
            this._context = context;
        }

        [HttpGet]
        public IActionResult GetProducts()
        {
            var products = _context.Products.ToList();
            return Ok(products);
        }

        [HttpGet("{Id}")]
        public IActionResult GetProductById(int Id)
        {
            var product = _context.Products.Find(Id);
            if(product is null)
            {
                return NotFound();
            }
            return Ok(product);
        }
    }
}
