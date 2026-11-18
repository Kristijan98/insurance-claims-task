using Claims.Auditing;
using Claims.Data;
using Claims.Repositories;
using Claims.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Claims.Tests;

public class ServiceTests
{
    private static readonly DateTime CoverStart = new(2030, 1, 1);
    private static readonly DateTime CoverEnd = new(2030, 6, 30);

    private readonly ClaimsContext _context = new(
        new DbContextOptionsBuilder<ClaimsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private readonly Auditer _auditer = new();

    private ClaimsService CreateClaimsService() =>
        new(new ClaimsRepository(_context), new CoversRepository(_context), _auditer);

    private CoversService CreateCoversService() =>
        new(new CoversRepository(_context), _auditer);

    private async Task<Cover> AddCoverAsync()
    {
        var cover = new Cover { Id = "cover-1", StartDate = CoverStart, EndDate = CoverEnd, Type = CoverType.Yacht };
        _context.Covers.Add(cover);
        await _context.SaveChangesAsync();
        return cover;
    }

    [Fact]
    public async Task CreateClaim_CoverDoesNotExist_Throws()
    {
        var service = CreateClaimsService();
        var claim = new Claim { CoverId = "missing", Created = CoverStart };

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(claim));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(181)]
    public async Task CreateClaim_CreatedOutsideCoverPeriod_Throws(int daysAfterStart)
    {
        await AddCoverAsync();
        var service = CreateClaimsService();
        var claim = new Claim { CoverId = "cover-1", Created = CoverStart.AddDays(daysAfterStart) };

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(claim));
        Assert.False(_auditer.Reader.TryRead(out _));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateClaim_CreatedOnCoverBoundary_SavesAndAudits(bool onEndDate)
    {
        await AddCoverAsync();
        var service = CreateClaimsService();
        var claim = new Claim { CoverId = "cover-1", Created = onEndDate ? CoverEnd : CoverStart };

        var created = await service.CreateAsync(claim);

        Assert.NotNull(await service.GetByIdAsync(created.Id));
        Assert.True(_auditer.Reader.TryRead(out var entry));
        var audit = Assert.IsType<ClaimAudit>(entry);
        Assert.Equal(created.Id, audit.ClaimId);
        Assert.Equal("POST", audit.HttpRequestType);
    }

    [Fact]
    public async Task CreateCover_ComputesPremiumSavesAndAudits()
    {
        var service = CreateCoversService();
        var cover = new Cover { StartDate = CoverStart, EndDate = CoverStart.AddDays(10), Type = CoverType.Tanker };

        var created = await service.CreateAsync(cover);

        Assert.Equal(PremiumCalculator.Compute(cover.StartDate, cover.EndDate, cover.Type), created.Premium);
        Assert.NotNull(await service.GetByIdAsync(created.Id));
        Assert.True(_auditer.Reader.TryRead(out var entry));
        var audit = Assert.IsType<CoverAudit>(entry);
        Assert.Equal(created.Id, audit.CoverId);
        Assert.Equal("POST", audit.HttpRequestType);
    }

    [Fact]
    public async Task DeleteCover_RemovesAndAudits()
    {
        await AddCoverAsync();
        var service = CreateCoversService();

        await service.DeleteAsync("cover-1");

        Assert.Null(await service.GetByIdAsync("cover-1"));
        Assert.True(_auditer.Reader.TryRead(out var entry));
        var audit = Assert.IsType<CoverAudit>(entry);
        Assert.Equal("cover-1", audit.CoverId);
        Assert.Equal("DELETE", audit.HttpRequestType);
    }
}
