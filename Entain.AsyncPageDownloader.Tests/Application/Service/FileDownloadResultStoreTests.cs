using System.Text.Json;
using Entain.AsyncPageDownloader.Options;
using Entain.AsyncPageDownloader.Service;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Entain.AsyncPageDownloader.Tests.Application.Service;

public class FileDownloadResultStoreTests
{
    [Fact]
    public async Task SaveAsync_ShouldPersistResultsToConfiguredFile()
    {
        // Arrange
        var outputDirectory = Path.Combine(Path.GetTempPath(), $"download-results-{Guid.NewGuid():N}");
        var outputFilePath = Path.Combine(outputDirectory, "results.json");

        var store = new FileDownloadResultStore(
            Microsoft.Extensions.Options.Options.Create(new DownloaderOptions { OutputFilePath = outputFilePath }),
            NullLogger<FileDownloadResultStore>.Instance);

        WebPageDownloadResult[] results =
        [
            WebPageDownloadResult.Success("https://example.com", 200, "Example content"),
            WebPageDownloadResult.Failure("https://example.com/missing", "Not found", 404)
        ];

        try
        {
            // Act
            await store.SaveAsync(results);

            // Assert
            File.Exists(outputFilePath).Should().BeTrue();

            var persistedResults = JsonSerializer.Deserialize<IReadOnlyList<WebPageDownloadResult>>(
                await File.ReadAllTextAsync(outputFilePath),
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            persistedResults.Should().BeEquivalentTo(results);
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, recursive: true);
        }
    }
}
