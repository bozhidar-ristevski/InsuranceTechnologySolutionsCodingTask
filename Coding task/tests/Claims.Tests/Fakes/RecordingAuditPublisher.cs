using Claims.Application.Abstractions;

namespace Claims.Tests.Fakes;

public sealed class RecordingAuditPublisher : IAuditPublisher
{
    public List<(string Kind, string Id, string HttpRequestType, DateTime DateTimeStamp)> Items { get; } = [];


    public ValueTask PublishClaimAsync(string claimId, string httpRequestType, DateTime dateTimeStamp, CancellationToken cancellationToken)
    {
        Items.Add(("Claim", claimId, httpRequestType, dateTimeStamp));
        return ValueTask.CompletedTask;
    }

    public ValueTask PublishCoverAsync(string coverId, string httpRequestType, DateTime dateTimeStamp, CancellationToken cancellationToken)
    {
        Items.Add(("Cover", coverId, httpRequestType, dateTimeStamp));
        return ValueTask.CompletedTask;
    }
}
