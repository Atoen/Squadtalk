using System.Net;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Shared;
using Shared.Data.TypedIds;
using Shared.Extensions;
using Squadtalk.Data.Entities;
using Squadtalk.Repositories;
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
        if (authorizeContext.HttpContext.User.Identity is not { IsAuthenticated: true })
        {
            authorizeContext.FailRequest(HttpStatusCode.Unauthorized);
        }

        return Task.CompletedTask;
    }

    private static Task OnBeforeCreateAsync(BeforeCreateContext beforeCreateContext)
    {
        return CheckMetadata(beforeCreateContext, RequiredMetadataKeys);
    }

    private static async Task CheckMetadata(BeforeCreateContext context, params string[] metadataKeys)
    {
        string? channelIdMetadata = null;

        foreach (var key in metadataKeys)
        {
            if (!context.Metadata.TryGetValue(key, out var metadata) || metadata.HasEmptyValue)
            {
                context.FailRequest($"'{key}' metadata must be specified. ");
            }
            else if (key == EmbedData.ChannelId)
            {
                channelIdMetadata = metadata.GetString(Encoding.UTF8);
            }
        }

        if (context.HasFailed || channelIdMetadata is null) return;

        var userId = context.HttpContext.User.GetUserId();
        var channelId = ChannelId.From(channelIdMetadata);

        var channelRepository = context.HttpContext.RequestServices.GetRequiredService<ChannelRepository>();
        var userParticipatesInChannel = await channelRepository.UserParticipatesInChannelAsync(userId, channelId);

        if (!userParticipatesInChannel)
        {
            context.FailRequest(HttpStatusCode.Unauthorized);
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
