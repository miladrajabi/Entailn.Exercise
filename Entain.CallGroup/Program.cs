using Entain.Application.Service;

var urls = args.Length > 0
    ? args
    :
    [
        "https://www.google.com",
        "https://dotnet.microsoft.com",
        "https://github.com"
    ];

using var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30)
};

var downloader = new WebPageDownloader(httpClient);
var results = await downloader.DownloadPagesAsync(urls);

foreach (var result in results)
{
    if (result.IsSuccess)
    {
        Console.WriteLine($"Downloaded {result.Url} ({result.Content!.Length:N0} characters)");
        continue;
    }

    Console.WriteLine($"Failed {result.Url}: {result.ErrorMessage}");
}
