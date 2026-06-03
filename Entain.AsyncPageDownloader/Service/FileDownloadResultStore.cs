using System.Text.Json;
using Entain.AsyncPageDownloader.Interface.Service;
using Entain.AsyncPageDownloader.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Entain.AsyncPageDownloader.Service;

public sealed class FileDownloadResultStore(
    IOptions<DownloaderOptions> options,
    ILogger<FileDownloadResultStore> logger) : IDownloadResultStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly DownloaderOptions _options = options.Value;

    public async Task SaveAsync(
        IReadOnlyCollection<WebPageDownloadResult> results,
        CancellationToken cancellationToken = default)
    {
        var outputPath = Path.GetFullPath(_options.OutputFilePath);
        var outputDirectory = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrWhiteSpace(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        await using var fileStream = File.Create(outputPath);
        await JsonSerializer.SerializeAsync(fileStream, results, JsonOptions, cancellationToken);

        logger.LogInformation("Persisted {ResultCount} download results to {OutputPath}.", results.Count, outputPath);
    }
}
