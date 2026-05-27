using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProductCatalog.Infrastructure.Monitoring;

public sealed class QueryInspectionInterceptor : DbCommandInterceptor
{
    private readonly ILogger<QueryInspectionInterceptor> _logger;
    private readonly QueryInspectionState _state;
    private readonly QueryInspectionOptions _options;

    public QueryInspectionInterceptor(
        ILogger<QueryInspectionInterceptor> logger,
        QueryInspectionState state,
        IOptions<QueryInspectionOptions> options)
    {
        _logger = logger;
        _state = state;
        _options = options.Value;
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        InspectCommand(command, eventData.Duration);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        InspectCommand(command, eventData.Duration);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        InspectCommand(command, eventData.Duration);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        InspectCommand(command, eventData.Duration);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        InspectCommand(command, eventData.Duration);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        InspectCommand(command, eventData.Duration);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        InspectCommand(command, eventData.Duration, eventData.Exception);
        base.CommandFailed(command, eventData);
    }

    public override Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        InspectCommand(command, eventData.Duration, eventData.Exception);
        return base.CommandFailedAsync(command, eventData, cancellationToken);
    }

    private void InspectCommand(DbCommand command, TimeSpan duration, Exception? exception = null)
    {
        if (!_options.Enabled)
        {
            return;
        }

        var commandText = QueryInspectionState.Normalize(command.CommandText);
        if (string.IsNullOrWhiteSpace(commandText))
        {
            return;
        }

        var elapsedMilliseconds = (long)duration.TotalMilliseconds;
        if (elapsedMilliseconds >= _options.SlowQueryThresholdMs)
        {
            _logger.LogWarning(
                exception,
                "Slow SQL command detected in {ElapsedMs}ms. Command: {CommandText}",
                elapsedMilliseconds,
                commandText);
        }

        if (!commandText.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var pattern = _state.Track(commandText, _options.NPlusOneDuplicateThreshold);
        if (pattern.ShouldWarn)
        {
            _logger.LogWarning(
                "Possible N+1 pattern detected after {Executions} repeated SELECT commands. Command: {CommandText}. Heuristic may raise false positives for intentional polling or loops.",
                pattern.Executions,
                pattern.NormalizedCommand);
        }
    }
}
