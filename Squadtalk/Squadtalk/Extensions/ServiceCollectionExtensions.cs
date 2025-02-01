using System.Text;
using Blazored.LocalStorage;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using Polly.Registry;
using Shared.Routing;
using Shared.Services;
using Squadtalk.Client.Localization;
using Squadtalk.Client.Services;
using Squadtalk.Data.Entities;
using Squadtalk.Repositories;
using Squadtalk.Services;
using Squadtalk.Services.Prerender;

namespace Squadtalk.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder ConfigureAuthentication(this WebApplicationBuilder builder)
    {
        var authenticationBuilder = builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        });

        authenticationBuilder.AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidIssuer = builder.Configuration["LiveKit:Issuer"],
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["LiveKit:ApiSecret"]!))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = context.Request.Headers.Authorization;
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        authenticationBuilder.AddCookie(IdentityConstants.ApplicationScheme, options =>
        {
            options.LoginPath = new PathString(Routes.Pages.Login);
            options.SlidingExpiration = true;

            options.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync,
                OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments(Routes.Endpoints.ApiBase))
                    {
                        context.Response.StatusCode = 401;
                    }
                    else
                    {
                        context.Response.Redirect(context.RedirectUri);
                    }

                    return Task.CompletedTask;
                }
            };
        });

        authenticationBuilder.AddExternalCookie();
        authenticationBuilder.AddTwoFactorRememberMeCookie();
        authenticationBuilder.AddTwoFactorUserIdCookie();

        return builder;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ChatUserRepository>();
        serviceCollection.AddScoped<MessageRepository>();
        serviceCollection.AddScoped<GroupRepository>();
        serviceCollection.AddScoped<FileRepository>();
        serviceCollection.AddScoped<FriendRepository>();

        return serviceCollection;
    }

    public static IServiceCollection AddServerServices(this IServiceCollection serviceCollection,
        IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            serviceCollection.AddSingleton<IEmailSender<ApplicationUser>, NoOpEmailSender>();
        }
        else
        {
            serviceCollection.AddSingleton<IEmailSender<ApplicationUser>, EmailSender>();
        }

        serviceCollection.AddRepositories();

        serviceCollection.AddTransient<SystemMessageService>();

        serviceCollection.AddSingleton<TusHelper>();

        serviceCollection.AddBlazoredLocalStorage();
        serviceCollection.AddMudServices();

        serviceCollection.AddSingleton<SmtpClient>();
        serviceCollection.AddSingleton<ResiliencePipelineRegistry<string>>();
        serviceCollection.AddSingleton<HubConnectionManager>();
        serviceCollection.AddSingleton<VoiceCallManager>();

        serviceCollection.AddTransient<LiveKitEventHandler>();
        serviceCollection.AddSingleton<LiveKitService>();

        serviceCollection.AddScoped<ITextChatService, TextChatService>();

        serviceCollection.AddScoped<IChatManager, ChatManager>();
        serviceCollection.AddScoped<IContactManager, ContactManager>();

        serviceCollection.AddScoped<ContactTabState>();
        serviceCollection.AddScoped<ITextChatService, TextChatService>();
        serviceCollection.AddScoped<IConnectionService, ConnectionService>();
        serviceCollection.AddScoped<IChannelSorter, ChannelSorter>();
        serviceCollection.AddScoped<IFileTransferService, FileTransferService>();
        serviceCollection.AddScoped<IVoiceChatService, NoOpVoiceChatService>();

        serviceCollection.AddScoped<EmbedService>();
        serviceCollection.AddScoped<ImagePreviewGenerator>();
        serviceCollection.AddScoped<TusHelper>();
        serviceCollection.AddScoped<PrerenderPersistantState>();
        serviceCollection.AddScoped<IChannelSorter, ChannelSorter>();

        serviceCollection.AddScoped<IAccountManager, AccountManager>();
        serviceCollection.AddScoped<IFormValidator, NoOpFormValidator>();
        serviceCollection.AddScoped<IUserPreferencesService, ServerUserPreferencesService>();
        serviceCollection.AddScoped<LocalizedText>();
        serviceCollection.AddSingleton<ITextProviderManager, ServerTextProviderManager>();

        serviceCollection.AddScoped<IUserAuthenticationService, ServerUserAuthenticationService>();
        serviceCollection.AddScoped<IMediaQueryService, MediaQueryService>();

        return serviceCollection;
    }
}
