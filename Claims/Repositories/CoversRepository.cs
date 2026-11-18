using Claims.Data;
using Microsoft.EntityFrameworkCore;

namespace Claims.Repositories;

/// <summary>
/// Reads and writes covers in the database.
/// </summary>
public class CoversRepository
{
    private readonly ClaimsContext _context;

    /// <summary>
    /// Creates the repository.
    /// </summary>
    public CoversRepository(ClaimsContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns all covers.
    /// </summary>
    public async Task<IReadOnlyList<Cover>> GetAllAsync()
    {
        return await _context.Covers.ToListAsync();
    }

    /// <summary>
    /// Returns the cover with the given id, or <c>null</c> if it does not exist.
    /// </summary>
    public async Task<Cover?> GetByIdAsync(string id)
    {
        return await _context.Covers.SingleOrDefaultAsync(cover => cover.Id == id);
    }

    /// <summary>
    /// Saves a new cover.
    /// </summary>
    public async Task AddAsync(Cover cover)
    {
        _context.Covers.Add(cover);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes the cover with the given id. Does nothing if it does not exist.
    /// </summary>
    public async Task DeleteAsync(string id)
    {
        var cover = await GetByIdAsync(id);
        if (cover is null)
        {
            return;
        }

        _context.Covers.Remove(cover);
        await _context.SaveChangesAsync();
    }
}
