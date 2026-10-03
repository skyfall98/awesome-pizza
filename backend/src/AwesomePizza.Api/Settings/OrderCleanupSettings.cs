namespace AwesomePizza.Api.Settings;

public class OrderCleanupSettings
{
    public const string SectionName = "OrderCleanup";

    public int RetentionDays { get; init; } = 7;
    public int IntervalHours { get; init; } = 24;
}
