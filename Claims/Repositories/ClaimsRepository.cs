using Claims.Data;
using Microsoft.EntityFrameworkCore;

namespace Claims.Repositories;

/// <summary>
/// Reads and writes claims in the database.
/// </summary>
public class ClaimsRepository
{
    private readonly ClaimsContext _context;

    /// <summary>
    /// Creates the repository.
    /// </summary>
    public ClaimsRepository(ClaimsContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns all claims.
    /// </summary>
    public async Task<IReadOnlyList<Claim>> GetAllAsync()
    {
        return await _context.Claims.ToListAsync();
    }

    /// <summary>
    /// Returns the claim with the given id, or <c>null</c> if it does not exist.
    /// </summary>
    public async Task<Claim?> GetByIdAsync(string id)
    {
        return await _context.Claims.SingleOrDefaultAsync(claim => claim.Id == id);
    }

    /// <summary>
    /// Saves a new claim.
    /// </summary>
    public async Task AddAsync(Claim claim)
    {
        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes the claim with the given id. Does nothing if it does not exist.
    /// </summary>
    public async Task DeleteAsync(string id)
    {
        var claim = await GetByIdAsync(id);
        if (claim is null)
        {
            return;
        }

        _context.Claims.Remove(claim);
        await _context.SaveChangesAsync();
    }
}
