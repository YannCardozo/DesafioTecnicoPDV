using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class TesteController : ControllerBase
    {

        [HttpGet("teste")]
        public IActionResult Index()
        {
            return Ok("TESTE");
        }
    }
}
