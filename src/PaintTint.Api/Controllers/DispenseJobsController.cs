using Microsoft.AspNetCore.Mvc;
using PaintTint.Core.DTOs;
using PaintTint.Core.Services;

namespace PaintTint.Api.Controllers;

[ApiController]
[Route("api/dispense-jobs")]
[Produces("application/json")]
public class DispenseJobsController : ControllerBase
{
    private readonly IDispenseService _dispenseService;

    public DispenseJobsController(IDispenseService dispenseService)
    {
        _dispenseService = dispenseService;
    }

    /// <summary>
    /// Save a dispense job after validating formulas and limits.
    /// </summary>
    /// <param name="request">ShadeId, BaseId, and CanSizeLitres.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Saved dispense job record with generated ID and item breakdown.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(DispenseJobResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DispenseJobResponse>> CreateJob([FromBody] CreateDispenseJobRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Request body cannot be null." });
        }

        var job = await _dispenseService.CreateDispenseJobAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetLatestJob), new { id = job.Id }, job);
    }

    /// <summary>
    /// Get the latest dispense job.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Most recent dispense job.</returns>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(DispenseJobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DispenseJobResponse>> GetLatestJob(CancellationToken cancellationToken)
    {
        var job = await _dispenseService.GetLatestJobAsync(cancellationToken);
        if (job == null)
        {
            return NotFound(new { message = "No dispense jobs have been recorded yet." });
        }
        return Ok(job);
    }

    /// <summary>
    /// Get recent dispense jobs history.
    /// </summary>
    /// <param name="limit">Number of records to fetch (default: 10).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of recent dispense jobs.</returns>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(List<DispenseJobResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DispenseJobResponse>>> GetRecentJobs([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var jobs = await _dispenseService.GetRecentJobsAsync(limit, cancellationToken);
        return Ok(jobs);
    }
}
