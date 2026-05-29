using Entain.Application.Service;

if (args.Length == 0 || args.Contains("--help", StringComparer.OrdinalIgnoreCase))
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project Entain.AsyncPageDownloader/Entain.AsyncPageDownloader.csproj -- <url1> <url2> <url3>");
    Console.WriteLine();
    Console.WriteLine("Example:");
    Console.WriteLine("  dotnet run --project Entain.AsyncPageDownloader/Entain.AsyncPageDownloader.csproj -- https://example.com https://dotnet.microsoft.com");
    return;
}

using var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30)
};

var downloader = new WebPageDownloader(httpClient);
var results = await downloader.DownloadPagesAsync(args);

foreach (var result in results)
{
    if (result.IsSuccess)
    {
        Console.WriteLine($"Downloaded {result.Url} ({result.StatusCode}, {result.Content!.Length:N0} characters)");
        continue;
    }

    var status = result.StatusCode is null ? string.Empty : $" ({result.StatusCode})";
    Console.WriteLine($"Failed {result.Url}{status}: {result.ErrorMessage}");
}
