namespace Hirfolyam.Application.Shared;

// The days of the summaries follow the Hungarian calendar, whatever the time zone of the server is.
internal static class BudapestTime
{
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Budapest");

    public static DateOnly GetDay(DateTimeOffset time)
    {
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(time, Zone).DateTime);
    }

    // In UTC, because PostgreSQL accepts only that. Midnight is never skipped or repeated in Budapest.
    public static DateTimeOffset GetStart(DateOnly day)
    {
        var midnight = day.ToDateTime(TimeOnly.MinValue);

        return new DateTimeOffset(midnight, Zone.GetUtcOffset(midnight)).ToUniversalTime();
    }
}
