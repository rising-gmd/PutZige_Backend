// PutZige.API/Controllers/HealthController.cs
#nullable enable
using Microsoft.AspNetCore.Mvc;

namespace PutZige.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { status = "healthy" });
    }
}
