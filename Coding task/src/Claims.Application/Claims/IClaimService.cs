using Claims.Application.Claims.Models;

namespace Claims.Application.Claims;

public interface IClaimService
{
    Task<IReadOnlyList<ClaimDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<ClaimDto> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<ClaimDto> CreateAsync(CreateClaimRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(string id, CancellationToken cancellationToken);
}
