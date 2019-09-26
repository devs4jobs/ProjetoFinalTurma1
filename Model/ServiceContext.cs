using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
namespace Model
{
    public class ServiceContext : DbContext
    {
        public ServiceContext(DbContextOptions options) : base(options) {}

       public DbSet<Ticket> Tickets { get; set; }
       public DbSet<Usuario> Usuarios { get; set; }
       public DbSet<Resposta> Respostas { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {

            foreach (var Tipo in ChangeTracker.Entries<Usuario>())
                if (Tipo.State == EntityState.Modified || Tipo.State == EntityState.Added)
                {
                    Tipo.Entity.Nome = Tipo.Entity.Nome.ToUpper();
                    Tipo.Entity.Tipo = Tipo.Entity.Tipo.ToUpper();
                    Tipo.Entity.Email = Tipo.Entity.Email.ToUpper();
                }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
