namespace LogisticsAssetTracker.Api.Dtos.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserSummaryResponse User { get; set; } = null!;
}
