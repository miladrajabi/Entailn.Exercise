namespace Entain.Application.Service;

public sealed record WebPageDownloadResult(
    string Url,
    bool IsSuccess,
    string? Content,
    string? ErrorMessage)
{
    public static WebPageDownloadResult Success(string url, string content) =>
        new(url, true, content, null);

    public static WebPageDownloadResult Failure(string url, string errorMessage) =>
        new(url, false, null, errorMessage);
}
