using Claims.Application.Abstractions;
using Claims.Application.Covers.Models;
using Claims.Application.Exceptions;
using Claims.Domain.Entities;
using Claims.Domain.Enums;
using Claims.Domain.Services;

namespace Claims.Application.Covers;

public sealed class CoverService : ICoverService
{
    private readonly ICoverRepository _covers;
    private readonly IPremiumCalculator _premiumCalculator;
    private readonly IAuditPublisher _auditPublisher;
    private readonly IClock _clock;

    public CoverService(
        ICoverRepository covers,
        IPremiumCalculator premiumCalculator,
        IAuditPublisher auditPublisher,
        IClock clock)
    {
        _covers = covers;
        _premiumCalculator = premiumCalculator;
        _auditPublisher = auditPublisher;
        _clock = clock;
    }

    public async Task<IReadOnlyList<CoverDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var covers = await _covers.GetAllAsync(cancellationToken);
        return covers.Select(CoverDto.From).ToList();
    }

    public async Task<CoverDto> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var cover = await _covers.GetByIdAsync(id, cancellationToken)
                    ?? throw new NotFoundException(nameof(Cover), id);

        return CoverDto.From(cover);
    }

    public async Task<CoverDto> CreateAsync(CreateCoverRequest request, CancellationToken cancellationToken)
    {
        CoverValidator.Validate(request, _clock);

        var premium = _premiumCalculator.Compute(request.StartDate, request.EndDate, request.Type);
        var cover = Cover.Create(request.StartDate, request.EndDate, request.Type, premium);

        await _covers.AddAsync(cover, cancellationToken);
        await _auditPublisher.PublishCoverAsync(cover.Id, Consts.Post, cancellationToken);

        return CoverDto.From(cover);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var cover = await _covers.GetByIdAsync(id, cancellationToken)
                    ?? throw new NotFoundException(nameof(Cover), id);

        await _auditPublisher.PublishCoverAsync(cover.Id, Consts.Delete, cancellationToken);
        await _covers.DeleteAsync(cover, cancellationToken);
    }

    public decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType) =>
        _premiumCalculator.Compute(startDate, endDate, coverType);
}
