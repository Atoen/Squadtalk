using Microsoft.AspNetCore.Identity;
using Squadtalk.Data.Entities;

namespace Squadtalk.Services;

public class NoOpEmailSender(ILogger<NoOpEmailSender> logger) : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        logger.LogInformation(
            "NoOpEmailSender: SendConfirmationLinkAsync called for user {UserId} with email {Email}. Confirmation link: {ConfirmationLink}",
            user.Id, email, confirmationLink);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        logger.LogInformation(
            "NoOpEmailSender: SendPasswordResetLinkAsync called for user {UserId} with email {Email}. Reset link: {ResetLink}",
            user.Id, email, resetLink);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        logger.LogInformation(
            "NoOpEmailSender: SendPasswordResetCodeAsync called for user {UserId} with email {Email}. Reset code: {ResetCode}",
            user.Id, email, resetCode);

        return Task.CompletedTask;
    }
}