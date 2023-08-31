using System.Net;
using Squadtalk.Server.Health;
using Squadtalk.Server.Hubs;
using Squadtalk.Server.Models;
using Squadtalk.Server.Services;
using Squadtalk.Server.Setup;
using Squadtalk.Shared;
using HealthChecks.UI.Client;
using LiteX.HealthChecks.MariaDB;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using tusdotnet;
using tusdotnet.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => { options.Listen(IPAddress.Any, 80); });

builder.WebHost.UseStaticWebAssets();

builder.Services.Configure<KestrelServerOptions>(options => { options.Limits.MaxRequestBodySize = 30 * 1024 * 1024; });

builder.ConfigureAuthentication();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(IdentityData.AdminPolicyName,
        policyBuilder => { policyBuilder.RequireClaim(IdentityData.AdminClaimName, "true"); });
});

const string corsPolicy = "CorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicy, policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders(CorsHelper.GetExposedHeaders());
    });
});

var connectionString = builder.Configuration.GetConnectionString("MariaDB");

builder.Services.AddHealthChecks()
    .AddMariaDB(connectionString)
    .AddCheck<TusStoreHealthCheck>("TusStore");

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddHttpClient();

var serverVersion = new MySqlServerVersion(connectionString);
builder.Services.AddDbContext<AppDbContext>(optionsBuilder => optionsBuilder
    .UseMySql(connectionString, serverVersion)
    .EnableDetailedErrors()
);

builder.Services.AddTransient<IHashService, Argon2HashService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddSingleton<TusDiskStoreHelper>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<EmbedService>();
builder.Services.AddScoped<IGifSourceVerifier, GifSourceVerifierService>();
builder.Services.AddScoped<ImagePreviewGeneratorService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseCors(corsPolicy);

app.MapHealthChecks("_health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapTus("/tus", Tus.TusConfigurationFactory);

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.MapHub<ChatHub>("/chat");

app.Run();