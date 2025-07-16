using ApiCriptoParcialI.Models;
using Microsoft.AspNetCore.Mvc;
using ApiCriptoParcialI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ApiCriptoParcialI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
               
         private readonly AppDbContext _context;

        public ClienteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/cliente
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDTOs>>> Get()
        {
            var clients = await _context.Clientes.ToListAsync();
            var dto = clients.Select(c => new ClienteDTOs
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email
            }).ToList();

            return Ok(dto);
        }

        // GET: api/cliente/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDTOs>> Get(int id)
        {
            var c = await _context.Clientes.FindAsync(id);
            if (c == null)
                return NotFound();

            var dto = new ClienteDTOs
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email
            };

            return Ok(dto);
        }

        // POST: api/cliente
        [HttpPost]
        public async Task<ActionResult<Cliente>> Post(Cliente client)
        {
            _context.Clientes.Add(client);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = client.Id }, client);
        }

        // PUT: api/cliente/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ClienteDTOs updated)
        {
            if (id != updated.Id)
                return BadRequest("El ID no coincide.");

            var existing = await _context.Clientes.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Name = updated.Name;
            existing.Email = updated.Email;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/cliente/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clientes.FindAsync(id);
            if (client == null)
                return NotFound();

            var tieneMovimientos = await _context.Transactions.AnyAsync(t => t.ClienteId == id);
            if (tieneMovimientos)
                return BadRequest("No se puede eliminar el cliente porque tiene transacciones asociadas.");

            _context.Clientes.Remove(client);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

