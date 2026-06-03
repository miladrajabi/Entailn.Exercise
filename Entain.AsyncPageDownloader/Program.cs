using Entain.AsyncPageDownloader.Interface.Service;
using Entain.AsyncPageDownloader.Options;
using Entain.AsyncPageDownloader.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

if (args.Length == 0 || args.Contains("--help", StringComparer.OrdinalIgnoreCase))
{
    Console.WriteLine("Usage:");
    Console.WriteLine(
        "  dotnet run --project Entain.AsyncPageDownloader/Entain.AsyncPageDownloader.csproj -- <url1> <url2> <url3>");
    Console.WriteLine();
    Console.WriteLine("Example:");
    Console.WriteLine(
        "  dotnet run --project Entain.AsyncPageDownloader/Entain.AsyncPageDownloader.csproj -- https://example.com https://dotnet.microsoft.com");
    return;
}

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables(prefix: "ENTAIN_")
    .Build();

var services = new ServiceCollection();

services
    .AddOptions<DownloaderOptions>()
    .Bind(configuration.GetSection(DownloaderOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddLogging(builder =>
{
    builder.AddConfiguration(configuration.GetSection("Logging"));
    builder.AddConsole();
});

services.AddHttpClient<IWebPageDownloader, WebPageDownloader>((provider, client) =>
{
    var options = provider.GetRequiredService<IOptions<DownloaderOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds);
});

services.AddSingleton<IDownloadResultStore, FileDownloadResultStore>();
services.AddSingleton<IDownloadJobRunner, DownloadJobRunner>();

await using var serviceProvider = services.BuildServiceProvider(
    new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

var runner = serviceProvider.GetRequiredService<IDownloadJobRunner>();
var summary = await runner.RunAsync(args);

foreach (var result in summary.Results)
{
    if (result.IsSuccess)
    {
        Console.WriteLine($"Downloaded {result.Url} ({result.StatusCode}, {result.Content!.Length:N0} characters)");
        continue;
    }

    var status = result.StatusCode is null ? string.Empty : $" ({result.StatusCode})";
    Console.WriteLine($"Failed {result.Url}{status}: {result.ErrorMessage}");
}

Console.WriteLine();
Console.WriteLine($"Summary: {summary.Succeeded}/{summary.Total} succeeded, {summary.Failed} failed.");
Console.WriteLine($"Results saved to: {summary.OutputFilePath}");
