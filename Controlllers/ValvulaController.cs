using Microsoft.AspNetCore.Mvc;
using MqttApiPg.Services;

namespace MqttApiPg.Controlllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValvulaController : ControllerBase
    {
        private ValvulaService _valvulaService;

        public ValvulaController(ValvulaService valvulaService)
        {
            _valvulaService = valvulaService;
        }

        [HttpGet("abrir")]
        public async Task<IActionResult> AbreValvulaEsp()
        {
            try
            {
                await _valvulaService.AbreValvula();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("fechar")]
        public async Task<IActionResult> FechaValvulaEsp()
        {
            try
            {
                await _valvulaService.FechaValvula();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
