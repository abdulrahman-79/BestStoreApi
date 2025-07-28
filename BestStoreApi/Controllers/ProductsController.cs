using BestStoreApi.Models;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace BestStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            this._context = context;
            this._env = env;
        }


        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            var listCategories = _context.Categories.ToList();
            return Ok(listCategories);
        }

        [HttpGet]
        public IActionResult GetProducts(string? search, string? category,
            int? minPrice, int? maxPrice,
            string? sort, string? order,
            int? page)
        {
            IQueryable<Products> query = _context.Products;

            //pagination functionality
            if (page == null || page < 1) page = 1;

            int pageSize = 5;
            int totalpages;

            decimal count = query.Count();
            totalpages = (int)Math.Ceiling(count / pageSize);

            query = query.Skip((int)(page - 1) * pageSize).Take(pageSize);
            //search functionality
            if(search != null)
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            if(category != null)
            {
                query = query.Where(p => p.Category.Name == category);
            }

            if(minPrice != null)
            {
                query = query.Where(p => p.Price >= minPrice);
            }

            if (maxPrice != null)
            {
                query = query.Where(p => p.Price <= maxPrice);
            }

            //sort functionality
            if (sort == null) sort = "id";
            if (order == null || order != "desc") order = "asc";

            if (sort.ToLower() == "name")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Name);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Name);
                }
            }

            else if (sort.ToLower() == "brand")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Brand);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Brand);
                }
            }

            else if (sort.ToLower() == "category")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Category.Name);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Category.Name);
                }
            }

            else if (sort.ToLower() == "price")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Price);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Price);
                }
            }

            else if (sort.ToLower() == "date" || sort.ToLower() == "createdat")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.CreatedAt);
                }
                else
                {
                    query = query.OrderByDescending(p => p.CreatedAt);
                }
            }

            else
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Id);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id);
                }
            }



            var products = query.ToList();

            var response = new
            {
                Products = products,
                TotalPages = totalpages,
                PageSize = pageSize,
                Page = page
            };

            return Ok(response);
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

        [HttpPost]
        public IActionResult CreateProduct([FromForm] ProductDto productDto)
        {
            var category = _context.Categories.Find(productDto.CategoryId);
            if (category is null)
            {
                ModelState.AddModelError("Category", "Please select a valid category");
                return BadRequest(ModelState);
            }

            if(productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The Image File is Required");
                return BadRequest(ModelState);
            }
            string imageFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            imageFileName += Path.GetExtension(productDto.ImageFile.FileName);

            string imagesFolder = _env.WebRootPath + "/Images/Products/";

            using (var stream = System.IO.File.Create(imagesFolder + imageFileName))
            {
                productDto.ImageFile.CopyTo(stream);
            }

            Products product = new Products()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = category,
                Price = productDto.Price,
                Description = productDto.Description ?? "",
                ImageFileName = imageFileName,
                CreatedAt = DateTime.Now
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            return Ok(product);
        }

        [HttpPut("{Id}")]
        public IActionResult UpdateProduct(int Id, [FromForm]ProductDto productDto)
        {
            var category = _context.Categories.Find(productDto.CategoryId);
            if (category is null)
            {
                ModelState.AddModelError("Category", "Please select a valid category");
                return BadRequest(ModelState);
            }

            var product = _context.Products.Find(Id);

            if (product == null)
            {
                return NotFound();
            }

            string imageFileName = product.ImageFileName;
            if (productDto.ImageFile != null)
            {
                // Save new image
                imageFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                imageFileName += Path.GetExtension(productDto.ImageFile.FileName);
                
                string imagesFolder = _env.WebRootPath + "/Images/Products/";

                using (var stream = System.IO.File.Create(imagesFolder + imageFileName))
                {
                    productDto.ImageFile.CopyTo(stream);
                }

                // Delete Old Image
                System.IO.File.Delete(imagesFolder + product.ImageFileName);
            }
            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = category;
            product.Price = productDto.Price;
            product.Description = productDto.Description ?? "";
            product.ImageFileName = imageFileName;

            _context.SaveChanges();

            return Ok(product);
        }

        [HttpDelete("{Id}")]
        public IActionResult DeleteProduct(int Id)
        {
            var product = _context.Products.Find(Id);

            if (product == null)
            {
                return NotFound();
            }

            string imagesFolder = _env.WebRootPath + "/Images/Products/";
            System.IO.File.Delete(imagesFolder + product.ImageFileName);

            _context.Remove(product);
            _context.SaveChanges();
            return Ok();
        }

    }
}
