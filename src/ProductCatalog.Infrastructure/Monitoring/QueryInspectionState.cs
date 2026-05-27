namespace ProductCatalog.Infrastructure.Monitoring;

public sealed class QueryInspectionState
{
    private readonly Dictionary<string, int> _selectExecutions = new(StringComparer.Ordinal);
    private readonly HashSet<string> _warnedPatterns = new(StringComparer.Ordinal);

    public QueryPatternObservation Track(string commandText, int duplicateThreshold)
    {
        var normalizedCommand = Normalize(commandText);
        if (string.IsNullOrWhiteSpace(normalizedCommand))
        {
            return QueryPatternObservation.None;
        }

        var executions = _selectExecutions.TryGetValue(normalizedCommand, out var currentExecutions)
            ? currentExecutions + 1
            : 1;

        _selectExecutions[normalizedCommand] = executions;

        var shouldWarn = executions >= duplicateThreshold && _warnedPatterns.Add(normalizedCommand);
        return new QueryPatternObservation(normalizedCommand, executions, shouldWarn);
    }

    internal static string Normalize(string commandText)
    {
        if (string.IsNullOrWhiteSpace(commandText))
        {
            return string.Empty;
        }

        return string.Join(
            ' ',
            commandText
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }
}

public readonly record struct QueryPatternObservation(
    string NormalizedCommand,
    int Executions,
    bool ShouldWarn)
{
    public static QueryPatternObservation None => new(string.Empty, 0, false);
}
