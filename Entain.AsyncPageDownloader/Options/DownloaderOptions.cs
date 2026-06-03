using System.ComponentModel.DataAnnotations;

namespace Entain.AsyncPageDownloader.Options;

public sealed class DownloaderOptions
{
    public const string SectionName = "Downloader";

    [Range(1, 100)]
    public int MaxConcurrentDownloads { get; init; } = 4;

    [Range(1, 600)]
    public int RequestTimeoutSeconds { get; init; } = 30;

    [Required]
    public string OutputFilePath { get; init; } = "download-results.json";
}
