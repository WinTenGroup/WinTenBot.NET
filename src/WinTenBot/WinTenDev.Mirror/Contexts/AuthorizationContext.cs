using Microsoft.EntityFrameworkCore;

namespace WinTenDev.Mirror.Contexts
{
    public class AuthorizationContext : DbContext
    {
        public AuthorizationContext(DbContextOptions<AuthorizationContext> options) : base(options)
        {
        }

        public DbSet<AuthorizedChat> AuthorizedChats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthorizedChat>()
                .ToTable("AuthorizedChats")
                .HasKey(x => x.ChatId);
        }
    }
}