using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MqttApiPg.Entities;

namespace MqttApiPg.Controlllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MensalController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public MensalController(MongoDbContext context)
        {
            _context = context;
        }

        // GET: api/Mensal
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mensal>>> GetMensais()
        {
            return await _context.Mensais.Find(_ => true).ToListAsync();
        }

        // GET: api/Mensal/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Mensal>> GetMensal(string id)
        {
            var mensal = await _context.Mensais.Find(x => x.Id == id).SingleOrDefaultAsync();

            if (mensal == null)
            {
                return NotFound();
            }

            return mensal;
        }

        [HttpGet("ano/{ano}")]
        public async Task<ActionResult<IEnumerable<Mensal>>> GetByAno(int ano)
        {
            try
            {
                return await _context.Mensais.Find(x => x.Ano.Equals(ano)).ToListAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
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

            try
            {
                await _context.Mensais.ReplaceOneAsync<Mensal>(x => x.Id == id, mensal);
            }
            catch (Exception ex)
            {
                if (!MensalExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Mensal
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Mensal>> PostMensal(Mensal mensal)
        {
            try
            {
                await _context.Mensais.InsertOneAsync(mensal);
            }
            catch (Exception ex)
            {
                if (MensalExists(mensal.Id ?? string.Empty))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetMensal", new { id = mensal.Id }, mensal);
        }

        // DELETE: api/Mensal/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMensal(string id)
        {
            var mensal = await _context.Mensais.FindAsync(x => x.Id == id);
            if (mensal is null)
            {
                return NotFound();
            }


            await _context.Mensais.DeleteOneAsync(x => x.Id == id);
            return NoContent();
        }

        private bool MensalExists(string id)
        {
            return _context.Mensais.Find(x => x.Id == id).Any();
        }
    }
}
