using System.Text;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Squadtalk.Server.Services;

namespace Tests;

public class ImagePreviewGenerator_Tests
{
    [Fact]
    public async Task CreateImagePreview_Success()
    {
        var configSubstitute = Substitute.For<IConfiguration>();
        configSubstitute["Tus:Address"].Returns("../../../Tus");
        var helperSubstitute = Substitute.For<TusDiskStoreHelper>(configSubstitute);
        
        var imagePath = "../../../Beautiful-Sunflower.jpg";
        var fileInfo = new FileInfo(imagePath);
        
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
        
        var previewGenerator = new ImagePreviewGeneratorService(helperSubstitute);
        var file = await store.GetFileAsync(id, CancellationToken.None).ConfigureAwait(false);

        var result = await previewGenerator.CreateImagePreviewAsync(file, CancellationToken.None).ConfigureAwait(false);
        
        Assert.NotEmpty(result.id);
        Assert.NotEqual(id, result.id);
        
        Assert.True(result.width <= previewGenerator.MaxWidth && result.height <= previewGenerator.MaxHeight);
        var aspectRatio = (double) result.width / result.height;
        Assert.True(Math.Abs(aspectRatio - 1) < 0.001);
    }

    [Fact]
    public void ShouldResize_True()
    {
        var configSubstitute = Substitute.For<IConfiguration>();
        configSubstitute["Tus:Address"].Returns("fakepath");
        
        var helperSubstitute = Substitute.For<TusDiskStoreHelper>(configSubstitute);
        
        var imagePreviewGenerator = new ImagePreviewGeneratorService(helperSubstitute)
        {
            MaxWidth = 200,
            MaxHeight = 200
        };
        
        Assert.True(imagePreviewGenerator.ShouldResize(20, 300));
        Assert.True(imagePreviewGenerator.ShouldResize(250, 30));
        Assert.True(imagePreviewGenerator.ShouldResize(230, 300));
    }
    
    [Fact]
    public void ShouldResize_False()
    {
        var configSubstitute = Substitute.For<IConfiguration>();
        configSubstitute["Tus:Address"].Returns("fakepath");
        
        var helperSubstitute = Substitute.For<TusDiskStoreHelper>(configSubstitute);
        
        var imagePreviewGenerator = new ImagePreviewGeneratorService(helperSubstitute)
        {
            MaxWidth = 200,
            MaxHeight = 200
        };
        
        Assert.False(imagePreviewGenerator.ShouldResize(200, 200));
        Assert.False(imagePreviewGenerator.ShouldResize(150, 30));
        Assert.False(imagePreviewGenerator.ShouldResize(130, 30));
    }
    
    [Theory]
    [InlineData(600, 400, 500, 700)]
    [InlineData(800, 600, 700, 900)]
    [InlineData(1000, 800, 900, 1100)]
    [InlineData(1200, 1000, 1100, 1300)]
    public void ResizeImage_Width(int imageWidth, int imageHeight, int targetWidth, int targetHeight)
    {
        var image = new Image<Rgba32>(imageWidth, imageHeight);
        var configSubstitute = Substitute.For<IConfiguration>();
        configSubstitute["Tus:Address"].Returns("fakepath");
        
        var helperSubstitute = Substitute.For<TusDiskStoreHelper>(configSubstitute);
        var imagePreviewGenerator = new ImagePreviewGeneratorService(helperSubstitute)
        {
            MaxWidth = targetWidth,
            MaxHeight = targetHeight
        };

        var result = imagePreviewGenerator.GetResizedDimensions(image);
        
        Assert.Equal(imageWidth, image.Width);
        Assert.Equal(imageHeight, image.Height);
        
        Assert.True(targetWidth >= result.width && targetHeight >= result.height);
        Assert.Equal(targetWidth, result.width);
        Assert.Equal(0, result.height);
    }
    
    [Theory]
    [InlineData(100, 200, 500, 500)]
    [InlineData(150, 250, 600, 600)]
    [InlineData(180, 280, 700, 700)]
    [InlineData(190, 300, 800, 800)]
    public void ResizeImage_Height(int imageWidth, int imageHeight, int targetWidth, int targetHeight)
    {
        var image = new Image<Rgba32>(imageWidth, imageHeight);
        var configSubstitute = Substitute.For<IConfiguration>();
        configSubstitute["Tus:Address"].Returns("fakepath");
        
        var helperSubstitute = Substitute.For<TusDiskStoreHelper>(configSubstitute);
        var imagePreviewGenerator = new ImagePreviewGeneratorService(helperSubstitute)
        {
            MaxWidth = targetWidth,
            MaxHeight = targetHeight
        };

        var result = imagePreviewGenerator.GetResizedDimensions(image);
        
        Assert.Equal(imageWidth, image.Width);
        Assert.Equal(imageHeight, image.Height);
        
        Assert.True(targetWidth >= result.width && targetHeight >= result.height);
        Assert.Equal(targetHeight, result.height);
        Assert.Equal(0, result.width);
    }

    [Theory]
    [InlineData(500, 300, 200, 200)]
    [InlineData(300, 500, 250, 200)]
    [InlineData(300, 300, 200, 250)]
    [InlineData(2524, 543, 430, 70)]
    public void ResizeImage_AspectRatio(int imageWidth, int imageHeight, int targetWidth, int targetHeight)
    {
        var image = new Image<Rgba32>(imageWidth, imageHeight);
        var configSubstitute = Substitute.For<IConfiguration>();
        configSubstitute["Tus:Address"].Returns("fakepath");
        
        var helperSubstitute = Substitute.For<TusDiskStoreHelper>(configSubstitute);
        var imagePreviewGenerator = new ImagePreviewGeneratorService(helperSubstitute)
        {
            MaxWidth = targetWidth,
            MaxHeight = targetHeight
        };

        var result = imagePreviewGenerator.GetResizedDimensions(image);
        
        Assert.Equal(imageWidth, image.Width);
        Assert.Equal(imageHeight, image.Height);
        
        Assert.True(targetWidth >= result.width && targetHeight >= result.height);

        var aspectRatio = (double) imageWidth / imageHeight;
        var resizedAspectRatio = (double) result.width / result.height;
        
        Assert.True(Math.Abs(aspectRatio - resizedAspectRatio) < 0.01);
    }
    

    
}