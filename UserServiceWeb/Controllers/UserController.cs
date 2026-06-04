using Microsoft.AspNetCore.Mvc;

namespace UserServiceWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Message = "Hello from UserController!" });
        }
    }
}
