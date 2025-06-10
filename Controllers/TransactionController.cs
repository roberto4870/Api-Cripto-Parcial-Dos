using ApiCriptoParcialI.DTOs;
using ApiCriptoParcialI.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCriptoParcialI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
        public class TransactionController : ControllerBase
        {
            private readonly AppDbContext _context;
            private readonly HttpClient _httpClient;

            public TransactionController(AppDbContext context, IHttpClientFactory httpClientFactory)
            {
                _context = context;
                _httpClient = httpClientFactory.CreateClient();
            }

            // POST /transactions
            [HttpPost]
            public async Task<ActionResult<Transaction>> Post(Transaction transaction)
            {
                // Validaciones
                if (transaction.CryptoAmount <= 0)
                    return BadRequest("El monto de la criptomoneda debe ser mayor a 0.");

                var client = await _context.Clientes.FindAsync(transaction.ClienteId);
                if (client == null)
                    return BadRequest("Cliente no encontrado.");

                // Llamar a la API externa
                string url = $"https://criptoya.com/api/satoshitango/{transaction.CryptoCode}/ars";
                try
                {
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    // Con la propiedad 'totalAsk' traigo el precio en ARS
                    decimal precioARS = root.GetProperty("totalAsk").GetDecimal();

                    // Calculo el total en ARS
                    decimal totalARS = transaction.CryptoAmount * precioARS;
                    transaction.Money = totalARS;

                    // Guardo en la BD
                    _context.Transactions.Add(transaction);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction(nameof(Get), new { id = transaction.Id }, transaction);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error al consultar el precio en CriptoYa: {ex.Message}");
                }
            }

            // GET /transactions
            [HttpGet]
            public async Task<ActionResult<IEnumerable<TransactionDTOs>>> Get()
            {
                var transactions = await _context.Transactions
                .OrderByDescending(t => t.DateTime)
                    .ToListAsync();

                var dto = transactions.Select(t => new TransactionDTOs
                {
                    Id = t.Id,
                    CryptoCode = t.CryptoCode,
                    CryptoAmount = t.CryptoAmount,
                    ClientId = t.ClienteId,
                    Money = t.Money,
                    Action = t.Action,
                    Datetime = t.DateTime
                }).ToList();

                return Ok(dto);
            }

            // GET /transactions/{id}
            [HttpGet("{id}")]
            public async Task<ActionResult<TransactionDTOs>> Get(int id)
            {
                var t = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);

                if (t == null)
                    return NotFound();

                var dto = new TransactionDTOs
                {
                    Id = t.Id,
                    CryptoCode = t.CryptoCode,
                    CryptoAmount = t.CryptoAmount,
                    ClientId = t.ClienteId,
                    Money = t.Money,
                    Action = t.Action,
                    Datetime = t.DateTime
                };

                return Ok(dto);
            }

            // PATCH /transactions/{id}
            [HttpPatch("{id}")]
            public async Task<IActionResult> Patch(int id, [FromBody] JsonElement body)
            {
                var t = await _context.Transactions.FindAsync(id);
                if (t == null)
                    return NotFound();

                if (body.TryGetProperty("money", out var moneyProp))
                {
                    t.Money = moneyProp.GetDecimal();
                    await _context.SaveChangesAsync();
                }

                return NoContent();
            }

            // DELETE /transactions/{id}
            [HttpDelete("{id}")]
            public async Task<IActionResult> Delete(int id)
            {
                var t = await _context.Transactions.FindAsync(id);
                if (t == null)
                    return NotFound();

                _context.Transactions.Remove(t);
                await _context.SaveChangesAsync();

                return NoContent();
            }
        }
}

