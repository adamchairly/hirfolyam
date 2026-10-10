namespace Hirfolyam.Application.Options;

public sealed class ImportOptions
{
    public const string SectionName = "Import";

    // Older feed items are skipped; duplicates within the window are recognized anyway.
    public int LookbackHours { get; set; } = 48;
}
