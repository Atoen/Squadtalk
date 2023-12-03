using Microsoft.AspNetCore.Identity;
using MimeKit;
using Squadtalk.Data;
using MailKit.Net.Smtp;
using MimeKit.Text;
using Polly;
using Polly.Registry;
using Polly.Retry;

namespace Squadtalk.Services;

public sealed class EmailSender : IEmailSender<ApplicationUser>, IDisposable
{
    private readonly SmtpClient _client;
    private readonly ILogger<EmailSender> _logger;
    private readonly ResiliencePipelineRegistry<string> _registry;

    private readonly MailboxAddress _sender;
    private readonly string _password;
    private readonly string _host;
    private readonly int _port;

    public EmailSender(SmtpClient client, IConfiguration configuration, ILogger<EmailSender> logger, ResiliencePipelineRegistry<string> registry)
    {
        _client = client;
        _logger = logger;
        _registry = registry;

        var senderName = configuration["Mail:Username"];
        var senderAddress = configuration["Mail:Address"];
        
        ArgumentException.ThrowIfNullOrWhiteSpace(senderName);
        ArgumentException.ThrowIfNullOrWhiteSpace(senderAddress);
        
        _sender = new MailboxAddress(senderName, senderAddress);
        
        _password = configuration["Mail:Password"]!;
        _host = configuration["Mail:Host"]!;
        
        ArgumentException.ThrowIfNullOrWhiteSpace(_password);
        ArgumentException.ThrowIfNullOrWhiteSpace(_host);
        
        _port = Convert.ToInt32(configuration["Mail:Port"]);
    }
    
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var message = CreateMessage(user.UserName!, email, "Confirm your email",
            $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

        return TrySendAsync(message, email);
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var message = CreateMessage(user.UserName!, email, "Reset your password",
            $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

        return TrySendAsync(message, email);
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var message = CreateMessage(user.UserName!, email, "Reset your password",
            $"Please reset your password using the following code: {resetCode}");

        return TrySendAsync(message, email);
    }

    private async Task ConnectAsync()
    {
        await _client.ConnectAsync(_host, _port, true);
        await _client.AuthenticateAsync(_sender.Address, _password);
    }

    private async Task TrySendAsync(MimeMessage message, string address)
    {
        var pipeline = _registry.GetOrAddPipeline("smtp", builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<SmtpCommandException>(),
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(4),
                UseJitter = true,
                MaxRetryAttempts = 3
            });
        });

        try
        {
            await pipeline.ExecuteAsync((msg, _) => SendAsync(msg), message);
            _logger.LogInformation("Successfully sent email to {Address}, subject: '{Subject}'",
                address, message.Subject);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e,"Failed to send email to {Address}", address);
        }
    }

    private async ValueTask SendAsync(MimeMessage message)
    {
        if (!_client.IsConnected)
        {
            await ConnectAsync();
        }

        await _client.SendAsync(message);
    }

    private MimeMessage CreateMessage(string username, string email, string subject, string body) => new()
    {
        From = { _sender },
        To = { new MailboxAddress(username, email) },
        Subject = subject,
        Body = new TextPart(TextFormat.Html)
        {
            Text = body
        }
    };

    public void Dispose()
    {
        _client.Disconnect(true);
        _client.Dispose();
    }
}