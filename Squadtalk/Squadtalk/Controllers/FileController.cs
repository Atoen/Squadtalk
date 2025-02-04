using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Shared;
using Shared.Data.TypedIds;
using Squadtalk.Data.Repositories;
using Squadtalk.Data.TypedIds;
using Squadtalk.Extensions;
using tusdotnet.Interfaces;

namespace Squadtalk.Controllers;

[ApiController]
[Route("api/files")]
public class FileController(FileRepository fileRepository): ControllerBase
{
    [HttpGet("{groupId}/{fileId}/{**slug}")]
    public async Task<IActionResult> DownloadFile(GroupId groupId, TusFileId fileId)
    {
        var cancellationToken = HttpContext.RequestAborted;
        var file = await fileRepository.GetTusFileAsync(groupId, fileId, cancellationToken);

        return file is not null
            ? await SetFileContentDispositionAsync(file, cancellationToken)
            : BadRequest("Invalid file id");
    }

    private async Task<IActionResult> SetFileContentDispositionAsync(ITusFile tusFile, CancellationToken cancellationToken)
    {
        var metadata = await tusFile.GetMetadataAsync(cancellationToken);
        var filename = metadata.GetString(EmbedData.FileName);
        var contentType = metadata.GetString(EmbedData.ContentType);

        var contentDisposition = new ContentDispositionHeaderValue("inline");
        contentDisposition.SetHttpFileName(filename);

        Response.Headers.Append("Content-Disposition", contentDisposition.ToString());

        var stream = await tusFile.GetContentAsync(cancellationToken);
        return File(stream, contentType);
    }
}
