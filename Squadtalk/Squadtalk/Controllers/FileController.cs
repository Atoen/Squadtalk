using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Shared;
using Shared.Data.TypedIds;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Data.TypedIds;
using Squadtalk.Extensions;
using Squadtalk.Services;
using tusdotnet.Interfaces;

namespace Squadtalk.Controllers;

[ApiController]
[Route("api/files")]
public class FileController : ControllerBase
{
    private readonly TusHelper _tusHelper;
    private readonly ApplicationDbContext _dbContext;

    public FileController(TusHelper tusHelper, ApplicationDbContext dbContext)
    {
        _tusHelper = tusHelper;
        _dbContext = dbContext;
    }

    private static readonly Func<ApplicationDbContext, NewChannelId, TusFileId, Task<DbFile?>> FileByChannelPathAsync =
        EF.CompileAsyncQuery(
            (ApplicationDbContext context, NewChannelId channelId, TusFileId fileId) => context.Files
                .AsNoTracking()
                .Where(x => x.ChannelId == channelId)
                .SingleOrDefault(x => x.TusId == fileId));

    [HttpGet("{channelId}/{fileId}/{**slug}")]
    public async Task<IActionResult> DownloadFile(NewChannelId channelId, TusFileId fileId)
    {
        var cancellationToken = HttpContext.RequestAborted;

        if (await FileByChannelPathAsync(_dbContext, channelId, fileId) is not { } file)
        {
            return BadRequest("Invalid file path");
        }

        try
        {
            var tusFile = await _tusHelper.DiskStore.GetFileAsync(file.TusId, cancellationToken);
            return tusFile is not null
                ? await SetFileContentDispositionAsync(tusFile, cancellationToken)
                : BadRequest("Invalid file id");
        }
        catch
        {
            return BadRequest("Error during accessing the file");
        }
    }

    private async Task<IActionResult> SetFileContentDispositionAsync(ITusFile tusFile, CancellationToken cancellationToken)
    {
        var metadata = await tusFile.GetMetadataAsync(cancellationToken);
        var filename = metadata.GetString(FileData.FileName);
        var contentType = metadata.GetString(FileData.ContentType);

        var contentDisposition = new ContentDispositionHeaderValue("inline");
        contentDisposition.SetHttpFileName(filename);

        Response.Headers.Append("Content-Disposition", contentDisposition.ToString());

        var stream = await tusFile.GetContentAsync(cancellationToken);
        return File(stream, contentType);
    }
}