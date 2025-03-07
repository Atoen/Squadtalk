using System.Text;
using AspNetCore.SignalR.OpenTelemetry;
using Blazored.LocalStorage;
using MailKit.Net.Smtp;
using MessagePack;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using Polly.Registry;
using Quartz;
using Quartz.AspNetCore;
using Shared.Routing;
using Shared.Services;
using Squadtalk.Client.Localization;
using Squadtalk.Client.Services;
using Squadtalk.Components.Account;
using Squadtalk.Configuration;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Data.Repositories;
using Squadtalk.Jobs;
using Squadtalk.Services;
using Squadtalk.Services.Prerender;
using StackExchange.Redis;

namespace Squadtalk.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services
            // .AddTransient<IClaimsTransformation, UserClaimsTransformation>()
            .AddCascadingAuthenticationState()
            .AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

        var authenticationBuilder = builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        });

        authenticationBuilder.AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidIssuer = builder.Configuration["LiveKit:Issuer"],
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["LiveKit:ApiSecret"]!))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = context.Request.Headers.Authorization;
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        authenticationBuilder.AddCookie(IdentityConstants.ApplicationScheme, options =>
        {
            options.LoginPath = new PathString(Routes.Pages.Login);
            options.SlidingExpiration = true;

            options.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync,
                OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments(Routes.Endpoints.ApiBase))
                    {
                        context.Response.StatusCode = 401;
                    }
                    else
                    {
                        context.Response.Redirect(context.RedirectUri);
                    }

                    return Task.CompletedTask;
                }
            };
        });

        authenticationBuilder.AddExternalCookie();
        authenticationBuilder.AddTwoFactorRememberMeCookie();
        authenticationBuilder.AddTwoFactorUserIdCookie();

        return builder;
    }

    public static WebApplicationBuilder ConfigureOpenTelemetry(this WebApplicationBuilder builder)
    {
        // builder.Services.AddOpenTelemetry()
        //     .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
        //     .WithMetrics(metrics =>
        //     {
        //         metrics
        //             .AddAspNetCoreInstrumentation()
        //             .AddHttpClientInstrumentation()
        //             .AddNpgsqlInstrumentation()
        //             .AddEventCountersInstrumentation()
        //             .AddRuntimeInstrumentation()
        //             .AddProcessInstrumentation()
        //             .AddMeter("Squadtalk.GCMetrics");
        //
        //         metrics.AddPrometheusExporter();
        //     })
        //     .WithTracing(tracing =>
        //     {
        //         tracing
        //             .AddHttpClientInstrumentation()
        //             .AddAspNetCoreInstrumentation()
        //             .AddQuartzInstrumentation()
        //             .AddRedisInstrumentation()
        //             .AddEntityFrameworkCoreInstrumentation()
        //             .AddSignalRInstrumentation()
        //             .AddNpgsql();
        //
        //         tracing.AddOtlpExporter();
        //     });

        return builder;
    }

    public static WebApplicationBuilder ConfigureQuartz(this WebApplicationBuilder builder)
    {
        builder.Services.AddQuartz(options =>
        {
            options.AddJob<SendEmailVerificationJob>(configure => configure
                .StoreDurably()
                .WithIdentity(SendEmailVerificationJob.JobName));

            options.AddJob<SendPasswordResetEmailJob>(configure => configure
                .StoreDurably()
                .WithIdentity(SendPasswordResetEmailJob.JobName));

            options.UsePersistentStore(persistentOptions =>
            {
                persistentOptions.UsePostgres(config =>
                {
                    config.ConnectionString = builder.Configuration.GetRequiredConnectionString("Scheduler");
                });

                persistentOptions.UseProperties = true;
                persistentOptions.UseSystemTextJsonSerializer();
            });
        });

        builder.Services.AddQuartzServer(options => options.WaitForJobsToComplete = true);

        return builder;
    }

    public static WebApplicationBuilder ConfigureInfrastructure(this WebApplicationBuilder builder)
    {
        var redisConnectionString = builder.Configuration.GetRequiredConnectionString("Redis");
        var postgresConnectionString = builder.Configuration.GetRequiredConnectionString("Postgres");

        builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));

        builder.Services.AddDbContextPool<ApplicationDbContext>(options => options.UseNpgsql(postgresConnectionString));
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

        builder.Services.AddSignalR()
            .AddMessagePackProtocol(options =>
            {
                options.SerializerOptions = MessagePackSerializerOptions.Standard
                    // .WithCompression(MessagePackCompression.Lz4Block)
                    // .WithCompressionMinLength(256)
                    .WithSecurity(MessagePackSecurity.UntrustedData);
            }).AddStackExchangeRedis(redisConnectionString, options =>
            {
                options.Configuration.DefaultDatabase = 1;
            });

        return builder;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ChatUserRepository>();
        serviceCollection.AddScoped<MessageRepository>();
        serviceCollection.AddScoped<GroupRepository>();
        serviceCollection.AddScoped<FileRepository>();
        serviceCollection.AddScoped<FriendRepository>();

        return serviceCollection;
    }

    public static WebApplicationBuilder AddServerServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        if (builder.Environment.IsDevelopment())
        {
            services.AddSingleton<IEmailSender, NoOpEmailSender>();
        }
        else
        {
            services.Configure<EmailConfiguration>(builder.Configuration.GetRequiredSection("Mail"));
            services.AddSingleton<IEmailSender, EmailSender>();
        }

        services.AddRepositories();

        services.AddTransient<SystemMessageService>();

        services.Configure<TusConfiguration>(builder.Configuration.GetRequiredSection("Tus"));
        services.AddSingleton<TusHelper>();

        services.Configure<RtcConfiguration>(builder.Configuration.GetRequiredSection("Rtc"));

        services.AddBlazoredLocalStorage();
        services.AddMudServices();

        services.AddSingleton<SmtpClient>();
        services.AddSingleton<ResiliencePipelineRegistry<string>>();
        services.AddSingleton<HubConnectionManager>();
        services.AddSingleton<VoiceCallManagerOld>();

        services.AddTransient<LiveKitEventHandler>();
        services.AddSingleton<LiveKitService>();

        services.AddScoped<ITextChatService, TextChatService>();

        services.AddScoped<IChatManager, ChatManager>();
        services.AddScoped<IContactManager, ContactManager>();

        services.AddScoped<ContactTabState>();
        services.AddScoped<ITextChatService, TextChatService>();
        services.AddScoped<IConnectionService, ConnectionService>();
        services.AddScoped<IChannelSorter, ChannelSorter>();

        services.AddScoped<IRtcConnectionService, RtcConnectionService>();
        services.AddScoped<IRtcMediaControlService, RtcMediaControlService>();
        services.AddSingleton<RtcConnectionManager>();

        services.AddScoped<IFileTransferService, FileTransferService>();
        // services.AddScoped<IVoiceChatServiceOld, NoOpVoiceChatServiceOld>();

        services.AddScoped<EmbedService>();
        services.AddScoped<ImagePreviewGenerator>();
        services.AddScoped<TusHelper>();
        services.AddScoped<PrerenderPersistantState>();
        services.AddScoped<IChannelSorter, ChannelSorter>();

        services.AddScoped<IAccountManager, AccountManager>();
        services.AddScoped<IFormValidator, NoOpFormValidator>();
        services.AddScoped<IUserPreferencesService, ServerUserPreferencesService>();
        services.AddScoped<LocalizedText>();
        services.AddSingleton<ITextProviderManager, ServerTextProviderManager>();

        services.AddScoped<IUserAuthenticationService, ServerUserAuthenticationService>();
        services.AddScoped<IMediaQueryService, MediaQueryService>();

        return builder;
    }
}
