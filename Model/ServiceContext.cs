using Microsoft.EntityFrameworkCore;

namespace Model
{
    public class ServiceContext : DbContext
    {
        public ServiceContext(DbContextOptions options) : base(options) {}
       public DbSet<Ticket> Tickets { get; set; }
       public DbSet<Usuario> Usuarios { get; set; }
       public DbSet<Resposta> Respostas { get; set; }


        public override int SaveChanges()
        {
            foreach(var Tipo in ChangeTracker.Entries<Usuario>())
            {
                if (Tipo.State == EntityState.Modified || Tipo.State == EntityState.Added)
                {
                    Tipo.Entity.Nome = Tipo.Entity.Nome.ToUpper();
                    Tipo.Entity.Tipo = Tipo.Entity.Tipo.ToUpper();
                }
            }
            return base.SaveChanges();
        }
    }
}
