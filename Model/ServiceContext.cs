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

            foreach (var Tipo2 in ChangeTracker.Entries<Ticket>())
                if (Tipo2.State == EntityState.Modified || Tipo2.State == EntityState.Added)
                {
                    Tipo2.Entity.Titulo = Tipo2.Entity.Titulo.ToUpper();
                    Tipo2.Entity.Mensagem = Tipo2.Entity.Mensagem.ToUpper();
                }

            foreach (var Tipo3 in ChangeTracker.Entries<Resposta>())
                if (Tipo3.State == EntityState.Modified || Tipo3.State == EntityState.Added)
                    Tipo3.Entity.Mensagem = Tipo3.Entity.Mensagem.ToUpper();

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
