using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Hepldesk.Models;

namespace Hepldesk.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser,  IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tiket> Tikets { get; set; }
        public DbSet<TiketStatusHistory> TiketStatusHistories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TiketStatusHistory>()
                .HasOne(h => h.Tiket)
                .WithMany()
                .HasForeignKey(h => h.TiketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tiket>()
                .HasIndex(t => t.NomorTiketPublik)
                .IsUnique()
                .HasFilter("[NomorTiketPublik] IS NOT NULL");
        }
    }
}