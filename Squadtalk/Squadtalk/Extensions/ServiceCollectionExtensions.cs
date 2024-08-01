using System.Text;
using Blazored.LocalStorage;
using Coravel;
using MailKit.Net.Smtp;
using MessagePack;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Polly.Registry;
using Shared.Services;
using Squadtalk.Client.Localization;
using Squadtalk.Client.Services;
using Squadtalk.Data.Entities;
using Squadtalk.Repositories;
using Squadtalk.Services;
using Squadtalk.Services.Prerender;
using Squadtalk.Services.Scheduling;

namespace Squadtalk.Extensions;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder ConfigureAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        }).AddJwtBearer(options =>
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
        }).AddIdentityCookies();

        return builder;
    }

    public static IServiceCollection AddServerServices(this IServiceCollection serviceCollection,
        IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            serviceCollection.AddSingleton<IDnsRecordUpdater, NoOpDnsUpdate>();
            serviceCollection.AddSingleton<IEmailSender<ApplicationUser>, NoOpEmailSender>();
        }
        else
        {
            serviceCollection.AddSingleton<IDnsRecordUpdater, UpdateDnsRecords>();
            serviceCollection.AddSingleton<IEmailSender<ApplicationUser>, EmailSender>();
        }

        serviceCollection.AddSingleton<DnsRecordUpdaterStateManager>();
        serviceCollection.AddTransient<IPService>();
        serviceCollection.AddTransient<SystemMessageService>();
        serviceCollection.AddScheduler();

        serviceCollection.AddSingleton<TusHelper>();
        serviceCollection.AddSignalR()
            .AddMessagePackProtocol(options =>
            {
                options.SerializerOptions = MessagePackSerializerOptions.Standard
                    .WithCompression(MessagePackCompression.Lz4BlockArray)
                    .WithCompressionMinLength(256)
                    .WithSecurity(MessagePackSecurity.UntrustedData);
            });

        serviceCollection.AddBlazoredLocalStorage();
        serviceCollection.AddBlazorBootstrap();

        serviceCollection.AddSingleton<SmtpClient>();
        serviceCollection.AddSingleton<ResiliencePipelineRegistry<string>>();
        serviceCollection.AddSingleton<ChatConnectionManager>();
        serviceCollection.AddSingleton<VoiceCallManager>();

        serviceCollection.AddTransient<LiveKitEventHandler>();
        serviceCollection.AddSingleton<LiveKitService>();

        serviceCollection.AddScoped<MessageRepository>();
        serviceCollection.AddScoped<UserRepository>();
        serviceCollection.AddScoped<ChannelRepository>();
        serviceCollection.AddScoped<FileRepository>();

        serviceCollection.AddScoped<ITextChatService, TextChatService>();
        serviceCollection.AddScoped<IMessageModelService, MessageModelService>();
        serviceCollection.AddScoped<ChannelCreator>();

        serviceCollection.AddScoped<IChatService, ChatService>();
        serviceCollection.AddScoped<ICommunicationService, LocalCommunicationService>();
        serviceCollection.AddScoped<IChannelManager, ChannelManager>();
        serviceCollection.AddScoped<IFileTransferService, FileTransferService>();
        serviceCollection.AddScoped<IVoiceChatService, VoiceChatService>();

        serviceCollection.AddScoped<EmbedService>();
        serviceCollection.AddScoped<ImagePreviewGenerator>();
        serviceCollection.AddScoped<TusHelper>();
        serviceCollection.AddScoped<UserVolumeManager>();
        serviceCollection.AddScoped<CreateTextChannelRequestHandler>();
        serviceCollection.AddScoped<ILocalizationService, ServerLocalizationService>();
        serviceCollection.AddScoped<ILocalization, Localization>();
        serviceCollection.AddScoped<IUserAuthenticationService, ServerUserAuthenticationService>();
        serviceCollection.AddScoped<IMediaQueryService, MediaQueryService>();

        return serviceCollection;
    }
}
