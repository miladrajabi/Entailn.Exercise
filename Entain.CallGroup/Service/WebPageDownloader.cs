using Entain.Application.Interface.Service;

namespace Entain.Application.Service;

public class WebPageDownloader : IWebPageDownloader
{
    private readonly HttpClient _httpClient;

    public WebPageDownloader(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<IReadOnlyList<WebPageDownloadResult>> DownloadPagesAsync(
        IEnumerable<string> urls,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(urls);

        var downloadTasks = urls.Select(url => DownloadPageAsync(url, cancellationToken));
        return await Task.WhenAll(downloadTasks);
    }

    private async Task<WebPageDownloadResult> DownloadPageAsync(
        string url,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return WebPageDownloadResult.Failure(url, "The URL is not a valid absolute URI.");

        try
        {
            var content = await _httpClient.GetStringAsync(uri, cancellationToken);
            return WebPageDownloadResult.Success(url, content);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return WebPageDownloadResult.Failure(url, ex.Message);
        }
    }
}
