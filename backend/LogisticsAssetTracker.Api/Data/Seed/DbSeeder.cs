using LogisticsAssetTracker.Api.Domain.Entities;
using LogisticsAssetTracker.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAssetTracker.Api.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        var admin = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Ana Admin",
            Email = "admin@logisticstracker.test",
            NormalizedEmail = "ADMIN@LOGISTICSTRACKER.TEST",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var manager = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Marko Manager",
            Email = "manager@logisticstracker.test",
            NormalizedEmail = "MANAGER@LOGISTICSTRACKER.TEST",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123!"),
            Role = UserRole.Manager,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var operatorUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Olga Operator",
            Email = "operator@logisticstracker.test",
            NormalizedEmail = "OPERATOR@LOGISTICSTRACKER.TEST",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Operator123!"),
            Role = UserRole.Operator,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var inactiveUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Ivan Inactive",
            Email = "inactive@logisticstracker.test",
            NormalizedEmail = "INACTIVE@LOGISTICSTRACKER.TEST",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Inactive123!"),
            Role = UserRole.Operator,
            IsActive = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Users.AddRange(admin, manager, operatorUser, inactiveUser);

        var warehouse = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Warehouse Skopje",
            Type = LocationType.Warehouse,
            Address = "Industriska 12, Skopje",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var truck = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Truck 12",
            Type = LocationType.Truck,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var distributionCenter = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Distribution Center North",
            Type = LocationType.DistributionCenter,
            Address = "North Industrial Zone",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var clientSite = new Location
        {
            Id = Guid.NewGuid(),
            Name = "Client Site A",
            Type = LocationType.ClientSite,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Locations.AddRange(warehouse, truck, distributionCenter, clientSite);

        var assets = new[]
        {
            new Asset
            {
                Id = Guid.NewGuid(),
                AssetCode = "ASSET-000001",
                NormalizedAssetCode = "ASSET-000001",
                Name = "Pallet Batch A",
                Type = AssetType.Pallet,
                CurrentLocationId = warehouse.Id,
                Status = AssetStatus.Available,
                Condition = AssetCondition.Good,
                QrCodeValue = Guid.NewGuid().ToString("N"),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Asset
            {
                Id = Guid.NewGuid(),
                AssetCode = "ASSET-000002",
                NormalizedAssetCode = "ASSET-000002",
                Name = "Crate 12B",
                Type = AssetType.Crate,
                CurrentLocationId = truck.Id,
                Status = AssetStatus.InTransit,
                Condition = AssetCondition.Good,
                QrCodeValue = Guid.NewGuid().ToString("N"),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Asset
            {
                Id = Guid.NewGuid(),
                AssetCode = "ASSET-000003",
                NormalizedAssetCode = "ASSET-000003",
                Name = "Container North-1",
                Type = AssetType.Container,
                CurrentLocationId = distributionCenter.Id,
                Status = AssetStatus.Delivered,
                Condition = AssetCondition.Good,
                QrCodeValue = Guid.NewGuid().ToString("N"),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Asset
            {
                Id = Guid.NewGuid(),
                AssetCode = "ASSET-000004",
                NormalizedAssetCode = "ASSET-000004",
                Name = "Handheld Scanner 4",
                Type = AssetType.Device,
                CurrentLocationId = warehouse.Id,
                Status = AssetStatus.Available,
                Condition = AssetCondition.Damaged,
                AssignedToUserId = operatorUser.Id,
                QrCodeValue = Guid.NewGuid().ToString("N"),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Asset
            {
                Id = Guid.NewGuid(),
                AssetCode = "ASSET-000005",
                NormalizedAssetCode = "ASSET-000005",
                Name = "Tool Kit 5",
                Type = AssetType.Tool,
                CurrentLocationId = clientSite.Id,
                Status = AssetStatus.Lost,
                Condition = AssetCondition.Good,
                QrCodeValue = Guid.NewGuid().ToString("N"),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        db.Assets.AddRange(assets);

        await db.SaveChangesAsync();
    }
}
