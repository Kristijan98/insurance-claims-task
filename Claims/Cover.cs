using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Claims;

/// <summary>
/// An insurance cover for a vessel over a fixed period.
/// </summary>
public class Cover : IValidatableObject
{
    /// <summary>
    /// Unique id, assigned by the server.
    /// </summary>
    [BsonId]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// First day of the cover. Cannot be in the past.
    /// </summary>
    [BsonElement("startDate")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Last day of the cover. At most one year after <see cref="StartDate"/>.
    /// </summary>
    [BsonElement("endDate")]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Type of vessel being covered.
    /// </summary>
    [BsonElement("claimType")]
    public CoverType Type { get; set; }

    /// <summary>
    /// Premium for the whole period, calculated by the server.
    /// </summary>
    [BsonElement("premium")]
    public decimal Premium { get; set; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.Date < DateTime.UtcNow.Date)
        {
            yield return new ValidationResult("StartDate cannot be in the past.", [nameof(StartDate)]);
        }

        if (EndDate < StartDate)
        {
            yield return new ValidationResult("EndDate cannot be before StartDate.", [nameof(EndDate)]);
        }

        if (EndDate > StartDate.AddYears(1))
        {
            yield return new ValidationResult("Total insurance period cannot exceed 1 year.", [nameof(EndDate)]);
        }
    }
}

/// <summary>
/// Type of vessel a cover applies to.
/// </summary>
public enum CoverType
{
    Yacht = 0,
    PassengerShip = 1,
    ContainerShip = 2,
    BulkCarrier = 3,
    Tanker = 4
}
