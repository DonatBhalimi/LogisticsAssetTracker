using LogisticsAssetTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAssetTracker.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AssetMovement> AssetMovements => Set<AssetMovement>();
    public DbSet<MovementApproval> MovementApprovals => Set<MovementApproval>();
    public DbSet<RiskRecommendation> RiskRecommendations => Set<RiskRecommendation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(u => u.NormalizedEmail).IsUnique();
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.Property(l => l.Type).HasConversion<string>().HasMaxLength(30);
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.Property(a => a.Type).HasConversion<string>().HasMaxLength(30);
            entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(a => a.Condition).HasConversion<string>().HasMaxLength(20);

            entity.HasIndex(a => a.NormalizedAssetCode).IsUnique();
            entity.HasIndex(a => a.QrCodeValue).IsUnique();

            entity.HasOne(a => a.CurrentLocation)
                .WithMany()
                .HasForeignKey(a => a.CurrentLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.AssignedToUser)
                .WithMany()
                .HasForeignKey(a => a.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });

        modelBuilder.Entity<AssetMovement>(entity =>
        {
            entity.Property(m => m.PreviousStatus).HasConversion<string>().HasMaxLength(20);
            entity.Property(m => m.NewStatus).HasConversion<string>().HasMaxLength(20);
            entity.Property(m => m.PreviousCondition).HasConversion<string>().HasMaxLength(20);
            entity.Property(m => m.NewCondition).HasConversion<string>().HasMaxLength(20);
            entity.Property(m => m.SourceType).HasConversion<string>().HasMaxLength(10);

            entity.HasIndex(m => m.AssetId);

            entity.HasOne(m => m.Asset)
                .WithMany()
                .HasForeignKey(m => m.AssetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.FromLocation)
                .WithMany()
                .HasForeignKey(m => m.FromLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.ToLocation)
                .WithMany()
                .HasForeignKey(m => m.ToLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.UpdatedByUser)
                .WithMany()
                .HasForeignKey(m => m.UpdatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MovementApproval>(entity =>
        {
            entity.Property(m => m.RequestedStatus).HasConversion<string>().HasMaxLength(20);
            entity.Property(m => m.RequestedCondition).HasConversion<string>().HasMaxLength(20);
            entity.Property(m => m.RequestedSourceType).HasConversion<string>().HasMaxLength(10);
            entity.Property(m => m.ApprovalStatus).HasConversion<string>().HasMaxLength(20);

            entity.HasIndex(m => m.AssetId);

            entity.HasOne(m => m.Asset)
                .WithMany()
                .HasForeignKey(m => m.AssetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.RequestedByUser)
                .WithMany()
                .HasForeignKey(m => m.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.DecidedByUser)
                .WithMany()
                .HasForeignKey(m => m.DecidedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            entity.HasOne(m => m.RequestedLocation)
                .WithMany()
                .HasForeignKey(m => m.RequestedLocationId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });

        modelBuilder.Entity<RiskRecommendation>(entity =>
        {
            entity.Property(r => r.RiskLevel).HasConversion<string>().HasMaxLength(20);
            entity.Property(r => r.RiskType).HasConversion<string>().HasMaxLength(30);
            entity.Property(r => r.GenerationSource).HasConversion<string>().HasMaxLength(30);

            entity.HasIndex(r => r.AssetId);

            entity.HasOne(r => r.Asset)
                .WithMany()
                .HasForeignKey(r => r.AssetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.GeneratedByUser)
                .WithMany()
                .HasForeignKey(r => r.GeneratedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });
    }
}
