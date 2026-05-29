# Entain - Asynchronous Web Page Download Exercise

This solution contains a small .NET 10 console application that downloads multiple web pages concurrently.

## Project Structure

- `Entain.AsyncPageDownloader/Program.cs` - console entry point
- `Entain.AsyncPageDownloader/Service/WebPageDownloader.cs` - async web page downloader
- `Entain.AsyncPageDownloader/Service/WebPageDownloadResult.cs` - per-URL result model
- `Entain.AsyncPageDownloader.Tests/Application/Service/WebPageDownloaderTests.cs` - deterministic unit tests

## Run the Application

```powershell
dotnet run --project .\Entain.AsyncPageDownloader\Entain.AsyncPageDownloader.csproj -- https://example.com https://dotnet.microsoft.com https://github.com
```

The application prints one line per URL, including whether the download succeeded, the HTTP status code, and the downloaded content length.

## Run the Tests

```powershell
dotnet test .\Entain.AsyncPageDownloader.sln
```

The tests use a fake `HttpMessageHandler`, so they do not depend on internet access or external websites being available.

## Implementation Notes

- Downloads are started together and awaited with `Task.WhenAll`.
- `HttpClient` is injected into `WebPageDownloader`, making the downloader testable.
- Each URL returns its own `WebPageDownloadResult`, so one failed request does not prevent other downloads from completing.
- Invalid URLs are reported as failed results instead of throwing from the whole batch.
- Cancellation is supported via `CancellationToken`.
