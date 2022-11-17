using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MqttApiPg.Entities;

namespace MqttApiPg.Controlllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiariaController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public DiariaController(MongoDbContext context)
        {
            _context = context;
        }

        // GET: api/Diaria
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Diaria>>> GetDiarias()
        {
            return await _context.Diarias.Find(_ => true).ToListAsync();
        }

        // GET: api/Diaria/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Diaria>> GetDiaria(string id)
        {
            var diaria = await _context.Diarias.Find(x => x.Id == id).SingleOrDefaultAsync();

            if (diaria == null)
            {
                return NotFound();
            }

            return diaria;
        }

        //// PUT: api/Diaria/5
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDiaria(string id, Diaria diaria)
        {
            if (id == null || id == string.Empty)
            {
                return BadRequest();
            }

            diaria.Id = id;

            try
            {
                await _context.Diarias.ReplaceOneAsync<Diaria>(x => x.Id == id, diaria);
            }
            catch (Exception ex)
            {
                if (!DiariaExists(id))
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

        // POST: api/Diaria
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Diaria>> PostDiaria(Diaria diaria)
        {
            try
            {
                await _context.Diarias.InsertOneAsync(diaria);
            }
            catch (Exception ex)
            {
                if (DiariaExists(diaria.Id ?? string.Empty))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetDiaria", new { id = diaria.Id }, diaria);
        }

        // DELETE: api/Diaria/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiaria(string id)
        {
            var diaria = await _context.Diarias.FindAsync(x => x.Id == id);
            if (diaria is null)
            {
                return NotFound();
            }

            await _context.Diarias.DeleteOneAsync(x => x.Id == id);
            return NoContent();
        }

        private bool DiariaExists(string id)
        {
            return _context.Diarias.Find(x => x.Id == id).Any();
        }
    }
}
