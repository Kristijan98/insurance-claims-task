using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

/// <summary>
/// REST endpoints for insurance claims.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ClaimsController : ControllerBase
{
	private readonly ClaimsService _claimsService;

	/// <summary>
	/// Creates the controller.
	/// </summary>
	public ClaimsController(ClaimsService claimsService)
	{
		_claimsService = claimsService;
	}

	/// <summary>
	/// Returns all claims.
	/// </summary>
	[HttpGet]
	public async Task<IEnumerable<Claim>> GetAsync()
	{
		return await _claimsService.GetAllAsync();
	}

	/// <summary>
	/// Returns a single claim by id.
	/// </summary>
	[HttpGet("{id}")]
	public async Task<ActionResult<Claim>> GetAsync(string id)
	{
		var claim = await _claimsService.GetByIdAsync(id);
		return claim is null ? NotFound() : Ok(claim);
	}

	/// <summary>
	/// Creates a new claim. Returns 400 if the claim breaks a validation rule.
	/// </summary>
	[HttpPost]
	public async Task<ActionResult<Claim>> CreateAsync(Claim claim)
	{
		try
		{
			return Ok(await _claimsService.CreateAsync(claim));
		}
		catch (ValidationException ex)
		{
			return ValidationProblem(ex.Message);
		}
	}

	/// <summary>
	/// Deletes a claim by id.
	/// </summary>
	[HttpDelete("{id}")]
	public async Task DeleteAsync(string id)
	{
		await _claimsService.DeleteAsync(id);
	}
}
