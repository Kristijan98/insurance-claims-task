using Claims.Auditing;
using Claims.Repositories;

namespace Claims.Services;

/// <summary>
/// Business logic for covers: premium calculation, persistence and auditing.
/// </summary>
public class CoversService
{
    private readonly CoversRepository _covers;
    private readonly Auditer _auditer;

    /// <summary>
    /// Creates the service.
    /// </summary>
    public CoversService(CoversRepository covers, Auditer auditer)
    {
        _covers = covers;
        _auditer = auditer;
    }

    /// <summary>
    /// Returns all covers.
    /// </summary>
    public Task<IReadOnlyList<Cover>> GetAllAsync() => _covers.GetAllAsync();

    /// <summary>
    /// Returns the cover with the given id, or <c>null</c> if it does not exist.
    /// </summary>
    public Task<Cover?> GetByIdAsync(string id) => _covers.GetByIdAsync(id);

    /// <summary>
    /// Calculates the premium and saves a new cover.
    /// </summary>
    public async Task<Cover> CreateAsync(Cover cover)
    {
        cover.Id = Guid.NewGuid().ToString();
        cover.Premium = PremiumCalculator.Compute(cover.StartDate, cover.EndDate, cover.Type);
        await _covers.AddAsync(cover);
        _auditer.AuditCover(cover.Id, "POST");
        return cover;
    }

    /// <summary>
    /// Deletes the cover with the given id.
    /// </summary>
    public async Task DeleteAsync(string id)
    {
        _auditer.AuditCover(id, "DELETE");
        await _covers.DeleteAsync(id);
    }
}
