namespace Entain.Application.Service;

public sealed record WebPageDownloadResult(
    string Url,
    bool IsSuccess,
    int? StatusCode,
    string? Content,
    string? ErrorMessage)
{
    public static WebPageDownloadResult Success(string url, int statusCode, string content)
    {
        return new WebPageDownloadResult(url, true, statusCode, content, null);
    }

    public static WebPageDownloadResult Failure(string url, string errorMessage, int? statusCode = null)
    {
        return new WebPageDownloadResult(url, false, statusCode, null, errorMessage);
    }
}