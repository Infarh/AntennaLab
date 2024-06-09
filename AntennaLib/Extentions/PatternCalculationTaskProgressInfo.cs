// ReSharper disable once CheckNamespace
namespace Antennas;

public readonly struct PatternCalculationTaskProgressInfo(double Progress, PatternValue Value)
{
    public double Progress { get; } = Progress;
    public PatternValue Value { get; } = Value;
}