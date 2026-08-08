using Microsoft.EntityFrameworkCore;

namespace KejaHUnt_PropertiesAPI.Models.Domain;

/// <summary>
/// Price-per-unit of water for a property. Versioned rather than mutated in place:
/// setting a new rate deactivates the old one instead of overwriting it, so historical
/// bills remain traceable to the rate that was active when they were generated.
/// </summary>
public class WaterRate
{
    public long Id { get; set; }

    public long PropertyId { get; set; }
    public Property Property { get; set; }

    [Precision(18, 2)]
    public decimal PricePerUnit { get; set; }

    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}