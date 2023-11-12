using System.Reflection;
using NSubstitute;
using Squadtalk.Server.Services;
using Squadtalk.Shared;
using tusdotnet.Interfaces;
using tusdotnet.Models;

namespace Tests;

[Collection("Tus collection")]
public class EmbedService_Tests : IClassFixture<TusFixture>
{
    private readonly TusFixture _fixture;

    public EmbedService_Tests(TusFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateEmbed_File_Success()
    {
        var fileId = await _fixture.CreateTestFileAsync("Usings.cs");

        var generatorSubstitute = Substitute.For<IImagePreviewGenerator>();
        generatorSubstitute.ShouldResize(0, 0).ReturnsForAnyArgs(false);

        var embedService = new EmbedService(generatorSubstitute);
        var file = await _fixture.Store.GetFileAsync(fileId, CancellationToken.None).ConfigureAwait(false);
        var result = await embedService.CreateEmbedAsync(file, "http", "localhost", CancellationToken.None)
            .ConfigureAwait(false);

        Assert.Equal(EmbedType.File, result.Type);
        Assert.Equal("Usings.cs", result.Data["Filename"]);
        Assert.Equal($"http://localhost/api/File?id={fileId}", result.Data["Uri"]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateEmbed_Image_Success(bool resize)
    {
        var fileId = await _fixture.CreateTestFileAsync("Beautiful-Sunflower.jpg");

        var generatorSubstitute = Substitute.For<IImagePreviewGenerator>();
        generatorSubstitute.ShouldResize(0, 0).ReturnsForAnyArgs(resize);
        generatorSubstitute.CreateImagePreviewAsync(null!, CancellationToken.None)
            .ReturnsForAnyArgs(("fakeid", 123, 321));

        var embedService = new EmbedService(generatorSubstitute);
        var file = await _fixture.Store.GetFileAsync(fileId, CancellationToken.None).ConfigureAwait(false);

        await using var content = await file.GetContentAsync(CancellationToken.None);

        var fileSubstitute = Substitute.For<ITusFile>();
        fileSubstitute.GetContentAsync(CancellationToken.None).ReturnsForAnyArgs(content);
        fileSubstitute.Id.Returns(fileId);

        var metadata = await file.GetMetadataAsync(CancellationToken.None);
        var metaType = typeof(Metadata);
        var ctorInfo = metaType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
            new[] { typeof(byte[]) }, null);
        Assert.NotNull(ctorInfo);

        metadata["width"] = (Metadata) ctorInfo.Invoke(new object[] { "900"u8.ToArray() });
        metadata["height"] = (Metadata) ctorInfo.Invoke(new object[] { "900"u8.ToArray() });

        fileSubstitute.GetMetadataAsync(CancellationToken.None).Returns(metadata);

        var result = await embedService.CreateEmbedAsync(fileSubstitute, "http", "localhost", CancellationToken.None)
            .ConfigureAwait(false);

        Assert.Equal(EmbedType.Image, result.Type);
        Assert.Equal($"http://localhost/api/File?id={fileId}", result.Data["Uri"]);
    }
}