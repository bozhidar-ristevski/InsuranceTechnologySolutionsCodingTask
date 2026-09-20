using Claims.Application.Abstractions;
using Hangfire;

namespace Claims.Infrastructure.Auditing;

public sealed class HangfireAuditPublisher : IAuditPublisher
{
    private readonly IBackgroundJobClient _backgroundJobs;

    public HangfireAuditPublisher(IBackgroundJobClient backgroundJobs)
    {
        _backgroundJobs = backgroundJobs;
    }

    public ValueTask PublishClaimAsync(string claimId, string httpRequestType, DateTime dateTimeStamp, CancellationToken cancellationToken)
    {
        _backgroundJobs.Enqueue<AuditJobs>(job => job.PublishClaimAsync(claimId, httpRequestType, dateTimeStamp));
        return ValueTask.CompletedTask;
    }

    public ValueTask PublishCoverAsync(string coverId, string httpRequestType, DateTime dateTimeStamp,  CancellationToken cancellationToken)
    {
        _backgroundJobs.Enqueue<AuditJobs>(job => job.PublishCoverAsync(coverId, httpRequestType, dateTimeStamp));
        return ValueTask.CompletedTask;
    }
}
