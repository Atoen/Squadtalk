using System.Net;
using Blazored.LocalStorage;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polly.Registry;
using RestSharp;
using tusdotnet;
using tusdotnet.Helpers;
using Shared.DTOs;
using Shared.Services;
using Squadtalk.Client.Pages;
using Squadtalk.Client.Services;
using Squadtalk.Components;
using Squadtalk.Components.Account;
using Squadtalk.Data;
using Squadtalk.Hubs;
using Squadtalk.Services;
using Squadtalk.Tus;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options => options.Listen(IPAddress.Any, 1235));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers();

builder.Services.AddResponseCompression();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    }).AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("Postgres")
                       ?? throw new InvalidOperationException("Connection string 'Postgres' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<SmtpClient>();
builder.Services.AddSingleton<ResiliencePipelineRegistry<string>>();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, EmailSender>();
builder.Services.AddSingleton<ChatConnectionManager<UserDto, string>>();
builder.Services.AddSingleton<IConnectionKeyAccessor<UserDto, string>, ConnectionKeyAccessor>();
builder.Services.AddScoped<MessageStorageService>();

builder.Services.AddScoped<IMessageService, ServersideMessagesService>();
builder.Services.AddScoped<IMessageModelService<Message>, MessageModelService<Message>>();
builder.Services.AddScoped<IMessageModelMapper<Message>, MessageModelMapper>();

builder.Services.AddScoped<ICommunicationManager, CommunicationManager>();
builder.Services.AddScoped<ISignalrService, ServersideSignalrService>();
builder.Services.AddScoped<ITabManager, TabManager>();
builder.Services.AddScoped<IFileTransferService, FileTransferService>();
builder.Services.AddScoped<EmbedService>();
builder.Services.AddScoped<ImagePreviewGenerator>();
builder.Services.AddScoped<TusHelper>();

builder.Services.AddSingleton(_ => new RestClient(options =>
    options.BaseUrl = new Uri("localhost:1235")
));

builder.Services.AddSingleton<TusHelper>();

builder.Services.AddSignalR();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazorBootstrap();

const string corsPolicy = "cors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders(CorsHelper.GetExposedHeaders());
    });
});

var app = builder.Build();

app.UseCors(corsPolicy);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseResponseCompression();
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapControllers();

app.MapHub<ChatHub>("/chathub", options =>
{
    options.AllowStatefulReconnects = true;
});

app.MapTus("/Upload", TusConfigurationFactory.GetConfiguration);

app.Run();
