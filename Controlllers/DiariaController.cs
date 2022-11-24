using Microsoft.AspNetCore.Mvc;
using MqttApiPg.Entities;
using MqttApiPg.Services;

namespace MqttApiPg.Controlllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiariaController : ControllerBase
    {
        private readonly DiariaService _diariaService;

        public DiariaController(DiariaService diariaService)
        {
            _diariaService = diariaService;
        }

        // GET: api/Diaria
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Diaria>>> GetDiariasAsync()
        {
            return await _diariaService.GetAsync();
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
        //    //FilterDefinition<Diaria> filter = Builders<Diaria>.Filter.

        //    try
        //    {
        //        var coll = _context.Diarias.AsQueryable()
        //            .GroupBy(x => x.DiaHora.Truncate(DateTimeUnit.Month))
        //            .Select(x => new { Bucket = x.Key, FirstDocumentInBucket = x.First() });

        //        var coll2 = _context.Diarias.AsQueryable()
        //            .Where<Diaria>(x => (x.DiaHora.Truncate(DateTimeUnit.Month)).Month.Equals(mes))
        //            .FirstOrDefaultAsync();

        //        return await coll2;
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex);
        //    }
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
