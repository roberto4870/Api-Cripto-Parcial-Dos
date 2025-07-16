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
            if (transaction.CryptoAmount <= 0)
                return BadRequest("El monto de la criptomoneda debe ser mayor a 0.");

            var client = await _context.Clientes.FindAsync(transaction.ClienteId);
            if (client == null)
                return BadRequest("Cliente no encontrado.");

            // Validar que no se vendan más criptos de las que posee el cliente
            if (transaction.Action.ToLower() == "venta")
            {
                var totalComprado = await _context.Transactions
                    .Where(t => t.ClienteId == transaction.ClienteId && t.CryptoCode == transaction.CryptoCode && t.Action.ToLower() == "compra")
                    .SumAsync(t => t.CryptoAmount);

                var totalVendido = await _context.Transactions
                    .Where(t => t.ClienteId == transaction.ClienteId && t.CryptoCode == transaction.CryptoCode && t.Action.ToLower() == "venta")
                    .SumAsync(t => t.CryptoAmount);

                var saldoDisponible = totalComprado - totalVendido;

                if (transaction.CryptoAmount > saldoDisponible)
                    return BadRequest("El cliente no tiene saldo suficiente de esta criptomoneda para realizar la venta.");

                // Validación de fecha: la venta no puede ser anterior a la compra
                var ultimaCompra = await _context.Transactions
                    .Where(t => t.ClienteId == transaction.ClienteId && t.CryptoCode == transaction.CryptoCode && t.Action.ToLower() == "compra")
                    .OrderByDescending(t => t.DateTime)
                    .FirstOrDefaultAsync();

                if (ultimaCompra != null && transaction.DateTime < ultimaCompra.DateTime)
                    return BadRequest("La fecha de la venta no puede ser anterior a la compra.");
            }

            // Llamar a la API externa
            string url = $"https://criptoya.com/api/satoshitango/{transaction.CryptoCode}/ars";
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                decimal precioARS = root.GetProperty("totalAsk").GetDecimal();

                decimal totalARS = transaction.CryptoAmount * precioARS;
                transaction.Money = totalARS;

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
        public async Task<ActionResult<IEnumerable<TransactionDetalleDTO>>> Get()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Cliente)
                .OrderByDescending(t => t.DateTime)
                .ToListAsync();

            var dto = transactions.Select(t => new TransactionDetalleDTO
            {
                Id = t.Id,
                CryptoCode = t.CryptoCode,
                CryptoAmount = t.CryptoAmount,
                ClienteId = t.ClienteId,
                ClientName = t.Cliente?.Name ?? "N/A",
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

        // GET /transactions/saldo/{clienteId}
        [HttpGet("saldo/{clienteId}")]
        public async Task<ActionResult> GetSaldo(int clienteId)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
                return NotFound("Cliente no encontrado");

            var movimientos = await _context.Transactions
                .Where(t => t.ClienteId == clienteId)
                .ToListAsync();

            var saldoPorCripto = movimientos
                .GroupBy(t => t.CryptoCode.ToLower())
                .Select(g => new SaldoCriptoDTO
                {
                    CryptoCode = g.Key,
                    Total = g.Where(t => t.Action == "compra").Sum(t => t.CryptoAmount)
                           - g.Where(t => t.Action == "venta").Sum(t => t.CryptoAmount)
                })
                .ToList();

            return Ok(saldoPorCripto);
        }

        // PATCH /transactions/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, [FromBody] JsonElement body)
        {
            var t = await _context.Transactions.FindAsync(id);
            if (t == null)
                return NotFound();

            if (body.TryGetProperty("money", out var moneyProp))
                t.Money = moneyProp.GetDecimal();

            if (body.TryGetProperty("cryptoAmount", out var cryptoAmountProp))
            {
                var newAmount = cryptoAmountProp.GetDecimal();
                if (newAmount <= 0)
                    return BadRequest("El monto debe ser mayor a 0.");
                t.CryptoAmount = newAmount;
            }

            if (body.TryGetProperty("cryptoCode", out var cryptoCodeProp))
                t.CryptoCode = cryptoCodeProp.GetString();

            if (body.TryGetProperty("action", out var actionProp))
            {
                var action = actionProp.GetString()?.ToLower();
                if (action != "compra" && action != "sale")
                    return BadRequest("Debe seleccionar una operación: 'compra' o 'venta'.");
                t.Action = action;
            }

            if (body.TryGetProperty("datetime", out var dateTimeProp))
            {
                if (DateTime.TryParse(dateTimeProp.GetString(), out var parsedDate))
                    t.DateTime = parsedDate;
            }

            if (body.TryGetProperty("clienteId", out var clienteIdProp))
            {
                var newClientId = clienteIdProp.GetInt32();
                var exists = await _context.Clientes.AnyAsync(c => c.Id == newClientId);
                if (!exists)
                    return BadRequest("Cliente no encontrado.");
                t.ClienteId = newClientId;
            }

            await _context.SaveChangesAsync();

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

