namespace Claims.Infrastructure.Auditing;

public sealed class ClaimAudit
{
    public int Id { get; set; }
    public string ClaimId { get; set; } = null!;
    public DateTime Created { get; set; }
    public string HttpRequestType { get; set; } = null!;
}
