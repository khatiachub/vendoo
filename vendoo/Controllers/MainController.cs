using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using vendoo.Models;
using vendoo.Packages;

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

        public IActionResult CreateCategories2Child([FromBody] CategoryModel model)
        {
            try
            {
                var newcompany = _basepack.AddCategories2ndChild(model);
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
    }
}
