using DbLayer.Tables.Public;
using Microsoft.EntityFrameworkCore;

namespace DbLayer.Context
{
    public class DatabaseContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Application> Applications { get; set; }

        public DatabaseContext()
        {

        }
        public DatabaseContext(DbContextOptions options)
            : base(options)
        {
            Database.SetCommandTimeout(60 * 15);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User", "public");

                entity.HasKey(e => new { e.Id });

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .IsRequired();

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .IsRequired();
            });
            modelBuilder.Entity<Application>(entity =>
            {
                entity.ToTable("Application", "public");

                entity.HasKey(e => new { e.Id });

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .IsRequired();

                entity.Property(e => e.CreationDate)
                    .HasColumnName("creationdate")
                    .HasColumnType("timestamp")
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnName("Description");

                entity.Property(e => e.UserId)
                    .HasColumnName("userid");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Applications)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("Application_userid_fkey");

                entity.Property(e => e.Views)
                    .HasColumnName("Views")
                    .IsRequired();
            });
        }
    }
}