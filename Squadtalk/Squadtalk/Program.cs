using System.Net;
using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using tusdotnet;
using tusdotnet.Helpers;
using Squadtalk.Client.Pages;
using Squadtalk.Components;
using Squadtalk.Components.Account;
using Squadtalk.Data;
using Squadtalk.Extensions;
using Squadtalk.Hubs;
using Squadtalk.Services.Scheduling;
using Squadtalk.Tus;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options => options.Listen(IPAddress.Loopback, 1235));

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

builder.Services.AddServerServices(builder.Environment);

builder.Services.AddSingleton(_ => new RestClient(options =>
    options.BaseUrl = new Uri(builder.Configuration.GetString("Rest:BasePath"))
));

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

app.Services.UseScheduler(scheduler =>
{
    scheduler.Schedule<IDnsRecordUpdater>()
        .EveryFiveSeconds()
        .RunOnceAtStart()
        .PreventOverlapping("dns");
}).OnError(e =>
    {
        var logger = app.Services.GetRequiredService<ILogger<IScheduler>>();
        logger.LogError(e, "Error while running scheduled task");
    }
);

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
    .AddAdditionalAssemblies(typeof(Messages).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapControllers();

app.MapHub<ChatHub>("/chathub", options =>
{
    options.AllowStatefulReconnects = true;
});

app.MapTus("/Upload", TusConfigurationFactory.GetConfiguration);

app.Run();