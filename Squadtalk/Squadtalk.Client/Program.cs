using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RestSharp;
using Shared.DTOs;
using Shared.Services;
using Squadtalk.Client;
using Squadtalk.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();
builder.Services.AddSingleton(_ => new RestClient(options =>
    options.BaseUrl = new Uri(builder.HostEnvironment.BaseAddress)
));

builder.Services.AddScoped<ICommunicationManager, CommunicationManager>();
builder.Services.AddScoped<ISignalrService, SignalRService>();

builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IMessageModelService<MessageDto>, MessageModelService<MessageDto>>();
builder.Services.AddScoped<IMessageModelMapper<MessageDto>, DtoMessageModelMapper>();
builder.Services.AddScoped<ITabManager, TabManager>();
builder.Services.AddScoped<IFileTransferService, FileTransferService>();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazorBootstrap();

await builder.Build().RunAsync();
