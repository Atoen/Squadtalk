using System.Net;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polly.Registry;
using RestSharp;
using Shared.DTOs;
using Shared.Services;
using Squadtalk.Client.Pages;
using Squadtalk.Client.Services;
using Squadtalk.Components;
using Squadtalk.Components.Account;
using Squadtalk.Data;
using Squadtalk.Hubs;
using Squadtalk.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options => options.Listen(IPAddress.Loopback, 1234));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    }).AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Connection string 'Postgres' not found.");
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
builder.Services.AddSingleton<ChatConnectionManager<UserDto>>();
builder.Services.AddScoped<MessageStorageService>();

builder.Services.AddScoped<IMessageService, ServersideMessagesService>();
builder.Services.AddScoped<IMessageModelService<Message>, MessageModelService<Message>>();
builder.Services.AddScoped<IMessageModelMapper<Message>, MessageModelMapper>();

builder.Services.AddScoped<ICommunicationManager, CommunicationManager>();
builder.Services.AddScoped<ISignalrService, ServersideSignalrService>();

builder.Services.AddSingleton(_ => new RestClient(options =>
    options.BaseUrl = new Uri("localhost:1234")
));

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseResponseCompression();
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
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

app.Run();
