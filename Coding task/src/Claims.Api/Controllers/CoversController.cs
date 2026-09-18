using Claims.Application.Covers;
using Claims.Application.Covers.Models;
using Claims.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

/// <summary>
/// HTTP API for insurance covers and premium computation.
/// </summary>
[ApiController]
[Route("[controller]")]
public sealed class CoversController : ControllerBase
{
    private readonly ICoverService _covers;

    public CoversController(ICoverService covers)
    {
        _covers = covers;
    }

    /// <summary>
    /// Computes premium without persisting a cover.
    /// </summary>
    [HttpPost("compute")]
    public ActionResult<decimal> ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        return Ok(_covers.ComputePremium(startDate, endDate, coverType));
    }

    /// <summary>
    /// Returns all covers.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CoverDto>>> GetAsync(CancellationToken cancellationToken)
    {
        return Ok(await _covers.GetAllAsync(cancellationToken));
    }

    /// <summary>
    /// Returns a single cover.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CoverDto>> GetAsync(string id, CancellationToken cancellationToken)
    {
        return Ok(await _covers.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Creates a cover, computes its premium, and stores it.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CoverDto>> CreateAsync(CreateCoverRequest request, CancellationToken cancellationToken)
    {
        var created = await _covers.CreateAsync(request, cancellationToken);
        return Ok(created);
    }

    /// <summary>
    /// Deletes a cover.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        await _covers.DeleteAsync(id, cancellationToken);
        return Ok();
    }
}
