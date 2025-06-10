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

        [HttpPost]
            public async Task<ActionResult<Cliente>> Post(Cliente client)
            {
                _context.Clientes.Add(client);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(Post), new { id = client.Id }, client);
            }
        
    }
}

