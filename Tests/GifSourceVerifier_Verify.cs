using System.Net;
using NSubstitute;
using RichardSzalay.MockHttp;
using Squadtalk.Server.Services;

namespace Tests;

public class GifSourceVerifier_Verify
{
    [Theory]
    [InlineData("https://media.tenor.com/gif")]
    [InlineData("https://media.giphy.com/media/gif")]
    public async Task Verify_CorrectLink_ReturnsTrue(string value)
    {
        var mockClient = new MockHttpMessageHandler();
        mockClient.When(value).Respond(HttpStatusCode.OK);
        var client = mockClient.ToHttpClient();

        var clientFactory = Substitute.For<IHttpClientFactory>();
        clientFactory.CreateClient("GifVerifier").Returns(client);

        var verifier = new GifSourceVerifierService(clientFactory);
        var result = await verifier.VerifyAsync(value);

        Assert.True(result);
    }

    [Theory]
    [InlineData("https://tenor.com/gif")]
    [InlineData("http://tenor.com/gif")]
    [InlineData("tenor.com/gif")]
    [InlineData("https://media.giphy")]
    [InlineData("https:/media.giphy")]
    [InlineData("gif")]
    [InlineData("")]
    public async Task VerifyAsync_InvalidSource_ReturnsFalse(string value)
    {
        var mockClient = new MockHttpMessageHandler();
        mockClient.When(value).Respond(HttpStatusCode.OK);
        var client = mockClient.ToHttpClient();

        var clientFactory = Substitute.For<IHttpClientFactory>();
        clientFactory.CreateClient("GifVerifier").Returns(client);

        var verifier = new GifSourceVerifierService(clientFactory);
        var result = await verifier.VerifyAsync(value);

        Assert.False(result);
    }
}