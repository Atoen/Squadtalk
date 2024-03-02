using Blazored.LocalStorage;
using Coravel;
using MailKit.Net.Smtp;
using MessagePack;
using Microsoft.AspNetCore.Identity;
using Polly.Registry;
using Shared.Services;
using Squadtalk.Client.Services;
using Squadtalk.Data;
using Squadtalk.Services;
using Squadtalk.Services.Scheduling;

namespace Squadtalk.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServerServices(this IServiceCollection serviceCollection, IWebHostEnvironment environment)
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
        serviceCollection.AddSingleton<ChatConnectionManager<ApplicationUser, string>>();
        serviceCollection.AddSingleton<IConnectionKeyAccessor<ApplicationUser, string>, ConnectionKeyAccessor>();
        serviceCollection.AddSingleton<VoiceCallManager>();
        
        serviceCollection.AddScoped<MessageStorageService>();
        serviceCollection.AddScoped<IMessageService, ServersideMessagesService>();
        serviceCollection.AddScoped<IMessageModelService<Message>, MessageModelService<Message>>();
        serviceCollection.AddScoped<IMessageModelMapper<Message>, MessageModelMapper>();
        serviceCollection.AddScoped<ICommunicationManager, CommunicationManager>();
        serviceCollection.AddScoped<ISignalrService, ServersideSignalrService>();
        serviceCollection.AddScoped<IVoiceChatService, ServerSideVoice>();
        serviceCollection.AddScoped<IChatVisibilityManager, ChatVisibilityManager>();
        serviceCollection.AddScoped<IFileTransferService, FileTransferService>();
        
        serviceCollection.AddScoped<EmbedService>();
        serviceCollection.AddScoped<ImagePreviewGenerator>();
        serviceCollection.AddScoped<TusHelper>();

        return serviceCollection;
    }
}