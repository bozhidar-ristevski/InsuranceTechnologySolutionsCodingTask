namespace Claims.Application.Abstractions;

public interface IAuditPublisher
{
    ValueTask PublishClaimAsync(string claimId, string httpRequestType, DateTime dateTimeStamp, CancellationToken cancellationToken);
    ValueTask PublishCoverAsync(string coverId, string httpRequestType, DateTime dateTimeStamp, CancellationToken cancellationToken);
}
