using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MqttApiPg.Entities;

namespace MqttApiPg.Controlllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistroController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public RegistroController(MongoDbContext context)
        {
            _context = context;
        }

        // GET: api/Registro
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Registro>>> GetRegistros()
        {
            return await _context.Registros.Find(_ => true).ToListAsync();
        }

        // GET: api/Registro/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Registro>> GetRegistro(string id)
        {
            var registro = await _context.Registros.Find(x => x.Id == id).SingleOrDefaultAsync();

            if (registro == null)
            {
                return NotFound();
            }

            return registro;
        }

        [HttpGet("emAberto")]
        public async Task<ActionResult<IEnumerable<Registro>>> GetAllFalseRegistros()
        {
            try
            {
                return await _context.Registros.Find(x => !x.DataSolucao.HasValue).ToListAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
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

            try
            {
                await _context.Registros.ReplaceOneAsync<Registro>(x => x.Id == id, registro);
            }
            catch (Exception ex)
            {
                if (!RegistroExists(id))
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

        // POST: api/Registro
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Registro>> PostRegistro(Registro registro)
        {
            try
            {
                await _context.Registros.InsertOneAsync(registro);
            }
            catch (Exception ex)
            {
                if (RegistroExists(registro.Id ?? string.Empty))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetRegistro", new { id = registro.Id }, registro);
        }

        // DELETE: api/Registro/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegistro(string id)
        {
            var registro = await _context.Registros.FindAsync(x => x.Id == id);
            if (registro is null)
            {
                return NotFound();
            }

            await _context.Registros.DeleteOneAsync(x => x.Id == id);
            return NoContent();
        }

        private bool RegistroExists(string id)
        {
            return _context.Registros.Find(x => x.Id == id).Any();
        }
    }
}
