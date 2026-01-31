// ReSharper disable once CheckNamespace
namespace Antennas;

/// <summary>Информация о ходе расчёта диаграммы направленности</summary>
public readonly struct PatternCalculationTaskProgressInfo(double Progress, PatternValue Value)
{
    /// <summary>Прогресс расчёта (значение от 0 до 1)</summary>
    public double Progress { get; } = Progress;
    
    /// <summary>Текущее рассчитанное значение диаграммы направленности</summary>
    public PatternValue Value { get; } = Value;
}