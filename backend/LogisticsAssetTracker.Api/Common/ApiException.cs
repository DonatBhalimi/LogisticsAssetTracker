namespace LogisticsAssetTracker.Api.Common;

public class ApiException : Exception
{
    public int StatusCode { get; }
    public string Code { get; }
    public object? Details { get; }

    public ApiException(int statusCode, string code, string message, object? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
        Details = details;
    }

    public static ApiException NotFound(string code, string message) =>
        new(404, code, message);

    public static ApiException Conflict(string code, string message) =>
        new(409, code, message);

    public static ApiException Unprocessable(string code, string message) =>
        new(422, code, message);

    public static ApiException Unauthorized(string code, string message) =>
        new(401, code, message);

    public static ApiException Forbidden(string code, string message) =>
        new(403, code, message);
}
