using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Refit;
using Shared.Services;
using Squadtalk.Client.Extensions;
using Squadtalk.Client.Localization;
using Squadtalk.Client.Network;
using Squadtalk.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton<UserAuthenticationService>();

builder.Services.AddSingleton<IUserAuthenticationService>(provider =>
    provider.GetRequiredService<UserAuthenticationService>());

builder.Services.AddSingleton<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<UserAuthenticationService>());

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddNetworking(builder.HostEnvironment.BaseAddress);

builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ISignalrService, SignalrService>();
builder.Services.AddScoped<ICommunicationService, CommunicationService>();
builder.Services.AddScoped<CreateTextChannelRequestHandler>();

builder.Services.AddScoped<ITextChatService, TextChatService>();
builder.Services.AddScoped<IMessageModelService, MessageModelService>();
builder.Services.AddScoped<IMessagePageProvider, HttpMessagePageProvider>();

builder.Services.AddScoped<IChannelManager, ChannelManager>();
builder.Services.AddScoped<IFileTransferService, FileTransferService>();
builder.Services.AddScoped<IVoiceChatService, VoiceChatService>();
builder.Services.AddScoped<UserVolumeManager>();

builder.Services.AddScoped<LocalizedText>();
builder.Services.AddScoped<FormValidator>();
builder.Services.AddScoped<IAccountManager, AccountManager>();

builder.Services.AddScoped<ITextProviderManager, BrowserTextProviderManager>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<IMediaQueryService, MediaQueryService>();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;

    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

var host = builder.Build();

await host.RunAsync();
