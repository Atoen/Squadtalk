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
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options =>
{
    options.Listen(IPAddress.Loopback, 1234, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
    });
});

builder.WebHost.UseStaticWebAssets();

builder.Services.Configure<KestrelServerOptions>(options =>
    options.Limits.MaxRequestBodySize = 100 * 1000 * 1000
);

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

var connectionStringBuilder = new NpgsqlConnectionStringBuilder
{
    Host = builder.Configuration["Postgres:Host"],
    Port = int.Parse(builder.Configuration["Postgres:Port"]!),
    Username = builder.Configuration["Postgres:Username"],
    Password = builder.Configuration["Postgres:Password"],
    Database = builder.Configuration["Postgres:Database"],
};

var connectionString = connectionStringBuilder.ConnectionString;

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddCheck<TusStoreHealthCheck>("TusStore");

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npsqlOptions => {
        npsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    })
    .EnableDetailedErrors()
);

builder.Services.AddTransient<IHashService, Argon2HashService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<TusDiskStoreHelper>();
builder.Services.AddSingleton<UserManager>();
builder.Services.AddScoped<ChannelService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddTransient<EmbedService>();
builder.Services.AddScoped<IGifSourceVerifier, GifSourceVerifierService>();
builder.Services.AddScoped<IImagePreviewGenerator, ImagePreviewGeneratorService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRateLimiter(limiterOptions => limiterOptions
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));

var app = builder.Build();

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

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blazor API V1");
});


app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapTus("/tus", Tus.TusConfigurationFactory);
app.MapHub<ChatHub>("/chat");

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapRazorPages();

app.MapFallbackToFile("index.html");

app.Run();