using ProductCatalog.Infrastructure.Monitoring;

namespace ProductCatalog.IntegrationTests.Monitoring;

public sealed class QueryInspectionStateTests
{
    [Fact]
    public void Track_ShouldWarn_WhenSameSelectRepeatsPastThreshold()
    {
        var state = new QueryInspectionState();

        state.Track("SELECT *   FROM Products WHERE Id = @__id_0", 3).ShouldWarn.Should().BeFalse();
        state.Track("SELECT * FROM Products WHERE Id = @__id_0", 3).ShouldWarn.Should().BeFalse();

        var thirdObservation = state.Track("SELECT * FROM Products WHERE Id = @__id_0", 3);

        thirdObservation.ShouldWarn.Should().BeTrue();
        thirdObservation.Executions.Should().Be(3);
        thirdObservation.NormalizedCommand.Should().Be("SELECT * FROM Products WHERE Id = @__id_0");
    }

    [Fact]
    public void Track_ShouldIgnoreBlankCommandText()
    {
        var state = new QueryInspectionState();

        var observation = state.Track("   ", 3);

        observation.ShouldWarn.Should().BeFalse();
        observation.Executions.Should().Be(0);
        observation.NormalizedCommand.Should().BeEmpty();
    }
}
