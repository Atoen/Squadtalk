using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Shared;
using Squadtalk.Extensions;
using Squadtalk.Services;
using tusdotnet.Interfaces;
using tusdotnet.Models;

namespace Squadtalk.Controllers;

[ApiController]
[Route("api/files")]
public class FileController : ControllerBase
{
    private readonly TusHelper _tusHelper;

    public FileController(TusHelper tusHelper)
    {
        _tusHelper = tusHelper;
    }

    [HttpGet("{id}/{**slug}")]
    public async Task<IActionResult> DownloadFile(string id)
    {
        var cancellationToken = HttpContext.RequestAborted;
        ITusFile file;

        try
        {
            file = await _tusHelper.DiskStore.GetFileAsync(id, cancellationToken);
            if (file is null)
            {
                return BadRequest("Invalid file id");
            }
        }
        catch (TusStoreException e)
        {
            return BadRequest(e.Message);
        }

        var metadata = await file.GetMetadataAsync(cancellationToken);
        var filename = metadata.GetString(FileData.FileName);
        var contentType = metadata.GetString(FileData.ContentType);

        var contentDisposition = new ContentDispositionHeaderValue("inline");
        contentDisposition.SetHttpFileName(filename);

        Response.Headers.Append("Content-Disposition", contentDisposition.ToString());

        var stream = await file.GetContentAsync(cancellationToken);
        return File(stream, contentType);
    }
}