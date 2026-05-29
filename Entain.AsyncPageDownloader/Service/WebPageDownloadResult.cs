namespace Entain.Application.Service;

public sealed record WebPageDownloadResult(
    string Url,
    bool IsSuccess,
    int? StatusCode,
    string? Content,
    string? ErrorMessage)
{
    public static WebPageDownloadResult Success(string url, int statusCode, string content) =>
        new(url, true, statusCode, content, null);

    public static WebPageDownloadResult Failure(string url, string errorMessage, int? statusCode = null) =>
        new(url, false, statusCode, null, errorMessage);
}
