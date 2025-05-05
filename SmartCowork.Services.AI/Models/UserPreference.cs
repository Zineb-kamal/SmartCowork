using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

public class UserPreference
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string PreferredSpaceType { get; set; }
    public int? PreferredCapacity { get; set; }
    public DayOfWeek? PreferredDayOfWeek { get; set; }
    public TimeSpan? PreferredStartTime { get; set; }
    public TimeSpan? PreferredDuration { get; set; }

    [NotMapped]
    public Dictionary<string, double> FeaturePreferences { get; set; } = new Dictionary<string, double>();

    // Propriété pour stocker la version sérialisée dans la base de données
    public string FeaturePreferencesJson
    {
        get => FeaturePreferences != null ? JsonSerializer.Serialize(FeaturePreferences) : null;
        set => FeaturePreferences = string.IsNullOrEmpty(value)
                ? new Dictionary<string, double>()
                : JsonSerializer.Deserialize<Dictionary<string, double>>(value);
    }
}