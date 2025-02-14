using Quartz;
using Squadtalk.Services;

namespace Squadtalk.Jobs;

internal sealed class SendEmailVerificationJob(
    IEmailSender emailSender,
    ILogger<SendEmailVerificationJob> logger) : IJob
{
    internal const string JobName = nameof(SendEmailVerificationJob);

    internal const string RecipientEmail = nameof(RecipientEmail);
    internal const string RecipientUsername = nameof(RecipientUsername);
    internal const string VerificationLink = nameof(VerificationLink);

    public async Task Execute(IJobExecutionContext context)
    {
        var jobData = context.MergedJobDataMap;

        var email = jobData.GetString(RecipientEmail);
        var username = jobData.GetString(RecipientUsername);
        var verificationLink = jobData.GetString(VerificationLink);

        if (email is null || username is null || verificationLink is null)
        {
            logger.LogError("Job data is invalid. Email: {Email}, username: {Username}, verification link: {Link}",
                email, username, verificationLink);
            return;
        }

        logger.LogDebug("Sending email to {Email}...", email);

        await emailSender.SendConfirmationLinkAsync(username, email, verificationLink);
    }
}
