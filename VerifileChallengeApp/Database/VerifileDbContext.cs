using Microsoft.EntityFrameworkCore;
using VerifileChallengeApp.Database.Models;

namespace VerifileChallengeApp.Database
{
    public class VerifileDbContext : DbContext
    {
        public DbSet<Person> People { get; set; }

        public VerifileDbContext(DbContextOptions<VerifileDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Person>(entity =>
            {
                entity.Property(e => e.Id).IsRequired().HasColumnName("id");
                entity.Property(e => e.LastUpdate).IsRequired().HasColumnName("last_update");
                entity.Property(e => e.GivenName).IsRequired().HasMaxLength(255).HasColumnName("given_name");
                entity.Property(e => e.FamilyName).IsRequired().HasMaxLength(255).HasColumnName("family_name");

                entity.HasKey(e => new { e.Id, e.LastUpdate}).HasName("id_last_update");
            });
        }
    }
}
