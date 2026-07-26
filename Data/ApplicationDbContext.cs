using KejaHUnt_PropertiesAPI.Models.Domain;
using KejaHUnt_PropertiesAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KejaHUnt_PropertiesAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Property> Properties { get; set; }
        public DbSet<PendingProperty> PendingProperties { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<GeneralFeatures> GeneralFeatures { get; set; }
        public DbSet<IndoorFeatures> IndoorFeatures { get; set; }
        public DbSet<OutDoorFeatures> OutDoorFeatures { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<PolicyDescription> PolicyDescriptions { get; set; }
        public DbSet<PendingPolicyDescription> PendingPolicyDescriptions { get; set; }
        public DbSet<UnitPayments> UnitPayments { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Existing configurations
            modelBuilder.Entity<UnitPayments>()
                .HasMany(x => x.Transactions)
                .WithOne(t => t.UnitPayment)
                .HasForeignKey(t => t.UnitPaymentId);

            modelBuilder.Entity<UnitPayments>()
                .Property(u => u.Status)
                .HasConversion(new EnumToStringConverter<UnitPaymentStatus>());

            modelBuilder.Entity<PaymentTransaction>()
                .Property(p => p.Status)
                .HasConversion(new EnumToStringConverter<PaymentTransactionStatus>());

            // NEW: Convert Unit.Status enum to string
            modelBuilder.Entity<Unit>(entity =>
            {
                entity.Property(e => e.Status)
                    .HasConversion<string>();
            });
        }
    }
}