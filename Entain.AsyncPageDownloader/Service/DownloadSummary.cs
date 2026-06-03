namespace Entain.AsyncPageDownloader.Service;

public sealed record DownloadSummary(
    int Total,
    int Succeeded,
    int Failed,
    string OutputFilePath,
    IReadOnlyList<WebPageDownloadResult> Results);
