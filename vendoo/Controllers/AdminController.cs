using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using vendoo.Packages;

namespace vendoo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly BasePackage _basepack;
        private readonly IConfiguration _configuration;


        public AdminController(BasePackage managepackage, IConfiguration configuration)
        {
            _basepack = managepackage;
            _configuration = configuration;
        }

    }
}
