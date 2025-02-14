using Quartz;
using Squadtalk.Services;

namespace Squadtalk.Jobs;

internal class SendPasswordResetEmailJob(
    IEmailSender emailSender,
    ILogger<SendPasswordResetEmailJob> logger) : IJob
{
    internal const string JobName = nameof(SendPasswordResetEmailJob);

    internal const string RecipientEmail = nameof(RecipientEmail);
    internal const string RecipientUsername = nameof(RecipientUsername);
    internal const string ResetLink = nameof(ResetLink);

    public async Task Execute(IJobExecutionContext context)
    {
        var jobData = context.MergedJobDataMap;

        var email = jobData.GetString(RecipientEmail);
        var username = jobData.GetString(RecipientUsername);
        var resetLink = jobData.GetString(ResetLink);

        if (email is null || username is null || resetLink is null)
        {
            logger.LogError("Job data is invalid. Email: {Email}, username: {Username}, verification link: {Link}",
            email, username, resetLink);
            return;
        }

        logger.LogDebug("Sending email to {Email}...", email);

        await emailSender.SendPasswordResetLinkAsync(username, email, resetLink);
    }
}
