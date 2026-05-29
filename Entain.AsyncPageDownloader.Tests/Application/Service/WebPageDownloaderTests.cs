using System.Net;
using Entain.Application.Interface.Service;
using Entain.Application.Service;
using FluentAssertions;
using Xunit;

namespace Entailn.Test.Application.Service;

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

        IWebPageDownloader webPageDownloader = new WebPageDownloader(httpClient);
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
        IWebPageDownloader webPageDownloader = new WebPageDownloader(httpClient);

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
        IWebPageDownloader webPageDownloader = new WebPageDownloader(httpClient);

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

        IWebPageDownloader webPageDownloader = new WebPageDownloader(httpClient);

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

    private static HttpClient CreateHttpClient(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        return new HttpClient(new StubHttpMessageHandler(responseFactory));
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = responseFactory(request);
            response.RequestMessage = request;

            return Task.FromResult(response);
        }
    }
}