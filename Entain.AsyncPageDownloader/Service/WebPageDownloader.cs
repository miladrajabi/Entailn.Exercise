using Entain.AsyncPageDownloader.Interface.Service;
using Entain.AsyncPageDownloader.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Entain.AsyncPageDownloader.Service;

public sealed class WebPageDownloader(
    HttpClient httpClient,
    IOptions<DownloaderOptions> options,
    ILogger<WebPageDownloader> logger) : IWebPageDownloader
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly DownloaderOptions _options = options.Value;

    public async Task<IReadOnlyList<WebPageDownloadResult>> DownloadPagesAsync(
        IEnumerable<string> urls,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(urls);

        using var throttler = new SemaphoreSlim(_options.MaxConcurrentDownloads);
        var downloadTasks = urls.Select(url => DownloadPageWithThrottleAsync(url, throttler, cancellationToken));

        return await Task.WhenAll(downloadTasks);
    }

    private async Task<WebPageDownloadResult> DownloadPageWithThrottleAsync(
        string url,
        SemaphoreSlim throttler,
        CancellationToken cancellationToken)
    {
        await throttler.WaitAsync(cancellationToken);

        try
        {
            return await DownloadPageAsync(url, cancellationToken);
        }
        finally
        {
            throttler.Release();
        }
    }

    private async Task<WebPageDownloadResult> DownloadPageAsync(
        string url,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            logger.LogWarning("Skipping invalid URL: {Url}", url);
            return WebPageDownloadResult.Failure(url, "The URL is not a valid absolute URI.");
        }

        try
        {
            logger.LogInformation("Downloading {Url}.", url);
            using var response = await _httpClient.GetAsync(uri, cancellationToken);
            var statusCode = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Download failed for {Url} with status code {StatusCode}.", url, statusCode);
                return WebPageDownloadResult.Failure(
                    url,
                    $"Request failed with HTTP status code {statusCode}.",
                    statusCode);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogInformation("Downloaded {Url} with {CharacterCount} characters.", url, content.Length);

            return WebPageDownloadResult.Success(url, statusCode, content);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Download failed for {Url}.", url);
            return WebPageDownloadResult.Failure(url, ex.Message);
        }
    }
}
