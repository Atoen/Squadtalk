using System.Text;
using tusdotnet.Stores;

namespace Tests;

public sealed class TusFixture : IDisposable
{
    public string StorePath => "../../../Tus";
    public string Path => "../../..";
    public TusDiskStore Store { get; }
    
    public TusFixture()
    {
        Directory.CreateDirectory(StorePath);
        Store = new TusDiskStore(StorePath);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(StorePath, true);
        }
        catch (IOException)
        {
        }
    }
    
    public async Task<string> CreateTestFileAsync(string file)
    {
        var path = System.IO.Path.Combine(Path, file);
        var fileInfo = new FileInfo(path);

        if (!fileInfo.Exists)
        {
            Assert.Fail($"Test file not found: {file}");
        }

        var filename = Encoding.UTF8.GetBytes(fileInfo.Name);
        var filetype = "image/jpg"u8.ToArray();
        var filesize = Encoding.UTF8.GetBytes(fileInfo.Length.ToString());

        var name64 = Convert.ToBase64String(filename);
        var type64 = Convert.ToBase64String(filetype);
        var size64 = Convert.ToBase64String(filesize);

        var meta = $"filename {name64},filetype {type64},filesize {size64}";

        await using var stream = fileInfo.OpenRead();

        var id = await Store.CreateFileAsync(stream.Length, meta, CancellationToken.None).ConfigureAwait(false);
        Assert.NotEmpty(id);

        await Store.SetUploadLengthAsync(id, stream.Length, CancellationToken.None).ConfigureAwait(false);
        await Store.AppendDataAsync(id, stream, CancellationToken.None).ConfigureAwait(false);

        return id;
    }
}

[CollectionDefinition("Tus collection")]
public class TusCollection : ICollectionFixture<TusFixture>
{
}