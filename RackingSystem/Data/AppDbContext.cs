using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data.GRN;
using RackingSystem.Data.Maintenances;
using RackingSystem.Models.GRN;

namespace RackingSystem.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(e => e.FullName)
                .HasMaxLength(500);

            modelBuilder.Entity<GRNDtlListDTO>(entity =>
                entity.HasKey(e => e.GRNDetail_Id));
        }

        public DbSet<Configuration> Configuration { get; set; }
        public DbSet<DocFormat> DocFormat { get; set; }
        public DbSet<DocFormatDetail> DocFormatDetail { get; set; }

        public DbSet<Slot> Slot { get; set; }
        public DbSet<Reel> Reel { get; set; }
        public DbSet<Item> Item { get; set; }
        public DbSet<Loader> Loader { get; set; }
        public DbSet<Trolley> Trolley { get; set; }
        public DbSet<TrolleySlot> TrolleySlot { get; set; }

        public DbSet<GRNDetail> GRNDetail { get; set; }

        public DbSet<GRNDtlListDTO> SP_GRNDTLSearchList { get; set; }
    }
}
