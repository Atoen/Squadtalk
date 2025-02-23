using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Shared.Services;
using Squadtalk.Client.Extensions;
using Squadtalk.Client.Localization;
using Squadtalk.Client.Services;
using Squadtalk.Client.Services.Rtc;
using Squadtalk.Client.Services.SignalR;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
// builder.Logging.SetMinimumLevel(LogLevel.Debug); // Global minimum level

// Add Console Logging with a custom format

// Configure specific logging levels
// builder.Logging.AddFilter("Microsoft.AspNetCore.Components.RenderTree.Renderer", LogLevel.Debug);


builder.Services.AddAuthorizationCore();
builder.Services.AddNetworking(builder.HostEnvironment.BaseAddress);

builder.Services.AddScoped<UserAuthenticationService>();

builder.Services.AddScoped<IUserAuthenticationService>(provider =>
    provider.GetRequiredService<UserAuthenticationService>());

builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<UserAuthenticationService>());

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<IChatManager, ChatManager>();
builder.Services.AddScoped<IContactManager, ContactManager>();

builder.Services.AddScoped<SignalrService>();
builder.Services.AddScoped<IConnectionService>(provider =>
    provider.GetRequiredService<SignalrService>());

builder.Services.AddScoped<ITextChatService, TextChatService>();
builder.Services.AddScoped<IMessageModelService, MessageModelService>();
builder.Services.AddScoped<ClientPersistantState>();

builder.Services.AddScoped<IChannelSorter, ChannelSorter>();

builder.Services.AddScoped<VoiceChatInterop>();
builder.Services.AddScoped<IRtcConnectionService, RtcConnectionService>();
builder.Services.AddScoped<IRtcMediaControlService, RtcMediaControlService>();

builder.Services.AddScoped<IFileTransferService, FileTransferService>();
// builder.Services.AddScoped<IVoiceChatServiceOld, VoiceChatServiceOld>();
builder.Services.AddScoped<UserVolumeManager>();

builder.Services.AddScoped<ContactTabState>();
builder.Services.AddScoped<LocalizedText>();
builder.Services.AddScoped<IFormValidator, FormValidator>();
builder.Services.AddScoped<IAccountManager, AccountManager>();

builder.Services.AddScoped<NotificationService>();
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
