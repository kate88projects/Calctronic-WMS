using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data.GRN;
using RackingSystem.Data.JO;
using RackingSystem.Data.Log;
using RackingSystem.Data.Maintenances;
using RackingSystem.Data.RackJobQueue;
using RackingSystem.Models.BOM;
using RackingSystem.Models.GRN;
using RackingSystem.Models.Item;
using RackingSystem.Models.RackJobQueue;
using RackingSystem.Models.Slot;

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

            modelBuilder.Entity<User>()
                .Property(e => e.IsActive);

            modelBuilder.Entity<GRNDtlListDTO>(entity =>
                entity.HasKey(e => e.GRNDetail_Id));

            modelBuilder.Entity<ItemListDTO>(entity =>
                entity.HasKey(e => e.Item_Id));

            modelBuilder.Entity<SlotFreeDTO>(entity =>
                entity.HasNoKey());

            modelBuilder.Entity<Slot_DrawerFreeDTO>(entity =>
                entity.HasNoKey());
        }

        public DbSet<Configuration> Configuration { get; set; }
        public DbSet<DocFormat> DocFormat { get; set; }
        public DbSet<DocFormatDetail> DocFormatDetail { get; set; }

        public DbSet<UserAccessRight> UserAccessRight { get; set; }
        public DbSet<ReelDimension> ReelDimension { get; set; }
        public DbSet<SlotCalculation> SlotCalculation { get; set; }
        public DbSet<SlotColumnSetting> SlotColumnSetting { get; set; }

        public DbSet<Slot> Slot { get; set; }
        public DbSet<Reel> Reel { get; set; }
        public DbSet<ItemGroup> ItemGroup { get; set; }
        public DbSet<Item> Item { get; set; }

        public DbSet<Loader> Loader { get; set; }
        public DbSet<LoaderColumn> LoaderColumn { get; set; }
        public DbSet<LoaderReel> LoaderReel { get; set; }

        public DbSet<Trolley> Trolley { get; set; }
        public DbSet<TrolleySlot> TrolleySlot { get; set; }
        public DbSet<BOM> BOM { get; set; }
        public DbSet<BOMDetail> BOMDetail { get; set; }
        public DbSet<GRNDetail> GRNDetail { get; set; }

        public DbSet<JobOrder> JobOrder { get; set; }
        public DbSet<JobOrderDetail> JobOrderDetail { get; set; }
        public DbSet<JobOrderRaws> JobOrderRaws { get; set; }

        public DbSet<RackJobQueue.RackJobQueue> RackJobQueue { get; set; }
        public DbSet<RackJobQueueLog> RackJobQueueLog { get; set; }

        // ------------        Log       ------------
        public DbSet<PLCLoaderLog> PLCLoaderLog { get; set; }

        // ------------ Stored Procedure ------------
        public DbSet<ItemListDTO> SP_ItemSearchList { get; set; }
        public DbSet<GRNDtlListDTO> SP_GRNDTLSearchList { get; set; }

        public DbSet<SlotFreeDTO> SP_SlotGetFreeSlotByCol_ASC { get; set; }
        public DbSet<SlotFreeDTO> SP_SlotGetFreeSlotByCol_DESC { get; set; }
        public DbSet<BOMListDTO> SP_BOMSearchList { get; set; }
        public DbSet<Slot_DrawerFreeDTO> SP_SlotGetFreeSDrawerByCol { get; set; }

        public DbSet<QListDTO> SP_QueueList { get; set; }

    }
}
