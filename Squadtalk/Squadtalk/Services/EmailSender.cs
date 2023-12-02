using Microsoft.AspNetCore.Identity;
using MimeKit;
using Squadtalk.Data;
using MailKit.Net.Smtp;
using MimeKit.Text;

namespace Squadtalk.Services;

public sealed class EmailSender : IEmailSender<ApplicationUser>, IDisposable
{
    private readonly SmtpClient _client;
    
    private readonly MailboxAddress _sender;
    private readonly string _password;
    private readonly string _host;
    private readonly int _port;

    public EmailSender(SmtpClient client, IConfiguration configuration)
    {
        _client = client;
        
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

    private async Task ConnectAsync()
    {
        await _client.ConnectAsync(_host, _port, true);
        await _client.AuthenticateAsync(_sender.Address, _password);
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

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var message = CreateMessage(user.UserName!, email, "Confirm your email",
            $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

        if (!_client.IsConnected)
        {
            await ConnectAsync();
        }
        
        await _client.SendAsync(message);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var message = CreateMessage(user.UserName!, email, "Reset your password",
            $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

        if (!_client.IsConnected)
        {
            await ConnectAsync();
        }
        
        await _client.SendAsync(message);
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var message = CreateMessage(user.UserName!, email, "Reset your password",
            $"Please reset your password using the following code: {resetCode}");

        if (!_client.IsConnected)
        {
            await ConnectAsync();
        }
        
        await _client.SendAsync(message);
    }

    public void Dispose()
    {
        _client.Disconnect(true);
        _client.Dispose();
    }
}