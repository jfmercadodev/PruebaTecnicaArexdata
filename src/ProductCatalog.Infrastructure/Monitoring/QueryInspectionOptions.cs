namespace ProductCatalog.Infrastructure.Monitoring;

public sealed class QueryInspectionOptions
{
    public const string SectionName = "PerformanceMonitoring";

    public bool Enabled { get; set; } = true;

    public int SlowQueryThresholdMs { get; set; } = 250;

    public int NPlusOneDuplicateThreshold { get; set; } = 3;
}
