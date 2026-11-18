using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

/// <summary>
/// REST endpoints for insurance covers.
/// </summary>
[ApiController]
[Route("[controller]")]
public class CoversController : ControllerBase
{
    private readonly CoversService _coversService;

    /// <summary>
    /// Creates the controller.
    /// </summary>
    public CoversController(CoversService coversService)
    {
        _coversService = coversService;
    }

    /// <summary>
    /// Calculates the premium for the given period and cover type without saving anything.
    /// </summary>
    [HttpPost("compute")]
    public ActionResult<decimal> ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        return Ok(PremiumCalculator.Compute(startDate, endDate, coverType));
    }

    /// <summary>
    /// Returns all covers.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cover>>> GetAsync()
    {
        return Ok(await _coversService.GetAllAsync());
    }

    /// <summary>
    /// Returns a single cover by id.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Cover>> GetAsync(string id)
    {
        var cover = await _coversService.GetByIdAsync(id);
        return cover is null ? NotFound() : Ok(cover);
    }

    /// <summary>
    /// Creates a new cover and calculates its premium.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Cover>> CreateAsync(Cover cover)
    {
        return Ok(await _coversService.CreateAsync(cover));
    }

    /// <summary>
    /// Deletes a cover by id.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task DeleteAsync(string id)
    {
        await _coversService.DeleteAsync(id);
    }
}
