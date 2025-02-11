using System.Net;
using MessagePack;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Routing;
using Shared.Services;
using Squadtalk.Client.Pages;
using Squadtalk.Components;
using Squadtalk.Components.Account;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Extensions;
using Squadtalk.Redis;
using Squadtalk.Services;
using Squadtalk.Signalr;
using Squadtalk.Tus;
using StackExchange.Redis;
using tusdotnet;
using tusdotnet.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options =>
{
    options.Listen(IPAddress.Loopback, 1235);
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers();

builder.Services.AddResponseCompression();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;

    var proxyAddress = IPAddress.Parse(builder.Configuration.GetString("ProxyAddress"));
    options.KnownProxies.Add(proxyAddress);
});

builder.ConfigureAuthentication();

var postgresConnectionString = builder.Configuration.GetRequiredConnectionString("postgres");

builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
    options.UseNpgsql(postgresConnectionString);
});

var redisConnectionString = builder.Configuration.GetRequiredConnectionString("redis");

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;

        options.Password.RequiredLength = IFormValidator.MinimumPasswordLength;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters = IFormValidator.AllowedUsernameChars;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddServerServices(builder.Environment);
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

builder.Services.AddSignalR()
    .AddMessagePackProtocol(options =>
    {
        options.SerializerOptions = MessagePackSerializerOptions.Standard
            .WithCompression(MessagePackCompression.Lz4BlockArray)
            .WithCompressionMinLength(256)
            .WithSecurity(MessagePackSecurity.UntrustedData);
    }).AddStackExchangeRedis(redisConnectionString, options =>
    {
        options.Configuration.DefaultDatabase = 1;
    });

const string corsPolicy = "cors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.WithOrigins("app.squadtalk.net")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders(CorsHelper.GetExposedHeaders());
    });
});

var app = builder.Build();

app.SetupRedisData();

app.UseCors(corsPolicy);
app.UseForwardedHeaders();

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
app.UseUserPreferences();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Chat).Assembly);

app.MapControllers();

app.MapHub<AppHub>("/chathub", options =>
{
    options.AllowStatefulReconnects = true;
});

app.MapTus("/Upload", TusConfigurationFactory.GetConfiguration);

app.MapFallback(context =>
{
    if (HttpMethods.IsGet(context.Request.Method))
    {
        context.Response.Redirect(Routes.Pages.NotFound);
    }

    return Task.CompletedTask;
});

app.Run();
