using Microsoft.AspNetCore.Mvc;
using PaintTint.Core.DTOs;
using PaintTint.Core.Services;

namespace PaintTint.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ShadesController : ControllerBase
{
    private readonly IShadeService _shadeService;

    public ShadesController(IShadeService shadeService)
    {
        _shadeService = shadeService;
    }

    /// <summary>
    /// List shades, optionally filtered by name or code.
    /// </summary>
    /// <param name="search">Search text matching shade code or name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of matching shades with hex colors.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ShadeSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ShadeSummaryDto>>> GetShades([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var shades = await _shadeService.GetShadesAsync(search, cancellationToken);
        return Ok(shades);
    }

    /// <summary>
    /// Get shade details with its colorant formulas for each supported base.
    /// </summary>
    /// <param name="id">Shade ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Detailed shade object including base formulas.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ShadeDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShadeDetailDto>> GetShadeById(int id, CancellationToken cancellationToken)
    {
        var shade = await _shadeService.GetShadeByIdAsync(id, cancellationToken);
        if (shade == null)
        {
            return NotFound(new { message = $"Shade with ID {id} was not found." });
        }
        return Ok(shade);
    }
}
