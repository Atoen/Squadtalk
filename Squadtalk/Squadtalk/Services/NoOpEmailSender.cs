namespace Squadtalk.Services;

internal class NoOpEmailSender(ILogger<NoOpEmailSender> logger) : IEmailSender
{
    public Task SendConfirmationLinkAsync(string username, string email, string confirmationLink)
    {
        logger.LogInformation(
            "NoOpEmailSender: SendConfirmationLinkAsync called for user {Username} with email {Email}. Confirmation link: {ConfirmationLink}",
            username, email, confirmationLink);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(string username, string email, string resetLink)
    {
        logger.LogInformation(
            "NoOpEmailSender: SendPasswordResetLinkAsync called for user {Username} with email {Email}. Reset link: {ResetLink}",
            username, email, resetLink);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(string username, string email, string resetCode)
    {
        logger.LogInformation(
            "NoOpEmailSender: SendPasswordResetCodeAsync called for user {Username} with email {Email}. Reset code: {ResetCode}",
            username, email, resetCode);

        return Task.CompletedTask;
    }
}
