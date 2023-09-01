using System.Text;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NSubstitute.Extensions;
using Squadtalk.Server.Services;
using Squadtalk.Shared;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using BindingFlags = System.Reflection.BindingFlags;

namespace Tests;

public class EmbedService_Tests
{
    [Fact]
    public async Task CreateEmbed_File_Success()
    {
        var configSubstitute = Substitute.For<IConfiguration>();
        configSubstitute["Tus:Address"].Returns("../../../Tus");
        var helperSubstitute = Substitute.For<TusDiskStoreHelper>(configSubstitute);

        var filePath = "../../../Usings.cs";
        var fileInfo = new FileInfo(filePath);
        
        if (!fileInfo.Exists)
        {
            Assert.Fail("Test file not found: Usings.cs");
        }
        
        var filename = Encoding.UTF8.GetBytes(fileInfo.Name); 
        var filetype = "image/jpg"u8.ToArray();
        var filesize = Encoding.UTF8.GetBytes(fileInfo.Length.ToString());

        var name64 = Convert.ToBase64String(filename);
        var type64 = Convert.ToBase64String(filetype);
        var size64 = Convert.ToBase64String(filesize);

        var meta = $"filename {name64},filetype {type64},filesize {size64}";
        
        await using var stream = fileInfo.OpenRead();
        stream.Seek(0, SeekOrigin.Begin);

        var store = helperSubstitute.Store;
        
        var id = await store.CreateFileAsync(stream.Length, meta, CancellationToken.None).ConfigureAwait(false);
        Assert.NotEmpty(id);
        
        await store.SetUploadLengthAsync(id, stream.Length, CancellationToken.None).ConfigureAwait(false);
        await store.AppendDataAsync(id, stream, CancellationToken.None).ConfigureAwait(false);

        var generatorSubstitute = Substitute.For<IImagePreviewGenerator>();
        generatorSubstitute.ShouldResize(0, 0).ReturnsForAnyArgs(false);
        
        var embedService = new EmbedService(generatorSubstitute);
        var file = await store.GetFileAsync(id, CancellationToken.None).ConfigureAwait(false);
        var result = await embedService.CreateEmbedAsync(file, "http", "localhost", CancellationToken.None)
            .ConfigureAwait(false);
        
        Assert.Equal(EmbedType.File, result.Type);
        Assert.Equal("Usings.cs", result.Data["Filename"]);
        Assert.Equal($"http://localhost/api/File?id={id}", result.Data["Uri"]);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateEmbed_Image_Success(bool resize)
    {
        var configSubstitute = Substitute.For<IConfiguration>();
        configSubstitute["Tus:Address"].Returns("../../../Tus");
        var helperSubstitute = Substitute.For<TusDiskStoreHelper>(configSubstitute);

        var filePath = "../../../Beautiful-Sunflower.jpg";
        var fileInfo = new FileInfo(filePath);

        if (!fileInfo.Exists)
        {
            Assert.Fail("Test file not found: Beautiful-Sunflower.jpg");
        }
        
        var filename = Encoding.UTF8.GetBytes(fileInfo.Name); 
        var filetype = "image/jpg"u8.ToArray();
        var filesize = Encoding.UTF8.GetBytes(fileInfo.Length.ToString());

        var name64 = Convert.ToBase64String(filename);
        var type64 = Convert.ToBase64String(filetype);
        var size64 = Convert.ToBase64String(filesize);

        var meta = $"filename {name64},filetype {type64},filesize {size64}";
        
        await using var stream = fileInfo.OpenRead();
        stream.Seek(0, SeekOrigin.Begin);

        var store = helperSubstitute.Store;
        
        var id = await store.CreateFileAsync(stream.Length, meta, CancellationToken.None).ConfigureAwait(false);
        Assert.NotEmpty(id);
        
        await store.SetUploadLengthAsync(id, stream.Length, CancellationToken.None).ConfigureAwait(false);
        await store.AppendDataAsync(id, stream, CancellationToken.None).ConfigureAwait(false);

        var generatorSubstitute = Substitute.For<IImagePreviewGenerator>();
        generatorSubstitute.ShouldResize(0, 0).ReturnsForAnyArgs(resize);
        generatorSubstitute.CreateImagePreviewAsync(null!, CancellationToken.None)
            .ReturnsForAnyArgs(("fakeid", 123, 321));
        
        var embedService = new EmbedService(generatorSubstitute);
        var file = await store.GetFileAsync(id, CancellationToken.None).ConfigureAwait(false);

        var fileSubstitute = Substitute.For<ITusFile>();

        fileSubstitute.GetContentAsync(CancellationToken.None)
            .ReturnsForAnyArgs(file.GetContentAsync(CancellationToken.None));

        fileSubstitute.Id.Returns(id);

        var metadata = await file.GetMetadataAsync(CancellationToken.None);
        var metaType = typeof(Metadata);
        var ctorInfo = metaType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(byte[]) }, null);
        Assert.NotNull(ctorInfo);
        
        metadata["width"] = (Metadata) ctorInfo.Invoke(new object[] { "900"u8.ToArray() });
        metadata["height"] = (Metadata) ctorInfo.Invoke(new object[] { "900"u8.ToArray() });

        fileSubstitute.GetMetadataAsync(CancellationToken.None).Returns(metadata);
        
        var result = await embedService.CreateEmbedAsync(fileSubstitute, "http", "localhost", CancellationToken.None)
            .ConfigureAwait(false);
        
        Assert.Equal(EmbedType.Image, result.Type);
        Assert.Equal($"http://localhost/api/File?id={id}", result.Data["Uri"]);
    }
}