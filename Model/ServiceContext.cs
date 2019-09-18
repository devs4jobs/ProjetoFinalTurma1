using Microsoft.EntityFrameworkCore;
namespace Model
{
    public class ServiceContext : DbContext
    {
        public ServiceContext(DbContextOptions options) : base(options) {}

       public DbSet<Ticket> Tickets { get; set; }
       public DbSet<Usuario> Usuarios { get; set; }
       public DbSet<Resposta> Respostas { get; set; }
    }
}
