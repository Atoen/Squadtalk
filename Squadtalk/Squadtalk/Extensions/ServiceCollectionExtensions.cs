using Blazored.LocalStorage;
using Coravel;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Polly.Registry;
using Shared.DTOs;
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
        
        serviceCollection.AddTransient<IPService>();
        serviceCollection.AddScheduler();
        
        serviceCollection.AddSingleton<TusHelper>();
        serviceCollection.AddSignalR();
        serviceCollection.AddBlazoredLocalStorage();
        serviceCollection.AddBlazorBootstrap();
        
        serviceCollection.AddSingleton<SmtpClient>();
        serviceCollection.AddSingleton<ResiliencePipelineRegistry<string>>();
        serviceCollection.AddSingleton<ChatConnectionManager<UserDto, string>>();
        serviceCollection.AddSingleton<IConnectionKeyAccessor<UserDto, string>, ConnectionKeyAccessor>();
        
        serviceCollection.AddScoped<MessageStorageService>();
        serviceCollection.AddScoped<IMessageService, ServersideMessagesService>();
        serviceCollection.AddScoped<IMessageModelService<Message>, MessageModelService<Message>>();
        serviceCollection.AddScoped<IMessageModelMapper<Message>, MessageModelMapper>();
        serviceCollection.AddScoped<ICommunicationManager, CommunicationManager>();
        serviceCollection.AddScoped<ISignalrService, ServersideSignalrService>();
        serviceCollection.AddScoped<ITabManager, TabManager>();
        serviceCollection.AddScoped<IFileTransferService, FileTransferService>();
        
        serviceCollection.AddScoped<EmbedService>();
        serviceCollection.AddScoped<ImagePreviewGenerator>();
        serviceCollection.AddScoped<TusHelper>();

        return serviceCollection;
    }
}