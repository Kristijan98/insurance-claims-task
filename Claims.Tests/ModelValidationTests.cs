using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Claims.Tests;

public class ModelValidationTests
{
    private static readonly DateTime Tomorrow = DateTime.UtcNow.Date.AddDays(1);

    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Cover_StartDateInPast_Fails()
    {
        var cover = new Cover { StartDate = Tomorrow.AddDays(-2), EndDate = Tomorrow };

        var results = Validate(cover);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Cover.StartDate)));
    }

    [Fact]
    public void Cover_EndDateBeforeStartDate_Fails()
    {
        var cover = new Cover { StartDate = Tomorrow, EndDate = Tomorrow.AddDays(-1) };

        var results = Validate(cover);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Cover.EndDate)));
    }

    [Fact]
    public void Cover_PeriodLongerThanOneYear_Fails()
    {
        var cover = new Cover { StartDate = Tomorrow, EndDate = Tomorrow.AddYears(1).AddDays(1) };

        var results = Validate(cover);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Cover.EndDate)));
    }

    [Fact]
    public void Cover_ExactlyOneYear_Passes()
    {
        var cover = new Cover { StartDate = Tomorrow, EndDate = Tomorrow.AddYears(1) };

        var results = Validate(cover);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData(100_000.01)]
    [InlineData(-1)]
    public void Claim_DamageCostOutOfRange_Fails(decimal damageCost)
    {
        var claim = new Claim { CoverId = "cover-1", DamageCost = damageCost };

        var results = Validate(claim);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Claim.DamageCost)));
    }

    [Fact]
    public void Claim_DamageCostAtLimit_Passes()
    {
        var claim = new Claim { CoverId = "cover-1", DamageCost = 100_000 };

        var results = Validate(claim);

        Assert.Empty(results);
    }

    [Fact]
    public void Claim_MissingCoverId_Fails()
    {
        var claim = new Claim { CoverId = "", DamageCost = 10 };

        var results = Validate(claim);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Claim.CoverId)));
    }
}
