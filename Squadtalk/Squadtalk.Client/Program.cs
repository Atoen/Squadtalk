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

builder.Services.AddSingleton<ICommunicationManager, CommunicationManager>();
builder.Services.AddSingleton<ISignalrService, SignalRService>();

builder.Services.AddSingleton<IMessageService, MessageService>();
builder.Services.AddSingleton<IMessageModelService<MessageDto>, MessageModelService<MessageDto>>();
builder.Services.AddSingleton<IMessageModelMapper<MessageDto>, DtoMessageModelMapper>();

await builder.Build().RunAsync();
