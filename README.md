# Entain - Asynchronous Web Page Download Exercise

This solution contains a .NET 10 console application that downloads multiple web pages concurrently and persists the results to disk.

The implementation is intentionally structured like a small backend component rather than a single script. It demonstrates dependency injection, logging, configuration, separation of concerns, throttling, persistence, and deterministic tests.

## Project Structure

- `Entain.AsyncPageDownloader/Program.cs` - composition root and console entry point
- `Entain.AsyncPageDownloader/appsettings.json` - downloader and logging configuration
- `Entain.AsyncPageDownloader/Options/DownloaderOptions.cs` - strongly typed settings
- `Entain.AsyncPageDownloader/Service/WebPageDownloader.cs` - reusable async downloader with throttling
- `Entain.AsyncPageDownloader/Service/DownloadJobRunner.cs` - orchestration service
- `Entain.AsyncPageDownloader/Service/FileDownloadResultStore.cs` - result persistence
- `Entain.AsyncPageDownloader/Service/WebPageDownloadResult.cs` - per-URL result model
- `Entain.AsyncPageDownloader.Tests/Application/Service/WebPageDownloaderTests.cs` - deterministic unit tests

## Run the Application

```powershell
dotnet run --project .\Entain.AsyncPageDownloader\Entain.AsyncPageDownloader.csproj -- https://example.com https://dotnet.microsoft.com https://github.com
```

The application prints one line per URL, including whether the download succeeded, the HTTP status code, and the downloaded content length. It also writes all results to the configured output file.

## Configuration

Settings are defined in `appsettings.json`:

```json
{
  "Downloader": {
    "MaxConcurrentDownloads": 4,
    "RequestTimeoutSeconds": 30,
    "OutputFilePath": "download-results.json"
  }
}
```

These values can also be overridden with environment variables using the `ENTAIN_` prefix, for example:

```powershell
$env:ENTAIN_Downloader__MaxConcurrentDownloads = "2"
```

## Run the Tests

```powershell
dotnet test .\Entain.AsyncPageDownloader.sln
```

The tests use a fake `HttpMessageHandler`, so they do not depend on internet access or external websites being available.

## Design Notes

- Dependency injection is used in `Program.cs` to register services and configure `HttpClient`.
- `IHttpClientFactory` is used via `AddHttpClient`, avoiding manual `HttpClient` construction in application code.
- Logging is provided through `Microsoft.Extensions.Logging`.
- Configuration is strongly typed with `DownloaderOptions` and validated on startup.
- Throttling is implemented with `SemaphoreSlim` using `MaxConcurrentDownloads`.
- Results are persisted through `IDownloadResultStore`, keeping storage separate from download logic.
- `DownloadJobRunner` coordinates the workflow while `WebPageDownloader` only handles page downloads.
- Each URL returns its own `WebPageDownloadResult`, so one failed request does not prevent other downloads from completing.
- Invalid URLs are reported as failed results instead of throwing from the whole batch.
- Cancellation is supported via `CancellationToken`.
