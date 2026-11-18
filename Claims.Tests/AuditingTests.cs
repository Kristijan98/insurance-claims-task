using Claims.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Claims.Tests;

public class AuditingTests
{
    [Fact]
    public void Auditer_QueuesClaimAndCoverEntries()
    {
        var auditer = new Auditer();

        auditer.AuditClaim("claim-1", "POST");
        auditer.AuditCover("cover-1", "DELETE");

        Assert.True(auditer.Reader.TryRead(out var first));
        Assert.Equal("claim-1", Assert.IsType<ClaimAudit>(first).ClaimId);
        Assert.True(auditer.Reader.TryRead(out var second));
        Assert.Equal("cover-1", Assert.IsType<CoverAudit>(second).CoverId);
    }

    [Fact]
    public async Task BackgroundService_SavesQueuedEntries()
    {
        var databaseName = Guid.NewGuid().ToString();
        var provider = new ServiceCollection()
            .AddDbContext<AuditContext>(options => options.UseInMemoryDatabase(databaseName))
            .BuildServiceProvider();
        var auditer = new Auditer();
        var service = new AuditBackgroundService(
            auditer, provider.GetRequiredService<IServiceScopeFactory>(), NullLogger<AuditBackgroundService>.Instance);

        auditer.AuditClaim("claim-1", "POST");
        auditer.AuditCover("cover-1", "POST");
        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuditContext>();
        Assert.Equal("claim-1", Assert.Single(context.ClaimAudits).ClaimId);
        Assert.Equal("cover-1", Assert.Single(context.CoverAudits).CoverId);
    }
}
