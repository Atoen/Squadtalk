using System.Net;
using Microsoft.AspNetCore.Identity;
using Shared;
using Squadtalk.Data.Entities;
using Squadtalk.Services;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;

namespace Squadtalk.Tus;

public static class TusConfigurationFactory
{
    public static Task<DefaultTusConfiguration> GetConfiguration(HttpContext httpContext)
    {
        var helper = httpContext.RequestServices.GetRequiredService<TusHelper>();

        var config = new DefaultTusConfiguration
        {
            Store = helper.DiskStore,
            Events = new Events
            {
                OnAuthorizeAsync = OnAuthorizeAsync,
                OnBeforeCreateAsync = OnBeforeCreateAsync,
                OnFileCompleteAsync = OnFileCompleteAsync
            }
        };

        return Task.FromResult(config);
    }

    private static readonly string[] RequiredMetadataKeys = [EmbedData.ChannelId, EmbedData.FileName, EmbedData.ContentType];

    private static Task OnAuthorizeAsync(AuthorizeContext authorizeContext)
    {
        //TODO Verify upload channel

        if (authorizeContext.HttpContext.User.Identity is not { IsAuthenticated: true })
        {
            authorizeContext.FailRequest(HttpStatusCode.Unauthorized);
        }

        return Task.CompletedTask;
    }

    private static Task OnBeforeCreateAsync(BeforeCreateContext beforeCreateContext)
    {
        CheckMetadata(beforeCreateContext, RequiredMetadataKeys);
        
        return Task.CompletedTask;
    }

    private static void CheckMetadata(BeforeCreateContext context, params string[] metadataKeys)
    {
        foreach (var key in metadataKeys)
        {
            if (!context.Metadata.TryGetValue(key, out var metadata) || metadata.HasEmptyValue)
            {
                context.FailRequest($"'{key}' metadata must be specified. ");
            }
        }
    }
    
    private static async Task OnFileCompleteAsync(FileCompleteContext fileCompleteContext)
    {
        var httpContext = fileCompleteContext.HttpContext;
        var cancellationToken = httpContext.RequestAborted;

        var userManager = httpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null) return;

        var file = await fileCompleteContext.GetFileAsync();
        var systemMessageService = httpContext.RequestServices.GetRequiredService<SystemMessageService>();
        await systemMessageService.SendFileEmbedMessageAsync(user, file, cancellationToken);
    }
}
