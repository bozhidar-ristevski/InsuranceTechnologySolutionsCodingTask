using Claims.Application.Abstractions;
using Claims.Application.Claims.Models;
using Claims.Application.Exceptions;
using Claims.Domain.Entities;

namespace Claims.Application.Claims;

public sealed class ClaimService : IClaimService
{
    private readonly IClaimRepository _claims;
    private readonly ICoverRepository _covers;
    private readonly IAuditPublisher _auditPublisher;
    private readonly IClock _clock;

    public ClaimService(
        IClaimRepository claims,
        ICoverRepository covers,
        IAuditPublisher auditPublisher,
        IClock clock)
    {
        _claims = claims;
        _covers = covers;
        _auditPublisher = auditPublisher;
        _clock = clock;
    }

    public async Task<IReadOnlyList<ClaimDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var claims = await _claims.GetAllAsync(cancellationToken);
        return claims.Select(c => c.ToDto()).ToList();
    }

    public async Task<ClaimDto> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var claim = await _claims.GetByIdAsync(id, cancellationToken)
                    ?? throw new NotFoundException(nameof(Claim), id);

        return claim.ToDto();
    }

    public async Task<ClaimDto> CreateAsync(CreateClaimRequest request, CancellationToken cancellationToken)
    {
        var cover = await _covers.GetByIdAsync(request.CoverId, cancellationToken)
                    ?? throw new NotFoundException(nameof(Cover), request.CoverId);

        ClaimValidator.Validate(request, cover);

        var claim = Claim.Create(
            request.CoverId,
            request.Created,
            request.Name,
            request.Type,
            request.DamageCost);

        await _claims.AddAsync(claim, cancellationToken);
        await _auditPublisher.PublishClaimAsync(claim.Id, Consts.Post, _clock.UtcNow, cancellationToken);

        return claim.ToDto();
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var claim = await _claims.GetByIdAsync(id, cancellationToken)
                    ?? throw new NotFoundException(nameof(Claim), id);

        await _auditPublisher.PublishClaimAsync(claim.Id, Consts.Delete, _clock.UtcNow, cancellationToken);
        await _claims.DeleteAsync(claim, cancellationToken);
    }
}
