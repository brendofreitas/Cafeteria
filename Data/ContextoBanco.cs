using Cafeteria.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Cafeteria.Data
{
    public class ContextoBanco : DbContext
    {
        public ContextoBanco(DbContextOptions<ContextoBanco> options) : base(options)
        {
        }
        
        public DbSet<Cafe> Cafes { get; set; }
    }
}
