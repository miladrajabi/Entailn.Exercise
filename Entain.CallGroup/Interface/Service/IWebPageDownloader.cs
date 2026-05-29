using Entain.Application.Service;

namespace Entain.Application.Interface.Service;

public interface IWebPageDownloader
{
    Task<IReadOnlyList<WebPageDownloadResult>> DownloadPagesAsync(
        IEnumerable<string> urls,
        CancellationToken cancellationToken = default);
}
