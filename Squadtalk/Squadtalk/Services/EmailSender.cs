using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Polly;
using Polly.Registry;
using Polly.Retry;
using Squadtalk.Configuration;

namespace Squadtalk.Services;

public interface IEmailSender
{
    Task SendConfirmationLinkAsync(string username, string email, string confirmationLink);

    Task SendPasswordResetLinkAsync(string username, string email, string resetLink);

    Task SendPasswordResetCodeAsync(string username, string email, string resetCode);
}

internal sealed class EmailSender : IEmailSender, IDisposable
{
    private readonly SmtpClient _client;
    private readonly EmailConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;
    private readonly ResiliencePipelineRegistry<string> _registry;

    private readonly MailboxAddress _sender;

    public EmailSender(SmtpClient client, EmailConfiguration configuration, ILogger<EmailSender> logger, ResiliencePipelineRegistry<string> registry)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
        _registry = registry;

        _sender = new MailboxAddress(configuration.Username, configuration.Address);
    }

    public Task SendConfirmationLinkAsync(string username, string email, string confirmationLink)
    {
        var message = CreateMessage(username, email, "Confirm your email",
            $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

        return TrySendAsync(message, email);
    }

    public Task SendPasswordResetLinkAsync(string username, string email, string resetLink)
    {
        var message = CreateMessage(username, email, "Reset your password",
            $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

        return TrySendAsync(message, email);
    }

    public Task SendPasswordResetCodeAsync(string username, string email, string resetCode)
    {
        var message = CreateMessage(username, email, "Reset your password",
            $"Please reset your password using the following code: {resetCode}");

        return TrySendAsync(message, email);
    }

    private async Task ConnectAsync()
    {
        await _client.ConnectAsync(_configuration.Host, _configuration.Port, true);
        await _client.AuthenticateAsync(_sender.Address, _configuration.Password);
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
            _logger.LogWarning(e, "Failed to send email to {Address}", address);
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

    private MimeMessage CreateMessage(string username, string email, string subject, string body)
    {
        return new MimeMessage
        {
            From = { _sender },
            To = { new MailboxAddress(username, email) },
            Subject = subject,
            Body = new TextPart(TextFormat.Html)
            {
                Text = body
            }
        };
    }

    public void Dispose()
    {
        _client.Disconnect(true);
        _client.Dispose();
    }
}