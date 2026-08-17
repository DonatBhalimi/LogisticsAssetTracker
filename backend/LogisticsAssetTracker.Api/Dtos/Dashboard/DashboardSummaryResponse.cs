namespace LogisticsAssetTracker.Api.Dtos.Dashboard;

public class DashboardSummaryResponse
{
    public int TotalActiveAssets { get; set; }
    public int AvailableAssets { get; set; }
    public int InTransitAssets { get; set; }
    public int DamagedAssets { get; set; }
    public int LostAssets { get; set; }
    public int RiskyAssets { get; set; }
    public int SuspiciousActivityCount { get; set; }
    public int PendingApprovals { get; set; }
}
