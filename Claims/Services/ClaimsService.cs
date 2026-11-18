using Claims.Auditing;
using Claims.Repositories;

namespace Claims.Services;

/// <summary>
/// Business logic for claims: validation against the related cover, persistence and auditing.
/// </summary>
public class ClaimsService
{
    private readonly ClaimsRepository _claims;
    private readonly CoversRepository _covers;
    private readonly Auditer _auditer;

    /// <summary>
    /// Creates the service.
    /// </summary>
    public ClaimsService(ClaimsRepository claims, CoversRepository covers, Auditer auditer)
    {
        _claims = claims;
        _covers = covers;
        _auditer = auditer;
    }

    /// <summary>
    /// Returns all claims.
    /// </summary>
    public Task<IReadOnlyList<Claim>> GetAllAsync() => _claims.GetAllAsync();

    /// <summary>
    /// Returns the claim with the given id, or <c>null</c> if it does not exist.
    /// </summary>
    public Task<Claim?> GetByIdAsync(string id) => _claims.GetByIdAsync(id);

    /// <summary>
    /// Validates and saves a new claim.
    /// </summary>
    /// <exception cref="ValidationException">
    /// The related cover does not exist, or the claim was created outside the cover period.
    /// </exception>
    public async Task<Claim> CreateAsync(Claim claim)
    {
        var cover = await _covers.GetByIdAsync(claim.CoverId)
            ?? throw new ValidationException($"Cover '{claim.CoverId}' does not exist.");

        if (claim.Created.Date < cover.StartDate.Date || claim.Created.Date > cover.EndDate.Date)
        {
            throw new ValidationException("Created date must be within the period of the related cover.");
        }

        claim.Id = Guid.NewGuid().ToString();
        await _claims.AddAsync(claim);
        _auditer.AuditClaim(claim.Id, "POST");
        return claim;
    }

    /// <summary>
    /// Deletes the claim with the given id.
    /// </summary>
    public async Task DeleteAsync(string id)
    {
        _auditer.AuditClaim(id, "DELETE");
        await _claims.DeleteAsync(id);
    }
}
