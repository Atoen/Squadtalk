using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Squadtalk.Server.Services;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Stores;

namespace Squadtalk.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly TusDiskStoreHelper _diskStoreHelper;

    public FileController(TusDiskStoreHelper diskStoreHelper)
    {
        _diskStoreHelper = diskStoreHelper;
    }

    [HttpGet]
    public async Task<ActionResult> DownloadFile([FromQuery] string id)
    {
        // await Task.Delay(TimeSpan.FromSeconds(15));
        
        var store = new TusDiskStore(_diskStoreHelper.Path);

        ITusFile file;
        try
        {
            file = await store.GetFileAsync(id, HttpContext.RequestAborted);
            if (file is null) return BadRequest("Invalid file id");
        }
        catch (TusStoreException e)
        {
            return BadRequest(e.Message);
        }

        var metadata = await file.GetMetadataAsync(HttpContext.RequestAborted);

        var contentType = metadata.TryGetValue("contentType", out var typeMeta)
            ? typeMeta.GetString(Encoding.UTF8)
            : "application/octet-stream";

        if (metadata.TryGetValue("filename", out var nameMeta))
        {
            var name = nameMeta.GetString(Encoding.UTF8);
            var contentDisposition = new ContentDisposition
            {
                FileName = name,
                Inline = true
            };

            Response.Headers.Add("Content-Disposition", contentDisposition.ToString());
        }

        var fileStream = await file.GetContentAsync(HttpContext.RequestAborted);
        return File(fileStream, contentType);
    }
}