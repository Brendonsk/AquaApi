using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MqttApiPg.Entities;
using MqttApiPg.Services;

namespace MqttApiPg.Controlllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistroController : ControllerBase
    {
        private readonly RegistroService _registroService;
        private readonly ILogger<RegistroController> _logger;

        public RegistroController(ILogger<RegistroController> logger, RegistroService registroService)
        {
            _logger = logger;
            _registroService = registroService;
        }

        // GET: api/Registro
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Registro>>> GetRegistros()
        {
            return await _registroService.GetAsync();
        }

        // GET: api/Registro/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Registro>> GetRegistro(string id)
        {
            var result = await _registroService.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }

        [HttpGet("emAberto")]
        public async Task<ActionResult<IEnumerable<Registro>>> GetAllFalseRegistros()
        {
            return await _registroService.GetAllFalseRegistros();
        }

        //// PUT: api/Registro/5
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRegistro(string id, Registro registro)
        {
            if (id == null || id == string.Empty)
            {
                return BadRequest();
            }

            registro.Id = id;

            var result = await _registroService.UpdateAsync(id, registro);

            if (result is not null)
            {
                return new OkObjectResult(result);
            }

            return NotFound();
        }

        // POST: api/Registro
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Registro>> PostRegistro(Registro registro)
        {
            await _registroService.CreateAsync(registro);

            return CreatedAtAction(nameof(GetRegistro), new { id = registro.Id }, registro);
        }

        // DELETE: api/Registro/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegistro(string id)
        {
            var result = await _registroService.DeleteAsync(id);
            if (result is not null)
            {
                return new OkObjectResult(result);
            }

            return NotFound();
        }
    }
}
