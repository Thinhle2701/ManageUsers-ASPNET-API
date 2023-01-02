using Microsoft.EntityFrameworkCore;
using UserLoginApp.Models;

namespace UserLoginApp.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) 
        { 
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserID);
                entity.Property(e => e.UserID)
                .HasColumnName("UserID")
                .HasComment("Primary key")
                .ValueGeneratedNever();
            });
        }
    }
}
