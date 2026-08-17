using System.Text.Json;

namespace LogisticsAssetTracker.Api.Common;

// Shared by Phase 5 risk/dashboard code that needs to read the AssetId an AuditLog
// entry's metadata refers to. Deliberately not wired into Phase 4's AssetMovementService,
// which has its own established (and untouched) copy of this same small parsing logic.
public static class AuditMetadataHelper
{
    public static Guid? TryGetAssetId(string? metadataJson) => TryGetGuidProperty(metadataJson, "AssetId");

    public static Guid? TryGetGuidProperty(string? json, string propertyName)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(propertyName, out var prop) &&
                prop.ValueKind == JsonValueKind.String &&
                Guid.TryParse(prop.GetString(), out var value))
            {
                return value;
            }
        }
        catch (JsonException)
        {
            // Malformed metadata is ignored for detection purposes.
        }

        return null;
    }
}
