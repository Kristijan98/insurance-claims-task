using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Claims
{
    /// <summary>
    /// A claim made against a cover.
    /// </summary>
    public class Claim
    {
        /// <summary>
        /// Maximum damage cost a single claim may have.
        /// </summary>
        public const double MaxDamageCost = 100_000;

        /// <summary>
        /// Unique id, assigned by the server.
        /// </summary>
        [BsonId]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Id of the cover this claim belongs to.
        /// </summary>
        [Required]
        [BsonElement("coverId")]
        public string CoverId { get; set; } = string.Empty;

        /// <summary>
        /// Date the claim was created. Must fall within the cover period.
        /// </summary>
        [BsonElement("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Name of the claim.
        /// </summary>
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Kind of damage being claimed.
        /// </summary>
        [BsonElement("claimType")]
        public ClaimType Type { get; set; }

        /// <summary>
        /// Cost of the damage. Cannot exceed <see cref="MaxDamageCost"/>.
        /// </summary>
        [Range(0, MaxDamageCost)]
        [BsonElement("damageCost")]
        public decimal DamageCost { get; set; }
    }

    /// <summary>
    /// Kind of damage a claim covers.
    /// </summary>
    public enum ClaimType
    {
        Collision = 0,
        Grounding = 1,
        BadWeather = 2,
        Fire = 3
    }
}
