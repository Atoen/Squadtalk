using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Shared.Routing;
using Squadtalk.Client.Pages;
using Squadtalk.Components;
using Squadtalk.Extensions;
using Squadtalk.Redis;
using Squadtalk.Services;
using Squadtalk.Signalr;
using Squadtalk.Tus;
using tusdotnet;
using tusdotnet.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options => options.Listen(IPAddress.Loopback, 1235));

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
    };
});

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers();
builder.Services.AddResponseCompression();

builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.All);

builder
    .ConfigureInfrastructure()
    .ConfigureAuthentication()
    .ConfigureOpenTelemetry()
    .ConfigureQuartz()
    .AddServerServices();

builder.Services.AddHttpContextAccessor();

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

app.MapHub<AppHub>("/chathub", options => options.AllowStatefulReconnects = true);

app.MapTus("/Upload", TusConfigurationFactory.GetConfiguration);

// app.MapPrometheusScrapingEndpoint()
//     .RequireHost("localhost");

app.MapFallback(context =>
{
    context.Response.StatusCode = 404;
    if (HttpMethods.IsGet(context.Request.Method) && !context.Request.Path.StartsWithSegments(Routes.Endpoints.ApiBase))
    {
        context.Response.Redirect(Routes.Pages.NotFound);
    }

    return Task.CompletedTask;
});

app.Run();
