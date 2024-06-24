using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Shared;
using Shared.Data.TypedIds;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Extensions;
using Squadtalk.Hubs;
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

    private static readonly string[] RequiredMetadataKeys = [FileData.ChannelId, FileData.FileName, FileData.ContentType];

    private static Task OnAuthorizeAsync(AuthorizeContext authorizeContext)
    {
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

        var file = await fileCompleteContext.GetFileAsync();
        var metadata = await file.GetMetadataAsync(cancellationToken);
        var channelId = (ChannelId) metadata.GetString(FileData.ChannelId);

        var fileService = httpContext.RequestServices.GetRequiredService<FileStorageService>();
        await fileService.StoreFileAsync(file, channelId);

        var userManager = httpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null) return;
        
        var embedService = httpContext.RequestServices.GetRequiredService<EmbedService>();
        var embed = await embedService.CreateFileEmbedAsync(file, channelId, cancellationToken);
        
        var messageService = httpContext.RequestServices.GetRequiredService<MessageStorageService>();
        var message = messageService.CreateMessage(user, string.Empty, channelId)
            .WithEmbed(embed);
        
        await messageService.StoreMessageAsync(message);

        var chatHub = httpContext.RequestServices.GetRequiredService<IHubContext<ChatHub, IChatClient>>();
        await chatHub.Clients.Group(channelId).ReceiveMessage(message.ToDto());
    }
}