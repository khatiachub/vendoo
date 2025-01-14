using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Oracle.ManagedDataAccess.Client;
using System.Security.Cryptography.X509Certificates;
using vendoo.Models;
using vendoo.Packages;
using static System.Net.Mime.MediaTypeNames;

namespace vendoo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly BasePackage _basepack;
        private readonly IConfiguration _configuration;


        public MainController(BasePackage managepackage, IConfiguration configuration)
        {
            _basepack = managepackage;
            _configuration = configuration;
        }

        [HttpPost("CreateUser")]

        public IActionResult CreateUser([FromBody] UserModel model)
        {
            try
            {
                var newcompany = _basepack.RegisterUser(model);
                if (newcompany)
                {
                    return Ok(new { message = "user created successfully" });
                }
                else
                {
                    return StatusCode(500, "Failed to create user.");
                }
            }
            catch (OracleException ex)
            {
                if (ex.Number == 20001)
                {
                    return StatusCode(20001, $"Oracle error occurred: {ex.Message}");
                }
                else
                {
                    return StatusCode(500, $"Oracle error occurred: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating user: {ex.Message}");
            }
        }

        [HttpPost("AddCategories")]

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
        [HttpPost("AddCategories2Child")]
        public async Task<IActionResult> CreateCategories2Child([FromForm] ChildCategoryModel model)
        {
            try
            {
                var imagePaths = await SaveImageFromIFormFileAsync(model.Image);
                Console.WriteLine(imagePaths);

                var newCompany = await _basepack.AddCategories2ndChild(model,imagePaths);

                if (newCompany)
                {
                    return Ok(new { message = "Category created successfully" });
                }
                else
                {
                    return StatusCode(500, "Failed to create category");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating category: {ex.Message}");
            }
        }



        [HttpPost("AddItemDescription")]

        public async Task<IActionResult> CreateItemDescription([FromForm] ItemDescriptionModel model)
        {
            try
            {
                var imagePaths = await SaveImagesFromIFormFileAsync(model.Image_path);

                if (imagePaths == null || !imagePaths.Any())
                {
                    return BadRequest("No images were saved.");
                }
                var newcompany = _basepack.AddItemDescription(model,imagePaths);
                if (await newcompany)
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

        private async Task<string> SaveImageFromIFormFileAsync(IFormFile file)
        {
            string savedPath = string.Empty; 
            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Upload", "Files");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }
            try
            {
                string fileName = $"{Guid.NewGuid()}_{file.FileName}";
                string filePath = Path.Combine(uploadFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                savedPath = Path.Combine("Upload", "Files", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image: {ex.Message}");
            }

            return savedPath;
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
        [HttpGet("GetCategories2st/{id}/{category_name}")]
        public IActionResult GetCategories2st(int id,string category_name)
        {
            try
            {
                var cat = _basepack.GetCategories2ndChild(id,category_name);
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
        public IActionResult GetFilteredItems(
             int id,
             string category_name,
             [FromQuery] string? child_name = null,
             [FromQuery] int? locationId = null,
             [FromQuery] int? guestId = null,
             [FromQuery] int? priceMin = null,
             [FromQuery] int? priceMax = null
         )
        {
            try
            {
                var categories = _basepack.GetFilteredItems(
                    id, category_name, child_name, locationId, guestId, priceMin, priceMax);

                if (categories != null && categories.Any())
                {
                    return Ok(categories);
                }
                else
                {
                    return Ok(new { message = "No categories found matching the criteria." });
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
        [HttpGet("GetGuestNumber/")]
        public IActionResult GetGuestNumber()
        {
            try
            {
                var cat = _basepack.getGuestNumber();
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

    }
}
