using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MqttApiPg.Entities;
using MqttApiPg.Services;

namespace MqttApiPg.Controlllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiariaController : ControllerBase
    {
        private readonly DiariaService _diariaService;
        private readonly ILogger<DiariaController> _logger;

        public DiariaController(DiariaService diariaService, ILogger<DiariaController> logger)
        {
            _diariaService = diariaService;
            _logger = logger;
        }

        // GET: api/Diaria
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Diaria>>> GetDiariasAsync()
        {
            return await _diariaService.GetAsync();
        }

        [HttpGet("porMesDeAno/{ano}/{mes}")]
        public async Task<ActionResult<IEnumerable<Diaria>>> GetDiariasPorMesDeAnoAsync(int ano, int mes)
        {
            var res = await _diariaService.GetDiariasByMonthOfYear(ano, mes);
            _logger.LogInformation(res.ToString());
            return res;
        }

        // GET: api/Diaria/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Diaria>> GetDiaria(string id)
        {
            var result = await _diariaService.GetByIdAsync(id);
            if (result is not null)
            {
                return result;
            }

            return NotFound();
        }

        //[HttpGet("ultimaDoMes/{mes}")]
        //public async Task<ActionResult<Diaria?>> GetUltimaLeituraDiariaDoMes(int mes)
        //{
        //    throw new NotImplementedException();
        //}

        //// PUT: api/Diaria/5
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDiaria(string id, Diaria diaria)
        {
            if (id == null || id == string.Empty)
            {
                return BadRequest();
            }

            var result = await _diariaService.UpdateAsync(id, diaria);

            if (result is not null)
            {
                return new OkObjectResult(result);
            }

            return NotFound();
        }

        // POST: api/Diaria
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Diaria>> PostDiaria(Diaria diaria)
        {
            await _diariaService.CreateAsync(diaria);

            return CreatedAtAction(nameof(GetDiaria), new { id = diaria.Id }, diaria);
        }

        // DELETE: api/Diaria/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiaria(string id)
        {
            var result = await _diariaService.DeleteAsync(id);
            if (result is not null)
            {
                return new OkObjectResult(result);
            }

            return NotFound();
        }
    }
}
