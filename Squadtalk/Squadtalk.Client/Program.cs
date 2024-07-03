using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RestSharp;
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

builder.Services.AddScoped<ITextChatService, TextChatService>();
builder.Services.AddScoped<ISignalrService, SignalrService>();
builder.Services.AddScoped<ICommunicationService, CommunicationService>();
builder.Services.AddScoped<ICreateTextChannelRequestHandler, CreateTextChannelRequestHandler>();

builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IMessageModelService, MessageModelService>();
builder.Services.AddScoped<IMessagePageProvider, HttpMessagePageProvider>();

builder.Services.AddScoped<IChatVisibilityManager, ChatVisibilityManager>();
builder.Services.AddScoped<IFileTransferService, FileTransferService>();
builder.Services.AddScoped<IVoiceChatService, VoiceChatService>();

builder.Services.AddScoped<ILiveKitService, LiveKitService>();
builder.Services.AddScoped<INewVoiceChatService, NewVoiceChatService>();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazorBootstrap();

await builder.Build().RunAsync();
