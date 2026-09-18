namespace Claims.Application.Abstractions;

public interface IAuditPublisher
{
    ValueTask PublishClaimAsync(string claimId, string httpRequestType, CancellationToken cancellationToken);
    ValueTask PublishCoverAsync(string coverId, string httpRequestType, CancellationToken cancellationToken);
}
