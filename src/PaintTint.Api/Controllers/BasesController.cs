using Microsoft.AspNetCore.Mvc;
using PaintTint.Core.DTOs;
using PaintTint.Core.Services;

namespace PaintTint.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BasesController : ControllerBase
{
    private readonly IBaseService _baseService;

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
