using Microsoft.AspNetCore.Mvc;
using PaintTint.Core.DTOs;
using PaintTint.Core.Interfaces;

namespace PaintTint.Api.Controllers;

/// <summary>
/// API controller for calculating scaled paint tint formulas, tint percentages, and pricing.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TintController : ControllerBase
{
    private readonly IDispenseService _dispenseService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TintController"/> class.
    /// </summary>
    /// <param name="dispenseService">The dispense and calculation service.</param>
    public TintController(IDispenseService dispenseService)
    {
        _dispenseService = dispenseService;
    }

    /// <summary>
    /// Calculate scaled colorant amounts, tint percentage, and total price for a given shade, base, and can size.
    /// </summary>
    /// <param name="request">ShadeId, BaseId, and CanSizeLitres (1, 4, 10, 20).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Scaled amounts, rounding to 0.05 ml, tint percentage, validation status, and calculated price.</returns>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(CalculateTintResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CalculateTintResponse>> Calculate([FromBody] CalculateTintRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Request body cannot be null." });
        }

        var result = await _dispenseService.CalculateAsync(request, cancellationToken);
        return Ok(result);
    }
}
