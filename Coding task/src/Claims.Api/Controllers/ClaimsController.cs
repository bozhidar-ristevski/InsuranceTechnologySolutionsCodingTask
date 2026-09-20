using Claims.Application.Claims;
using Claims.Application.Claims.Models;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

/// <summary>
/// HTTP API for insurance claims.
/// </summary>
[ApiController]
[Route("[controller]")]
public sealed class ClaimsController : ControllerBase
{
    private readonly IClaimService _claims;

    public ClaimsController(IClaimService claims)
    {
        _claims = claims;
    }

    /// <summary>
    /// Returns all claims.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClaimDto>>> GetAsync(CancellationToken cancellationToken)
    {
        return Ok(await _claims.GetAllAsync(cancellationToken));
    }

    /// <summary>
    /// Returns a single claim.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ClaimDto>> GetAsync(string id, CancellationToken cancellationToken)
    {
        return Ok(await _claims.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Creates a claim after validating damage cost and cover period.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ClaimDto>> CreateAsync(CreateClaimRequest request, CancellationToken cancellationToken)
    {
        var created = await _claims.CreateAsync(request, cancellationToken);
        return Created($"/{GetType().Name.Replace("Controller", "")}/{created.Id}", created);
    }

    /// <summary>
    /// Deletes a claim.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        await _claims.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
