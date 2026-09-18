using Claims.Application.Abstractions;

namespace Claims.Tests.Fakes;

public sealed class RecordingAuditPublisher : IAuditPublisher
{
    public List<(string Kind, string Id, string HttpRequestType)> Items { get; } = [];

    public ValueTask PublishClaimAsync(string claimId, string httpRequestType, CancellationToken cancellationToken)
    {
        Items.Add(("Claim", claimId, httpRequestType));
        return ValueTask.CompletedTask;
    }

    public ValueTask PublishCoverAsync(string coverId, string httpRequestType, CancellationToken cancellationToken)
    {
        Items.Add(("Cover", coverId, httpRequestType));
        return ValueTask.CompletedTask;
    }
}
