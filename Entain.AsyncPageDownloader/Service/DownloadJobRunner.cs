using Entain.AsyncPageDownloader.Interface.Service;
using Entain.AsyncPageDownloader.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Entain.AsyncPageDownloader.Service;

public sealed class DownloadJobRunner(
    IWebPageDownloader downloader,
    IDownloadResultStore resultStore,
    IOptions<DownloaderOptions> options,
    ILogger<DownloadJobRunner> logger) : IDownloadJobRunner
{
    private readonly DownloaderOptions _options = options.Value;

    public async Task<DownloadSummary> RunAsync(
        IReadOnlyCollection<string> urls,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(urls);

        logger.LogInformation(
            "Starting download job for {UrlCount} URLs with concurrency limit {MaxConcurrentDownloads}.",
            urls.Count,
            _options.MaxConcurrentDownloads);

        var results = await downloader.DownloadPagesAsync(urls, cancellationToken);
        await resultStore.SaveAsync(results, cancellationToken);

        var succeeded = results.Count(result => result.IsSuccess);
        var failed = results.Count - succeeded;

        logger.LogInformation(
            "Finished download job. Succeeded: {Succeeded}. Failed: {Failed}.",
            succeeded,
            failed);

        return new DownloadSummary(
            results.Count,
            succeeded,
            failed,
            Path.GetFullPath(_options.OutputFilePath),
            results);
    }
}
