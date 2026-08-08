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
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<WaterRate> WaterRates { get; set; }
        public DbSet<WaterMeterReading> WaterMeterReadings { get; set; }
        public DbSet<WaterBill> WaterBills { get; set; }
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

            // Invoice configuration
            modelBuilder.Entity<Invoice>()
                .Property(i => i.Status)
                .HasConversion(new EnumToStringConverter<UnitPaymentStatus>());

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Property)
                .WithMany()
                .HasForeignKey(i => i.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Unit)
                .WithMany()
                .HasForeignKey(i => i.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.UnitPayments)
                .WithMany()
                .HasForeignKey(i => i.UnitPaymentsId)
                .OnDelete(DeleteBehavior.Restrict);

            // Water billing configuration
            modelBuilder.Entity<WaterRate>()
                .HasIndex(r => new { r.PropertyId, r.IsActive });

            modelBuilder.Entity<WaterMeterReading>()
                .HasIndex(r => new { r.UnitId, r.BillingYear, r.BillingMonth })
                .IsUnique();

            modelBuilder.Entity<WaterBill>()
                .HasOne(b => b.Reading)
                .WithOne(r => r.Bill)
                .HasForeignKey<WaterBill>(b => b.WaterMeterReadingId);

            modelBuilder.Entity<UnitPayments>()
                .HasOne(p => p.WaterBill)
                .WithOne(b => b.UnitPayments)
                .HasForeignKey<UnitPayments>(p => p.WaterBillId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UnitPayments>()
                .HasIndex(p => new { p.UnitId, p.PeriodMonth, p.PeriodYear })
                .IsUnique();
        }
    }
}