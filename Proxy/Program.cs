using System.Net;
using Proxy;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options =>
{
    options.Listen(IPAddress.Any, 1230, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
        context.RequestTransforms.Add(new CookieTransform());
    });

var app = builder.Build();

app.MapReverseProxy();

app.Run();