using Microsoft.EntityFrameworkCore;

namespace ApiCriptoParcialI.Models
{
    
        public class AppDbContext : DbContext
        {
            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

            public DbSet<Cliente> Clientes { get; set; }
            public DbSet<Transaction> Transactions { get; set; }
        }
    
}
