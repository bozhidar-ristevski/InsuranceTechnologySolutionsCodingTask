namespace Claims.Infrastructure.Auditing;

public sealed class CoverAudit
{
    public int Id { get; set; }
    public string CoverId { get; set; } = null!;
    public DateTime Created { get; set; }
    public string HttpRequestType { get; set; } = null!;
}
