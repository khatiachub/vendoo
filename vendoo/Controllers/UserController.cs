using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Oracle.ManagedDataAccess.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using vendoo.Models;
using vendoo.Packages;

namespace vendoo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("MyPolicy")]
    public class UserController : ControllerBase
    {
        private readonly BasePackage _basepack;
        private readonly IConfiguration _configuration;


        public UserController(BasePackage managepackage, IConfiguration configuration)
        {
            _basepack = managepackage;
            _configuration = configuration;
        }

        protected string GenerateNewJsonWebToken(List<Claim> claims)
        {
            var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var tokenObject = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.MaxValue,
                claims: claims,
                signingCredentials: new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256)
            );
            string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);
            return token;
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

        [HttpPost("LoginUser")]

        public IActionResult LoginUser([FromBody] LoginModel model)
        {
            try
            {
                var user = _basepack.LoginUser(model);
                if (user != null)
                {
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("JWTID", Guid.NewGuid().ToString()),
                    };
                    var token = GenerateNewJsonWebToken(authClaims);
                    return Ok(new { message = "login was successfull", token, user.Id,user.Role });
                }
                else
                {
                    return StatusCode(500, "Failed to loginuser.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating user: {ex.Message}");
            }
        }

        [HttpGet("GetUser/{id}")]
        //[Authorize]

        public IActionResult GetUser(int id)
        {
            try
            {
                var user = _basepack.GetUser(id);
                if (user!=null)
                {
                    
                    return Ok(new { message = user });
                }
                else
                {
                    return StatusCode(500, "Failed to get user.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating user: {ex.Message}");
            }
        }
        [HttpPut("UpdateUser/{id}")]
        [Authorize(Roles = "user")]

        public IActionResult UpdateUser([FromBody] UserModel model,int id)
        {
            try
            {
                var user = _basepack.UpdateUser(model,id);
                if (user)
                {
                     
                    return Ok(new { message = "update was successfull"});
                }
                else
                {
                    return StatusCode(500, "Failed to loginuser.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating user: {ex.Message}");
            }
        }
        [HttpDelete("DeleteUser/{id}")]
        [Authorize(Roles = "user")]

        public IActionResult DeleteUser(int id)
        {
            try
            {
                var user = _basepack.DeleteUser(id);
                if (user)
                {
                   
                    return Ok(new { message = "delete was successfull"});
                }
                else
                {
                    return StatusCode(500, "Failed to loginuser.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating user: {ex.Message}");
            }
        }
    }
}
