using Entain.AsyncPageDownloader.Service;

namespace Entain.AsyncPageDownloader.Interface.Service;

public interface IWebPageDownloader
{
    Task<IReadOnlyList<WebPageDownloadResult>> DownloadPagesAsync(
        IEnumerable<string> urls,
        CancellationToken cancellationToken = default);
}