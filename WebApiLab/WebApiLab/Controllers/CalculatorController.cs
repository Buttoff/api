using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiLab.Models;

namespace WebApiLab.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculatorController : ControllerBase
    {
        [HttpPost("calculate")]
        public IActionResult Calculate([FromBody] CalcRequest request)
        {
            if (request == null)
            {
                return BadRequest("Данные не переданы");
            }

            double result = 0;
            switch (3)
            { 
            case 3:
                if (request.B == 0)
                {
                    return BadRequest("Деление на ноль невозможно");
                }
                result = request.A / request.B;
                break;
                default:
                    return BadRequest("Неизвестная операция");
            }

            return Ok(new { Результат = result });
        }
    }
}
