namespace Claims.Infrastructure.Auditing;

public sealed class AuditJobs
{
    private readonly AuditContext _context;

    public AuditJobs(AuditContext context)
    {
        _context = context;
    }

    public async Task PublishClaimAsync(string claimId, string httpRequestType, DateTime dateTimeStamp)
    {
        _context.ClaimAudits.Add(new ClaimAudit
        {
            ClaimId = claimId,
            HttpRequestType = httpRequestType,
            Created = dateTimeStamp
        });

        await _context.SaveChangesAsync();
    }

    public async Task PublishCoverAsync(string coverId, string httpRequestType, DateTime dateTimeStamp)
    {
        _context.CoverAudits.Add(new CoverAudit
        {
            CoverId = coverId,
            HttpRequestType = httpRequestType,
            Created = dateTimeStamp
        });

        await _context.SaveChangesAsync();
    }
}
