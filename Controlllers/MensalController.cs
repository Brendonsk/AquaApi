using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MqttApiPg.Entities;
using MqttApiPg.Services;
using System.Diagnostics;

namespace MqttApiPg.Controlllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MensalController : ControllerBase
    {
        private readonly MensalService _mensalService;

        public MensalController(MensalService mensalService)
        {
            _mensalService = mensalService;
        }

        // GET: api/Mensal
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mensal>>> GetMensais()
        {
            return await _mensalService.GetAsync();
        }

        // GET: api/Mensal/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Mensal>> GetMensal(string id)
        {
            var result = await _mensalService.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }

        [HttpGet("ano/{ano}")]
        public async Task<ActionResult<IEnumerable<Mensal>>> GetByAno(int ano)
        {
            return await _mensalService.GetByAno(ano);
        }

        //// PUT: api/Mensal/5
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMensal(string id, Mensal mensal)
        {
            if (id == null || id == string.Empty)
            {
                return BadRequest();
            }
            mensal.Id = id;

            var result = await _mensalService.UpdateAsync(id, mensal);

            if (result is not null)
            {
                return new OkObjectResult(result);
            }

            return NotFound();
        }

        // POST: api/Mensal
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Mensal>> PostMensal(Mensal mensal)
        {
            await _mensalService.CreateAsync(mensal);

            return CreatedAtAction(nameof(GetMensal), new { id = mensal.Id }, mensal);
        }

        // DELETE: api/Mensal/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMensal(string id)
        {
            var result = await _mensalService.DeleteAsync(id);
            if (result is not null)
            {
                return new OkObjectResult(result);
            }

            return NotFound();
        }
    }
}
