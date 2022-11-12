#nullable enable
// ReSharper disable once CheckNamespace
namespace Antennas;

public readonly struct PatternCalculationTaskProgressInfo
{
    public double Progress { get; }
    public PatternValue Value { get; }

    public PatternCalculationTaskProgressInfo(double Progress, PatternValue Value)
    {
        this.Progress = Progress;
        this.Value    = Value;
    }
}