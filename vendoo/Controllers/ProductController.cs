using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using vendoo.Models;
using vendoo.Packages;
using static System.Net.Mime.MediaTypeNames;

namespace vendoo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("MyPolicy")]

    public class ProductController : ControllerBase
    {
        private readonly BasePackage _basepack;
        private readonly IConfiguration _configuration;


        public ProductController(BasePackage managepackage, IConfiguration configuration)
        {
            _basepack = managepackage;
            _configuration = configuration;
        }


        [HttpPost("AddCategories")]
        [Authorize(Roles = "admin")]
        public IActionResult CreateCategories([FromBody] CategoriesModel model)
        {
            try
            {
                var newcompany = _basepack.AddCategories(model);
                if (newcompany)
                {
                    return Ok(new { message = "category created successfully" });
                }
                else
                {
                    return StatusCode(500, "Failed to create category");
                }
            }
            
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating user: {ex.Message}");
            }
        }
        [HttpPost("AddCategories1Child")]
        [Authorize(Roles = "admin")]

        public IActionResult CreateCategories1Child([FromBody] CategoryModel model)
        {
            try
            {
                var newcompany = _basepack.AddCategories1StChild(model);
                if (newcompany)
                {
                    return Ok(new { message = "category created successfully" });
                }
                else
                {
                    return StatusCode(500, "Failed to create category");
                }
            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating user: {ex.Message}");
            }
        }
        

        [HttpPost("AddItemDescription/{category_id}")]
       // [Authorize(Roles = "admin")]

        public async Task<IActionResult> CreateItemDescription([FromForm] ItemDescriptionModel model,int category_id)
        {
            try
            {
                if (!int.TryParse(model.Category_id.ToString(), out int categoryId))
                {
                    return BadRequest("Invalid category ID. It must be a number.");
                }

                model.Category_id = categoryId;
                if (model.Image_path == null || !model.Image_path.Any())
                {
                    return BadRequest("No images uploaded.");
                }

                var imagePaths = await SaveImagesFromIFormFileAsync(model.Image_path);

                if (imagePaths == null || !imagePaths.Any())
                {
                    return BadRequest("No images were saved.");
                }
                var newcompany = _basepack.AddItemDescription(model,imagePaths,category_id);
                if (await newcompany)
                {
                    return Ok(new { message = "product created successfully",item=model });
                }
                else
                {
                    return StatusCode(500, "Failed to create category");
                }
            }
            catch (OracleException ex)
            {
                return StatusCode(500, new
                {
                    Message =model,
                    ErrorCode = ex.ErrorCode,
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating user: {ex.Message}");
            }



        }
        private async Task<List<string>> SaveImagesFromIFormFileAsync(List<IFormFile> files)
        {
            var savedPaths = new List<string>();
            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Upload", "Files");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            foreach (var file in files)
            {
                try
                {
                    string fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    savedPaths.Add(Path.Combine("Upload", "Files", fileName));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving image:");
                }
            }

            return savedPaths;
        }


        [HttpGet("GetCategories")]
        public IActionResult GetCategories()
        {
            try
            {
                var cat = _basepack.GetCategories();
                if (cat != null)
                {
                    return Ok(cat);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting categories: {ex.Message}");
            }
        }
        [HttpGet("GetCategories1st/{id}")]
        public IActionResult GetCategories1st(int id)
        {
            try
            {
                var cat = _basepack.GetCategories1stChild(id);
                if (cat != null)
                {
                    return Ok(cat);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting categories: {ex.Message}");
            }
        }
        [HttpGet("GetFilteredItems/{id}/{category_name}")]
        public IActionResult GetCategories2st(int id,string category_name)
        {
            try
            {
                var cat = _basepack.GetFilteredItems(id,category_name);
                if (cat != null)
                {
                    return Ok(cat);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting categories: {ex.Message}");
            }
        }
        [HttpGet("GetSearchedProducts/{product_name}")]
        public IActionResult GetSearchedProducts(string product_name)
        {
            try
            {
                var cat = _basepack.GetSearchedPRoducts(product_name);
                if (cat != null)
                {
                    return Ok(cat);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting categories: {ex.Message}");
            }
        }
        [HttpGet("GetProducts/")]
        public IActionResult GetProducts()
        {
            try
            {
                var cat = _basepack.GetProducts();
                if (cat != null)
                {
                    return Ok(cat);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting categories: {ex.Message}");
            }
        }


        [HttpGet("GetProductDescription/{id}")]
        public IActionResult GetProductDescription(int id)
        {
            try
            {
                var cat = _basepack.ItemDescription(id);
                if (cat != null)
                {
                    Console.WriteLine(cat.Image_path);
                    return Ok(cat);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting categories: {ex.Message}");
            }
        }

        [HttpGet("GetLocations/{maincat_id}")]
        public IActionResult GetLocation(int maincat_id,[FromQuery] int?Id = null)
        {
            try
            {
                var cat = _basepack.getLocations(Id,maincat_id);
                if (cat != null)
                {
                    return Ok(cat);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting categories: {ex.Message}");
            }
        }
        [HttpGet("GetGuestNumber/{category_id}")]
        public IActionResult GetGuestNumber(int category_id)
        {
            try
            {
                var cat = _basepack.getGuestNumber(category_id);
                if (cat != null)
                {
                    return Ok(cat);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting categories: {ex.Message}");
            }
        }

        [HttpGet("GetOrders/")]
        public IActionResult GetOrders()
        {
            try
            {
                var cat = _basepack.GetOrder();
                if (cat != null)
                {
                    return Ok(cat);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting categories: {ex.Message}");
            }
        }
        [HttpPost("AddToCart")]
        [Authorize]

        public async Task<IActionResult> AddToCart([FromBody] CartModel model)
        {
            try
            {
                var newCompany = await _basepack.AddToBasket(model);

                if (newCompany)
                {
                    return Ok(new { message = "product aded successfully" });
                }
                else
                {
                    return StatusCode(500, "Failed to add product");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating category: {ex.Message}");
            }
        }
        [HttpGet("GetBasket/{id}")]
        [Authorize]

        public IActionResult GetBasket(int id)
        {
            try
            {
                var cat = _basepack.GetBasket(id);
                if (cat != null)
                {
                    return Ok(cat);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting categories: {ex.Message}");
            }
        }
        [HttpDelete("deletefromcart/{userid}/{productid}")]
        [Authorize]

        public async Task<IActionResult> DeleteFromCart(int userid, int productid)
        {
            try
            {
                bool result = await _basepack.DeleteFromBasket(userid, productid);
                if (result)
                {
                    return Ok(new { Success = true, Message = "Product deleted from cart successfully." });
                }
                else
                {
                    return BadRequest(new { Success = false, Message = "Failed to delete product from cart." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error deleting product from cart: {ex.Message}" });
            }
        }
        [HttpPut("updatebasket/")]
        [Authorize]

        public async Task<IActionResult>UpdateBasket(CartModel model)
        {
            try
            {
                bool result = await _basepack.UpdateBasket(model);
                if (result)
                {
                    return Ok(new { Success = true, Message = "Product deleted from cart successfully." });
                }
                else
                {
                    return BadRequest(new { Success = false, Message = "Failed to delete product from cart." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Error deleting product from cart: {ex.Message}" });
            }
        }

    }
}
