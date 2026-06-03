using Entain.AsyncPageDownloader.Service;

namespace Entain.AsyncPageDownloader.Interface.Service;

public interface IDownloadJobRunner
{
    Task<DownloadSummary> RunAsync(IReadOnlyCollection<string> urls, CancellationToken cancellationToken = default);
}
