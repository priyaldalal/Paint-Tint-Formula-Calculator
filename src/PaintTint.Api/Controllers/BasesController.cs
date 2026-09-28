using Microsoft.AspNetCore.Mvc;
using PaintTint.Core.DTOs;
using PaintTint.Core.Interfaces;

namespace PaintTint.Api.Controllers;

/// <summary>
/// API controller for querying available paint base types, tint limits, and pricing.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BasesController : ControllerBase
{
    private readonly IBaseService _baseService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasesController"/> class.
    /// </summary>
    /// <param name="baseService">The base query service.</param>
    public BasesController(IBaseService baseService)
    {
        _baseService = baseService;
    }

    /// <summary>
    /// List all paint bases (Pastel, Medium, Deep) with tint limits and price per litre.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of base types.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<BaseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BaseDto>>> GetBases(CancellationToken cancellationToken)
    {
        var bases = await _baseService.GetBasesAsync(cancellationToken);
        return Ok(bases);
    }
}
