using System.Net;
using Microsoft.AspNetCore.SignalR;
using Squadtalk.Server.Hubs;
using Squadtalk.Server.Models;
using Squadtalk.Server.Services;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Stores;

namespace Squadtalk.Server.Setup;

public class Tus
{
    public static Task<DefaultTusConfiguration> TusConfigurationFactory(HttpContext httpContext)
    {
        var path = httpContext.RequestServices.GetRequiredService<TusDiskStoreHelper>().Path;

        var config = new DefaultTusConfiguration
        {
            Store = new TusDiskStore(path),
            // Expiration = new SlidingExpiration(TimeSpan.FromMinutes(5)),
            // UsePipelinesIfAvailable = true,

            Events = new Events
            {
                OnAuthorizeAsync = AuthorizeHandler,
                OnCreateCompleteAsync = CreateCompleteHandler,
                OnFileCompleteAsync = FileCompleteHandler,
            }
        };

        return Task.FromResult(config);
    }

    private static Task AuthorizeHandler(AuthorizeContext authorizeContext)
    {
        var httpContext = authorizeContext.HttpContext;
        var logger = httpContext.RequestServices.GetRequiredService<ILogger<Tus>>();

        if (httpContext.User.Identity is not { IsAuthenticated: true }) 
        {
            logger.LogInformation("Rejected unauthenticated request from {Remote}", httpContext.Connection.RemoteIpAddress);
            authorizeContext.FailRequest(HttpStatusCode.Unauthorized);
        }

        return Task.CompletedTask;
    }

    private static Task CreateCompleteHandler(CreateCompleteContext completeContext)
    {
        var logger = completeContext.HttpContext.RequestServices.GetRequiredService<ILogger<Tus>>();
        logger.LogInformation("Created file {File}", completeContext.FileId);

        return Task.CompletedTask;
    }

    private static async Task FileCompleteHandler(FileCompleteContext fileContext)
    {
        var httpContext = fileContext.HttpContext;

        var logger = httpContext.RequestServices.GetRequiredService<ILogger<Tus>>();
        logger.LogInformation("Completed file {File}", fileContext.FileId);

        var file = await fileContext.GetFileAsync();
        var embed = await httpContext.RequestServices.GetRequiredService<EmbedService>()
            .CreateEmbed(file, httpContext);

        var userResult = await httpContext.RequestServices.GetRequiredService<UserService>()
            .GetUser(httpContext.User);

        var user = userResult.AsT0;

        var messageService = httpContext.RequestServices.GetRequiredService<MessageService>();
        var hub = httpContext.RequestServices.GetRequiredService<IHubContext<ChatHub>>();

        var message = new Message
        {
            Author = user,
            Timestamp = DateTimeOffset.Now,
            Embed = embed
        };

        await messageService.StoreMessage(message);
        await hub.Clients.All.SendAsync("ReceiveMessage", message.ToDto());
    }
}