using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
namespace Model
{
    public class ServiceContext : DbContext
    {
        public ServiceContext(DbContextOptions options) : base(options) { }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Resposta> Respostas { get; set; }
        public DbSet<Anexo> Anexos { get; set; }

        // Declaração da chave composta
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Anexo>()
                .HasKey(c =>new { c.NomeArquivo,c.RespostaId });
        }

        // Override do metodo de salvar para realizar o to upper em nas propriedades do usuario
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
