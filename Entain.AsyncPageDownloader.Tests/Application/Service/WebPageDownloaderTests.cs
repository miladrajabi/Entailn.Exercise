using System.Net;
using Entain.AsyncPageDownloader.Interface.Service;
using Entain.AsyncPageDownloader.Options;
using Entain.AsyncPageDownloader.Service;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Entain.AsyncPageDownloader.Tests.Application.Service;

public class WebPageDownloaderTests
{
    [Fact]
    public async Task DownloadPagesAsync_ShouldReturnContent_ForValidUrls()
    {
        // Arrange
        using var httpClient = CreateHttpClient(request =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"Content for {request.RequestUri}")
            });

        IWebPageDownloader webPageDownloader = CreateDownloader(httpClient);
        string[] urls =
        [
            "https://example.com/page-one",
            "https://example.com/page-two",
            "https://example.com/page-three"
        ];

        // Act
        var result = await webPageDownloader.DownloadPagesAsync(urls);

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(page => page.IsSuccess);
        result.Should().OnlyContain(page => page.StatusCode == 200);
        result.Select(page => page.Url).Should().Equal(urls);
        result.Select(page => page.Content).Should().OnlyContain(content => !string.IsNullOrWhiteSpace(content));
    }

    [Fact]
    public async Task DownloadPagesAsync_ShouldReturnEmptyResult_WhenUrlsAreEmpty()
    {
        // Arrange
        using var httpClient = CreateHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK));
        IWebPageDownloader webPageDownloader = CreateDownloader(httpClient);

        // Act
        var result = await webPageDownloader.DownloadPagesAsync([]);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DownloadPagesAsync_ShouldReturnFailure_WhenUrlIsInvalid()
    {
        // Arrange
        using var httpClient = CreateHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK));
        IWebPageDownloader webPageDownloader = CreateDownloader(httpClient);

        // Act
        var result = await webPageDownloader.DownloadPagesAsync(["not-a-url"]);

        // Assert
        result.Should().ContainSingle();
        result[0].IsSuccess.Should().BeFalse();
        result[0].StatusCode.Should().BeNull();
        result[0].ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task DownloadPagesAsync_ShouldReturnFailure_ForFailedRequestWithoutStoppingOtherDownloads()
    {
        // Arrange
        using var httpClient = CreateHttpClient(request =>
        {
            if (request.RequestUri!.AbsolutePath.Contains("missing"))
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Successful page")
            };
        });

        IWebPageDownloader webPageDownloader = CreateDownloader(httpClient);

        // Act
        var result = await webPageDownloader.DownloadPagesAsync(
        [
            "https://example.com/exists",
            "https://example.com/missing"
        ]);

        // Assert
        result.Should().HaveCount(2);
        result[0].IsSuccess.Should().BeTrue();
        result[1].IsSuccess.Should().BeFalse();
        result[1].StatusCode.Should().Be(404);
        result[1].ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task DownloadPagesAsync_ShouldRespectConfiguredConcurrencyLimit()
    {
        // Arrange
        var activeRequests = 0;
        var maxObservedConcurrency = 0;

        using var httpClient = CreateHttpClient(async _ =>
        {
            var current = Interlocked.Increment(ref activeRequests);
            maxObservedConcurrency = Math.Max(maxObservedConcurrency, current);

            await Task.Delay(50);

            Interlocked.Decrement(ref activeRequests);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Successful page")
            };
        });

        IWebPageDownloader webPageDownloader = CreateDownloader(
            httpClient,
            new DownloaderOptions { MaxConcurrentDownloads = 2 });

        // Act
        var result = await webPageDownloader.DownloadPagesAsync(
        [
            "https://example.com/one",
            "https://example.com/two",
            "https://example.com/three",
            "https://example.com/four"
        ]);

        // Assert
        result.Should().OnlyContain(page => page.IsSuccess);
        maxObservedConcurrency.Should().BeLessThanOrEqualTo(2);
    }

    private static IWebPageDownloader CreateDownloader(
        HttpClient httpClient,
        DownloaderOptions? options = null)
    {
        return new WebPageDownloader(
            httpClient,
            Microsoft.Extensions.Options.Options.Create(options ?? new DownloaderOptions()),
            NullLogger<WebPageDownloader>.Instance);
    }

    private static HttpClient CreateHttpClient(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        return CreateHttpClient(request => Task.FromResult(responseFactory(request)));
    }

    private static HttpClient CreateHttpClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFactory)
    {
        return new HttpClient(new StubHttpMessageHandler(responseFactory));
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFactory) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await responseFactory(request);
            response.RequestMessage = request;

            return response;
        }
    }
}
