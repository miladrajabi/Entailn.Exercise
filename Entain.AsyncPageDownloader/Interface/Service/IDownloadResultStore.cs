using Entain.AsyncPageDownloader.Service;

namespace Entain.AsyncPageDownloader.Interface.Service;

public interface IDownloadResultStore
{
    Task SaveAsync(IReadOnlyCollection<WebPageDownloadResult> results, CancellationToken cancellationToken = default);
}
